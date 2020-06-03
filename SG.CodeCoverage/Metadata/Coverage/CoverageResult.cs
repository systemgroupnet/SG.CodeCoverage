using SG.CodeCoverage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Metadata.Coverage
{
    public class CoverageResult
    {

        public CoverageResult(VersionInfo instrumenterVersion, Guid instrumentUniqueId, IReadOnlyCollection<CoverageAssemblyResult> assemblies)
        {
            Version = instrumenterVersion;
            UniqueId = instrumentUniqueId;
            Assemblies = assemblies;
        }

        public VersionInfo Version { get; }
        public Guid UniqueId { get; }
        public IReadOnlyCollection<CoverageAssemblyResult> Assemblies { get; }

        public CoverageResult MergeWith(CoverageResult otherResult)
        {
            if (otherResult == null)
                return this;

            // Create an anonymous data that holds every paramter we need to construct the coverage result types
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
                            meth.EndLine,
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
                                meth.EndLine,
                                meth.VisitCount
                            }).ToList()
                        }).ToList()
                    });
                } else
                {
                    // Assembly is already present, check for types
                    foreach(var type in targetAssembly.Types)
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
                                    meth.EndLine,
                                    meth.VisitCount
                                }).ToList()
                            });
                        }
                        else
                        {
                            // Type is already present, check for methods
                            foreach(var meth in targetType.Methods)
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
                                        meth.EndLine,
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
                                        meth.EndLine,
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
                                meth.EndLine,
                                meth.VisitCount)).ToList().AsReadOnly()
                        )).ToList().AsReadOnly()
                        )).ToList().AsReadOnly()
                 );
        }

        public List<string> GetSources()
        {
            return Assemblies.SelectMany(x => x.Types).SelectMany(x => x.Methods).Select(x => x.Source).Distinct().ToList();
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

    }
}
