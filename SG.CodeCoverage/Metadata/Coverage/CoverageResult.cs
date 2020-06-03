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
