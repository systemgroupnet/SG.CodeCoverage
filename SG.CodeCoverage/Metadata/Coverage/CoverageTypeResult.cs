using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Metadata.Coverage
{
    public class CoverageTypeResult
    {
        public CoverageTypeResult(string fullName, int index, IReadOnlyCollection<CoverageMethodResult> methods)
        {
            FullName = fullName;
            Index = index;
            Methods = methods;
        }

        public string FullName { get; }
        public int Index { get; }

        public bool IsVisited => Methods.Any(x => x.IsVisited);

        public IReadOnlyCollection<CoverageMethodResult> Methods { get; }

        public SummaryResult GetLineSummary()
        {
            return new SummaryResult(
                total: Methods.Sum(x => x.EndLine - x.StartLine),
                covered: Methods.Sum(x => x.IsVisited ? x.LinesCount : 0)
            );
        }

        public SummaryResult GetBranchSummary()
        {
            return new SummaryResult(
                total: 0,
                covered: 0
            );
        }

        public SummaryResult GetMethodSummary()
        {
            return new SummaryResult(
               total: Methods.Count,
               covered: Methods.Count(x => x.IsVisited)
            );
        }
    }
}
