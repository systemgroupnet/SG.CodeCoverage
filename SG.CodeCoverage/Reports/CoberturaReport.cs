using SG.CodeCoverage.Coverage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SG.CodeCoverage.Reports
{
    public class CoberturaReport : IReport
    {
        public string Format => "cobertura";

        public string Extension => "cobertura.xml";
        public string GenerateReport(CoverageResult coverageResult)
        {

            var lineCoverage = coverageResult.GetLineSummary();
            var branchCoverage = coverageResult.GetBranchSummary();

            XDocument xml = new XDocument();
            XElement coverage = new XElement("coverage");
            coverage.Add(new XAttribute("line-rate", (lineCoverage.Percentage / 100).ToString(CultureInfo.InvariantCulture)));
            coverage.Add(new XAttribute("branch-rate", (branchCoverage.Percentage / 100).ToString(CultureInfo.InvariantCulture)));
            coverage.Add(new XAttribute("version", "1.9"));
            coverage.Add(new XAttribute("timestamp", (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds));

            XElement sources = new XElement("sources");
            var absolutePaths = coverageResult.GetSources().ToList();
            absolutePaths.ForEach(x => sources.Add(new XElement("source", x)));

            XElement packages = new XElement("packages");
            foreach (var module in coverageResult.Assemblies)
            {
                var asmLineSummary = module.GetLineSummary();
                var asmBranchSummary = module.GetBranchSummary();

                XElement package = new XElement("package");
                package.Add(new XAttribute("name", module.Name));
                package.Add(new XAttribute("line-rate", (asmLineSummary.Percentage / 100).ToString(CultureInfo.InvariantCulture)));
                package.Add(new XAttribute("branch-rate", (asmBranchSummary.Percentage / 100).ToString(CultureInfo.InvariantCulture)));
                package.Add(new XAttribute("complexity", module.CalculateCyclomaticComplexity()));

                XElement classes = new XElement("classes");
                foreach (var document in module.GetDocumentToTypesMap())
                {
                    foreach (var cls in document.Value)
                    {
                        var clsLineSummary = cls.GetLineSummary();
                        var clsBranchSummary = cls.GetBranchSummary();

                        XElement @class = new XElement("class");
                        @class.Add(new XAttribute("name", cls.FullName));
                        @class.Add(new XAttribute("filename", document.Key));
                        @class.Add(new XAttribute("line-rate", (clsLineSummary.Percentage / 100).ToString(CultureInfo.InvariantCulture)));
                        @class.Add(new XAttribute("branch-rate", (clsBranchSummary.Percentage / 100).ToString(CultureInfo.InvariantCulture)));
                        @class.Add(new XAttribute("complexity", cls.CalculateCyclomaticComplexity()));

                        XElement classLines = new XElement("lines");
                        XElement methods = new XElement("methods");

                        foreach (var meth in cls.Methods)
                        {

                            if (meth.LinesCount == 0)
                                continue;

                            var methLineSummary = meth.GetLineSummary();
                            var methBranchSummary = meth.GetBranchSummary();

                            XElement method = new XElement("method");
                            method.Add(new XAttribute("name", meth.FullName.Split(':').Last().Split('(').First()));
                            method.Add(new XAttribute("signature", "(" + meth.FullName.Split(':').Last().Split('(').Last()));
                            method.Add(new XAttribute("line-rate", (methLineSummary.Percentage / 100).ToString(CultureInfo.InvariantCulture)));
                            method.Add(new XAttribute("branch-rate", (branchCoverage.Percentage / 100).ToString(CultureInfo.InvariantCulture)));

                            methods.Add(method);
                        }

                        @class.Add(methods);
                        @class.Add(classLines);
                        classes.Add(@class);
                    }
                }

                package.Add(classes);
                packages.Add(package);
            }

            coverage.Add(new XAttribute("lines-covered", lineCoverage.Covered.ToString(CultureInfo.InvariantCulture)));
            coverage.Add(new XAttribute("lines-valid", lineCoverage.Total.ToString(CultureInfo.InvariantCulture)));
            coverage.Add(new XAttribute("branches-covered", branchCoverage.Covered.ToString(CultureInfo.InvariantCulture)));
            coverage.Add(new XAttribute("branches-valid", branchCoverage.Total.ToString(CultureInfo.InvariantCulture)));

            coverage.Add(sources);
            coverage.Add(packages);
            xml.Add(coverage);

            var stream = new MemoryStream();
            xml.Save(stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
