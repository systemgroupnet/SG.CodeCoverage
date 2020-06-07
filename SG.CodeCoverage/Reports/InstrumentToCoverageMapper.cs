using SG.CodeCoverage.Metadata;
using SG.CodeCoverage.Metadata.Coverage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Reports
{
    public static class InstrumentToCoverageMapper
    {
        public static CoverageResult ToCoverageResult(this InstrumentationMap map, int[][] hits)
        {
            return new CoverageResult(
                map.Version,
                map.UniqueId,
                map.Assemblies.Select(x => x.ToAssemblyCoverage(hits)).ToList().AsReadOnly()
            );
        }

        public static CoverageAssemblyResult ToAssemblyCoverage(this InstrumentedAssemblyMap assembly, int[][] hits)
        {
            return new CoverageAssemblyResult(
                assembly.Name,
                assembly.Types.Select(type => type.ToTypeCoverage(hits[type.Index])).ToList().AsReadOnly()
            );
        }

        public static CoverageTypeResult ToTypeCoverage(this InstrumentedTypeMap type, int[] typeHits)
        {
            int visitCount(int index) => typeHits.Length == 0 ? 0 : typeHits[index];
            return new CoverageTypeResult(
                type.FullName,
                type.Methods.Select(method => method.ToMethodCoverage(visitCount(method.Index))).ToList().AsReadOnly()
            );
        }

        public static CoverageMethodResult ToMethodCoverage(this InstrumentedMethodMap method, int visitCount)
        {
            return new CoverageMethodResult(
                method.FullName,
                method.Source,
                method.StartLine,
                method.EndLine,
                visitCount
            );
        }

    }
}
