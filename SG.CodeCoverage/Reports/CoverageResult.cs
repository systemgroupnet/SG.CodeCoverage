using SG.CodeCoverage.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace SG.CodeCoverage.Reports
{
    public class CoverageResult
    {
        public InstrumentationMap Map { get; }

        public int[][] Hits { get; }


        public CoverageResult(InstrumentationMap map, int[][] hits)
        {
            this.Map = map;
            this.Hits = hits;
        }
    }
}