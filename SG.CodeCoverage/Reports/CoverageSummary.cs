using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Reports
{
    public struct SummaryResult
    {
        public int Total { get; }

        public int Covered { get; }

        public float Percentage => ((float)Covered / (float)Total) * 100;

        public SummaryResult(int total, int covered)
        {
            Total = total;
            Covered = covered;
        }
    }

    public class CoverageSummary
    {
        public SummaryResult MethodLineCoverage(Metadata.InstrumentedMethodMap method, int visitCount)
        {
            return new SummaryResult
            (
                total: method.EndLine - method.StartLine,
                covered: visitCount > 0 ? method.EndLine - method.StartLine : 0 // Because Line covereage is not enabled yet
            );
        }

        public SummaryResult MethodBranchCoverage(Metadata.InstrumentedMethodMap method)
        {
            return new SummaryResult(
                total: 0,
                covered: 0
            );
        }

        public SummaryResult TypeLineCoverage(Metadata.InstrumentedTypeMap type, int[] typeHits)
        {
            return new SummaryResult(
               total: type.Methods.Sum(x => x.EndLine - x.StartLine),
               covered: type.Methods.Sum(x => typeHits[x.Index] > 0 ? x.EndLine - x.StartLine : 0)
            );
        }

        public SummaryResult TypeBranchCoverage(Metadata.InstrumentedTypeMap type, int[] typeHits)
        {
            return new SummaryResult(
               total: 0,
               covered: 0
            );
        }

        public SummaryResult TypeMethodCoverage(Metadata.InstrumentedTypeMap type, int[] typeHits)
        {
            return new SummaryResult(
               total: type.Methods.Count,
               covered: type.Methods.Count(x => typeHits[x.Index] > 0)
            );
        }

        public SummaryResult AssemblyLineCoverage(CoverageResult coverageResult)
        {
            var result = new SummaryResult(0, 0);
            var allTypes = coverageResult.Map.Assemblies.SelectMany(x => x.Types);
            foreach (var type in allTypes)
            {
                var typeResult = TypeLineCoverage(type, coverageResult.Hits[type.Index]);
                var newResult = new SummaryResult(
                    total: result.Total + typeResult.Total,
                    covered: result.Covered + typeResult.Covered
                );
            }

            return result;
        }

        public SummaryResult AssemblyBranchCoverage(CoverageResult coverageResult)
        {
            return new SummaryResult(0, 0);
        }
    }
}
