using SG.CodeCoverage.Coverage;

namespace SG.CodeCoverage.Collection
{
    public interface IRecordingController
    {
        void ResetHits();
        CoverageResult CollectResultAndReset();
    }
}
