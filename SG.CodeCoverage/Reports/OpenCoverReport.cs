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
    public class OpenCoverReport : IReport
    {
        public string Format => "opencover";

        public string Extension => "opencover.xml";

        public string GenerateReport(CoverageResult result)
        {
            XDocument xml = new XDocument();
            XElement coverage = new XElement("CoverageSession");
            XElement coverageSummary = new XElement("Summary");
            XElement modules = new XElement("Modules");

            int numClasses = 0, numMethods = 0;
            int visitedClasses = 0, visitedMethods = 0;

            int i = 1;

            foreach (var mod in result.Assemblies)
            {
                XElement module = new XElement("Module");
                module.Add(new XAttribute("hash", Guid.NewGuid().ToString().ToUpper()));

                XElement path = new XElement("ModulePath", mod.Name);
                XElement time = new XElement("ModuleTime", DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss"));
                XElement name = new XElement("ModuleName", Path.GetFileNameWithoutExtension(mod.Name));

                module.Add(path);
                module.Add(time);
                module.Add(name);

                XElement files = new XElement("Files");
                XElement classes = new XElement("Classes");

                var docToTypesMap = mod.GetDocumentToTypesMap();

                foreach (var doc in docToTypesMap)
                {
                    XElement file = new XElement("File");
                    file.Add(new XAttribute("uid", i.ToString()));
                    file.Add(new XAttribute("fullPath", doc.Key));
                    files.Add(file);

                    foreach (var cls in doc.Value)
                    {
                        XElement @class = new XElement("Class");
                        XElement classSummary = new XElement("Summary");

                        XElement className = new XElement("FullName", cls.FullName);

                        XElement methods = new XElement("Methods");
                        int j = 0;

                        foreach (var meth in cls.Methods)
                        {

                            var methLineCoverage = meth.GetLineSummary();
                            var methBranchCoverage = meth.GetBranchSummary();
                            int methCyclomaticComplexity = 0;

                            XElement method = new XElement("Method");

                            method.Add(new XAttribute("cyclomaticComplexity", methCyclomaticComplexity.ToString()));
                            method.Add(new XAttribute("nPathComplexity", "0"));
                            method.Add(new XAttribute("sequenceCoverage", methLineCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
                            method.Add(new XAttribute("branchCoverage", methBranchCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
                            method.Add(new XAttribute("isConstructor", meth.FullName.Contains("ctor").ToString()));
                            method.Add(new XAttribute("isGetter", meth.FullName.Contains("get_").ToString()));
                            method.Add(new XAttribute("isSetter", meth.FullName.Contains("set_").ToString()));
                            method.Add(new XAttribute("isStatic", (!meth.FullName.Contains("get_") || !meth.FullName.Contains("set_")).ToString()));

                            XElement methodName = new XElement("Name", meth.FullName);

                            XElement fileRef = new XElement("FileRef");
                            fileRef.Add(new XAttribute("uid", i.ToString()));

                            XElement methodPoint = new XElement("MethodPoint");
                            methodPoint.Add(new XAttribute("vc", methLineCoverage.Covered.ToString()));
                            methodPoint.Add(new XAttribute("uspid", "0"));
                            methodPoint.Add(new XAttribute(XName.Get("type", "xsi"), "SequencePoint"));
                            methodPoint.Add(new XAttribute("ordinal", j.ToString()));
                            methodPoint.Add(new XAttribute("offset", j.ToString()));
                            methodPoint.Add(new XAttribute("sc", "0"));
                            methodPoint.Add(new XAttribute("sl", meth.StartLine.ToString()));
                            methodPoint.Add(new XAttribute("ec", "1"));
                            methodPoint.Add(new XAttribute("el", meth.EndLine.ToString()));
                            methodPoint.Add(new XAttribute("bec", "0"));
                            methodPoint.Add(new XAttribute("bev", "0"));
                            methodPoint.Add(new XAttribute("fileid", i.ToString()));

                            XElement sequencePoints = new XElement("SequencePoints");
                            XElement branchPoints = new XElement("BranchPoints");
                            XElement methodSummary = new XElement("Summary");



                            numMethods++;
                            if (meth.IsVisited)
                                visitedMethods++;

                            methodSummary.Add(new XAttribute("numSequencePoints", methLineCoverage.Total.ToString()));
                            methodSummary.Add(new XAttribute("visitedSequencePoints", methLineCoverage.Covered.ToString()));
                            methodSummary.Add(new XAttribute("numBranchPoints", methBranchCoverage.Total.ToString()));
                            methodSummary.Add(new XAttribute("visitedBranchPoints", methBranchCoverage.Covered.ToString()));
                            methodSummary.Add(new XAttribute("sequenceCoverage", methLineCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
                            methodSummary.Add(new XAttribute("branchCoverage", methBranchCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
                            methodSummary.Add(new XAttribute("maxCyclomaticComplexity", methCyclomaticComplexity.ToString()));
                            methodSummary.Add(new XAttribute("minCyclomaticComplexity", methCyclomaticComplexity.ToString()));
                            methodSummary.Add(new XAttribute("visitedClasses", "0"));
                            methodSummary.Add(new XAttribute("numClasses", "0"));
                            methodSummary.Add(new XAttribute("visitedMethods", meth.IsVisited ? "1" : "0"));
                            methodSummary.Add(new XAttribute("numMethods", "1"));

                            method.Add(methodSummary);
                            method.Add(new XElement("MetadataToken"));
                            method.Add(methodName);
                            method.Add(fileRef);
                            method.Add(sequencePoints);
                            method.Add(branchPoints);
                            method.Add(methodPoint);
                            methods.Add(method);
                            j++;
                        }

                        numClasses++;
                        if (cls.IsVisited)
                            visitedClasses++;

                        var classLineCoverage = cls.GetLineSummary();
                        var classBranchCoverage = cls.GetBranchSummary();
                        var classMethodCoverage = cls.GetMethodSummary();
                        var classMaxCyclomaticComplexity = 0;
                        var classMinCyclomaticComplexity = 0;

                        classSummary.Add(new XAttribute("numSequencePoints", classLineCoverage.Total.ToString()));
                        classSummary.Add(new XAttribute("visitedSequencePoints", classLineCoverage.Covered.ToString()));
                        classSummary.Add(new XAttribute("numBranchPoints", classBranchCoverage.Total.ToString()));
                        classSummary.Add(new XAttribute("visitedBranchPoints", classBranchCoverage.Covered.ToString()));
                        classSummary.Add(new XAttribute("sequenceCoverage", classLineCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
                        classSummary.Add(new XAttribute("branchCoverage", classBranchCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
                        classSummary.Add(new XAttribute("maxCyclomaticComplexity", classMaxCyclomaticComplexity.ToString()));
                        classSummary.Add(new XAttribute("minCyclomaticComplexity", classMinCyclomaticComplexity.ToString()));
                        classSummary.Add(new XAttribute("visitedClasses", cls.IsVisited ? "1" : "0"));
                        classSummary.Add(new XAttribute("numClasses", "1"));
                        classSummary.Add(new XAttribute("visitedMethods", classMethodCoverage.Covered.ToString()));
                        classSummary.Add(new XAttribute("numMethods", classMethodCoverage.Total.ToString()));

                        @class.Add(classSummary);
                        @class.Add(className);
                        @class.Add(methods);
                        classes.Add(@class);
                    }
                    i++;
                }

                module.Add(files);
                module.Add(classes);
                modules.Add(module);
            }

            var moduleLineCoverage = result.GetLineSummary();
            var moduleBranchCoverage = result.GetBranchSummary();
            var moduleMaxCyclomaticComplexity = 0;
            var moduleMinCyclomaticComplexity = 0;

            coverageSummary.Add(new XAttribute("numSequencePoints", moduleLineCoverage.Total.ToString()));
            coverageSummary.Add(new XAttribute("visitedSequencePoints", moduleLineCoverage.Covered.ToString()));
            coverageSummary.Add(new XAttribute("numBranchPoints", moduleBranchCoverage.Total.ToString()));
            coverageSummary.Add(new XAttribute("visitedBranchPoints", moduleBranchCoverage.Covered.ToString()));
            coverageSummary.Add(new XAttribute("sequenceCoverage", moduleLineCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
            coverageSummary.Add(new XAttribute("branchCoverage", moduleBranchCoverage.Percentage.ToString("G", CultureInfo.InvariantCulture)));
            coverageSummary.Add(new XAttribute("maxCyclomaticComplexity", moduleMaxCyclomaticComplexity.ToString()));
            coverageSummary.Add(new XAttribute("minCyclomaticComplexity", moduleMinCyclomaticComplexity.ToString()));
            coverageSummary.Add(new XAttribute("visitedClasses", visitedClasses.ToString()));
            coverageSummary.Add(new XAttribute("numClasses", numClasses.ToString()));
            coverageSummary.Add(new XAttribute("visitedMethods", visitedMethods.ToString()));
            coverageSummary.Add(new XAttribute("numMethods", numMethods.ToString()));

            coverage.Add(coverageSummary);
            coverage.Add(modules);
            xml.Add(coverage);

            var stream = new MemoryStream();
            xml.Save(stream);

            return Encoding.UTF8.GetString(stream.ToArray());
        }


    }
}
