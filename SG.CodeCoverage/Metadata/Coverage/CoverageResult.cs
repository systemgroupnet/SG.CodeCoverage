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

    internal static class InstrumentToCoverageMapper
    {
        public static CoverageMethodResult ToMethodCoverage(this InstrumentedMethodMap method, int visitCount)
        {
            return new CoverageMethodResult(
                method.FullName,
                method.Index,
                method.Source,
                method.StartLine,
                method.EndLine,
                visitCount
            );
        }

        public static CoverageTypeResult ToTypeCoverage(this InstrumentedTypeMap type, int[] typeHits)
        {
            return new CoverageTypeResult(
                type.FullName,
                type.Index,
                type.Methods.Select(x => x.ToMethodCoverage(typeHits[x.Index])).ToList().AsReadOnly()
            );
        }

        public static CoverageAssemblyResult ToAssemblyCoverage(this InstrumentedAssemblyMap assembly, int[][] hits)
        {
            return new CoverageAssemblyResult(
                assembly.Name,
                assembly.Types.Select(x => x.ToTypeCoverage(hits[x.Index])).ToList().AsReadOnly()
            );
        }

        public static CoverageResult ToCoverageMap(this InstrumentationMap map, int[][] hits)
        {
            return new CoverageResult(
                map.Version,
                map.UniqueId,
                map.Assemblies.Select(x => x.ToAssemblyCoverage(hits)).ToList().AsReadOnly()
            );
        }

    }
}
