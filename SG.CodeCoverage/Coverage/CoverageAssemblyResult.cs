using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Coverage
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

        public Dictionary<string, List<CoverageTypeResult>> GetDocumentToTypesMap()
        {
            var result = new Dictionary<string, List<CoverageTypeResult>>();

            foreach (var type in Types)
            {
                var fileNames = type.Methods.Select(x => x.Source);

                foreach (var fileName in fileNames)
                {
                    if (result.ContainsKey(fileName))
                    {
                        if (!result[fileName].Contains(type))
                        {
                            result[fileName].Add(type);
                        }
                    }
                    else
                    {
                        result.Add(fileName, new List<CoverageTypeResult> { type });
                    }
                }
            }

            return result;
        }

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

        public SummaryResult GetBranchSummary()
        {
            return new SummaryResult(0, 0);
        }

        public SummaryResult GetMethodSummary()
        {
            var result = new SummaryResult(0, 0);
            foreach (var type in Types)
            {
                var current = type.GetMethodSummary();
                result = new SummaryResult(
                    total: result.Total + current.Total,
                    covered: result.Covered + current.Covered
                );
            }

            return result;
        }

        public int CalculateCyclomaticComplexity()
        {
            return 0;
        }
    }
}
