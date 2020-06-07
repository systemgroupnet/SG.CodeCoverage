using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Coverage
{
    public struct SummaryResult
    {
        public int Total { get; }

        public int Covered { get; }

        public float Percentage => (Covered / (float)Total) * 100;

        public SummaryResult(int total, int covered)
        {
            Total = total;
            Covered = covered;
        }
    }
}
