using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Reports
{
    public interface IReport
    {
        string Format { get; }

        string Extension { get; }

        string GenerateReport(CoverageResult coverageResult);
    }
}
