using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Coverage
{
    public class CoverageMethodResult
    {
        public CoverageMethodResult(string fullName, string source, int startLine, int startColumn, int endLine, int endColumn, int visitCount)
        {
            FullName = fullName;
            Source = source;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
            VisitCount = visitCount;
        }

        public string FullName { get; }
        public string Source { get; }
        public int StartLine { get; }
        public int StartColumn { get; set; }
        public int EndLine { get; }
        public int EndColumn { get; set; }
        public int VisitCount { get; set; }
        public bool IsVisited => VisitCount > 0;

        public int LinesCount => EndLine - StartLine;

        public SummaryResult GetLineSummary()
        {
            return new SummaryResult(
                total: LinesCount,
                covered: IsVisited ? LinesCount : 0 // Because Line covereage is not enabled yet
            );
        }

        public SummaryResult GetBranchSummary()
        {
            return new SummaryResult(
                total: 0,
                covered: 0
            );
        }
    }
}
