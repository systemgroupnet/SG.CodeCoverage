using Newtonsoft.Json;
using SG.CodeCoverage.Common;
using SG.CodeCoverage.Metadata;
using SG.CodeCoverage.Recorder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Coverage
{
    public class CoverageResult
    {
        public VersionInfo Version { get; private set; }
        public Guid UniqueId { get; private set; }
        public IReadOnlyCollection<CoverageAssemblyResult> Assemblies { get; private set; }

        public CoverageResult(string mapPath, string hitsPath)
        {
            var map = LoadMapFile(mapPath);
            var (uniqueId, hits) = LoadHits(hitsPath);
            Init(map, uniqueId, hits);
        }

        public CoverageResult(InstrumentationMap map, string hitsPath)
        {
            var (uniqueId, hits) = LoadHits(hitsPath);
            Init(map, uniqueId, hits);
        }

        public CoverageResult(InstrumentationMap map, int[][] hits, Guid hitsUniqueId)
        {
            Init(map, hitsUniqueId, hits);
        }

        public CoverageResult(VersionInfo instrumenterVersion, Guid instrumentUniqueId, IReadOnlyCollection<CoverageAssemblyResult> assemblies)
        {
            Version = instrumenterVersion;
            UniqueId = instrumentUniqueId;
            Assemblies = assemblies;
        }

        private void Init(InstrumentationMap map, Guid uniqueId, int[][] hits)
        {
            if (map.UniqueId != uniqueId)
                throw new Exception($"Hits file's unique id ({uniqueId}) does not match the unique id in the map file ({map.UniqueId}).");

            Version = map.Version;
            UniqueId = uniqueId;
            Assemblies = map.Assemblies.Select(asm => ToAssemblyCoverage(asm, hits)).ToList().AsReadOnly();
        }

        private InstrumentationMap LoadMapFile(string mapFilePath)
        {
            ValidateFilePath(mapFilePath);
            return InstrumentationMap.Parse(mapFilePath);
        }

        private (Guid uniqueId, int[][] hits) LoadHits(string hitsFile)
        {
            ValidateFilePath(hitsFile);
            return HitsFileUtils.LoadHits(hitsFile);
        }

        private CoverageAssemblyResult ToAssemblyCoverage(InstrumentedAssemblyMap assembly, int[][] hits)
        {
            return new CoverageAssemblyResult(
                assembly.Name,
                assembly.Types.Select(type => ToTypeCoverage(type, hits[type.Index])).ToList().AsReadOnly()
            );
        }

        private CoverageTypeResult ToTypeCoverage(InstrumentedTypeMap type, int[] typeHits)
        {
            int visitCount(int index) => typeHits.Length == 0 ? 0 : typeHits[index];
            return new CoverageTypeResult(
                type.FullName,
                type.Methods.Select(method => ToMethodCoverage(method, visitCount(method.Index))).ToList().AsReadOnly()
            );
        }

        private CoverageMethodResult ToMethodCoverage(InstrumentedMethodMap method, int visitCount)
        {
            return new CoverageMethodResult(
                method.FullName,
                method.Source,
                method.StartLine,
                method.StartColumn,
                method.EndLine,
                method.EndColumn,
                visitCount
            );
        }

        public CoverageResult MergeWith(CoverageResult otherResult)
        {
            if (otherResult == null)
                return this;

            // Create an anonymous data that holds every parameter we need to construct the coverage result types
            var result = Assemblies.Select(asm => new
                {
                    asm.Name,
                    Types = asm.Types.Select(type => new
                    {
                        type.FullName,
                        Methods = type.Methods.Select(meth => new
                        {
                            meth.FullName,
                            meth.Source,
                            meth.StartLine,
                            meth.StartColumn,
                            meth.EndLine,
                            meth.EndColumn,
                            meth.VisitCount
                        }).ToList()
                    }).ToList()
                }).ToList()
            ;

            // Update the anonymous data with results from the second coverage result
            foreach(var asm in otherResult.Assemblies)
            {
                var targetAssembly = result.Find(a => a.Name == asm.Name);

                if(targetAssembly == null)
                {
                    // This assembly is not present in the result, add everything about it to the result
                    result.Add(new
                    {
                        asm.Name,
                        Types = asm.Types.Select(type => new
                        {
                            type.FullName,
                            Methods = type.Methods.Select(meth => new
                            {
                                meth.FullName,
                                meth.Source,
                                meth.StartLine,
                                meth.StartColumn,
                                meth.EndLine,
                                meth.EndColumn,
                                meth.VisitCount
                            }).ToList()
                        }).ToList()
                    });
                } else
                {
                    // Assembly is already present, check for types
                    foreach(var type in asm.Types)
                    {
                        var targetType = targetAssembly.Types.Find(a => a.FullName == type.FullName);
                        if (targetType == null)
                        {
                            // This type is not present in the result, add everything about it to the result
                            targetAssembly.Types.Add(new
                            {
                                type.FullName,
                                Methods = type.Methods.Select(meth => new
                                {
                                    meth.FullName,
                                    meth.Source,
                                    meth.StartLine,
                                    meth.StartColumn,
                                    meth.EndLine,
                                    meth.EndColumn,
                                    meth.VisitCount
                                }).ToList()
                            });
                        }
                        else
                        {
                            // Type is already present, check for methods
                            foreach(var meth in type.Methods)
                            {
                                var targetMethod = targetType.Methods.Find(a => a.FullName == meth.FullName);
                                if(targetMethod == null)
                                {
                                    // This method is not present in the result, add it to the result
                                    targetType.Methods.Add(new
                                    {
                                        meth.FullName,
                                        meth.Source,
                                        meth.StartLine,
                                        meth.StartColumn,
                                        meth.EndLine,
                                        meth.EndColumn,
                                        meth.VisitCount
                                    });
                                }
                                else
                                {
                                    // Method is already present, sum up the visit count of methods from result 1 and result 2
                                    targetType.Methods.Remove(targetMethod);
                                    targetType.Methods.Add(new
                                    {
                                        meth.FullName,
                                        meth.Source,
                                        meth.StartLine,
                                        meth.StartColumn,
                                        meth.EndLine,
                                        meth.EndColumn,
                                        VisitCount = meth.VisitCount + targetMethod.VisitCount
                                    });
                                }
                            }
                        }
                    }
                }


            }

            return new CoverageResult(
                Version,
                UniqueId,
                result.Select(
                    asm => new CoverageAssemblyResult(
                        asm.Name,
                        asm.Types.Select(type => new CoverageTypeResult(
                            type.FullName,
                            type.Methods.Select(meth => new CoverageMethodResult(
                                meth.FullName,
                                meth.Source,
                                meth.StartLine,
                                meth.StartColumn,
                                meth.EndLine,
                                meth.EndColumn,
                                meth.VisitCount)).ToList().AsReadOnly()
                        )).ToList().AsReadOnly()
                        )).ToList().AsReadOnly()
                 );
        }

        public IEnumerable<string> GetSources()
        {
            return Assemblies.SelectMany(x => x.Types).SelectMany(x => x.Methods).Select(x => x.Source).Distinct();
        }

        public IEnumerable<CoverageMethodResult> GetVisitedMethods()
        {
            return Assemblies.SelectMany(x => x.Types).SelectMany(x => x.Methods).Where(x => x.IsVisited);
        }

        public IEnumerable<string> GetVisitedMethodNames()
        {
            return GetVisitedMethods().Select(x => x.FullName).Distinct();
        }

        public IEnumerable<string> GetVisitedSources()
        {
            return GetVisitedMethods().Select(x => x.Source).Distinct();
        }


        public SummaryResult GetLineSummary()
        {
            var result = new SummaryResult(0, 0);
            var allTypes = Assemblies.SelectMany(x => x.Types);
            foreach (var type in allTypes)
            {
                var typeResult = type.GetLineSummary();
                result = new SummaryResult(
                    total: result.Total + typeResult.Total,
                    covered: result.Covered + typeResult.Covered
                );
            }

            return result;
        }

        public SummaryResult GetBranchSummary()
        {
            return new SummaryResult(0, 0);
        }

        private void ValidateFilePath(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Could not find the file '{file}'.");
        }

        public static CoverageResult operator +(CoverageResult result1, CoverageResult result2)
        {
            return result1.MergeWith(result2);
        }

        public static CoverageResult Merge(CoverageResult result1, CoverageResult result2)
        {
            return result1 + result2;
        }

    }
}
