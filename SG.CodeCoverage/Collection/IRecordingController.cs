using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Metadata;

namespace SG.CodeCoverage.Collection
{
    public interface IRecordingController
    {
        InstrumentationMap InstrumentationMap { get; }
        void ResetHits();
        CoverageResult CollectResultAndReset();
    }
}
