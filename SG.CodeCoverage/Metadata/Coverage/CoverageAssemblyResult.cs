using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Metadata.Coverage
{
    public class CoverageAssemblyResult
    {
        public CoverageAssemblyResult(string name, IReadOnlyCollection<CoverageTypeResult> types)
        {
            Name = name;
            Types = types;
        }

        public string Name { get; }
        public IReadOnlyCollection<CoverageTypeResult> Types { get; }
        public bool IsVisited => Types.Any(x => x.IsVisited);

        public SummaryResult GetLineSummary()
        {
            var result = new SummaryResult(0, 0);
            foreach (var type in Types)
            {
                var current = type.GetLineSummary();
                result = new SummaryResult(
                    total: result.Total + current.Total,
                    covered: result.Covered + current.Covered
                );
            }

            return result;
        }
    }
}
