using System;
using System.Collections.Generic;
using System.Text;

namespace SG.CodeCoverage.Collection
{
    public class DataCollector
    {
        public string MapFilePath { get; }

        public string HitsFilePath { get; }

        public DataCollector(string mapFilePath, string hitsFilePath)
        {
            MapFilePath = mapFilePath;
            HitsFilePath = hitsFilePath;
        }

        public IEnumerable<string> GetVisitedFiles()
        {
            throw new NotImplementedException();
        }

    }
}
