using Newtonsoft.Json;
using SG.CodeCoverage.Coverage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Reports
{
    public class JsonReport : IReport
    {
        public string Format => "json";

        public string Extension => "json";

        public string GenerateReport(CoverageResult coverageResult)
        {
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };

            return JsonConvert.SerializeObject(coverageResult, settings);
        }
    }
}
