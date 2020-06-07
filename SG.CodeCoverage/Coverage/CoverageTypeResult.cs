using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Coverage
{
    public class CoverageTypeResult
    {
        public CoverageTypeResult(string fullName, IReadOnlyCollection<CoverageMethodResult> methods)
        {
            FullName = fullName;
            Methods = methods;
        }

        public string FullName { get; }

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

        public int CalculateCyclomaticComplexity()
        {
            return 0;
        }
    }
}
