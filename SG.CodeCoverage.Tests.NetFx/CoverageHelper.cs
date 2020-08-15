using SG.CodeCoverage.Coverage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Tests.NetFx
{
    public static class CoverageHelper
    {
        public static CoverageAssemblyResult FindAssembly(this CoverageResult cov, string asm)
        {
            return cov.Assemblies?.FirstOrDefault(x => x.Name == asm);
        }

        public static CoverageTypeResult FindType(this CoverageResult cov, string asm, string type)
        {
            return cov.FindAssembly(asm)?.Types.FirstOrDefault(x => x.FullName == type);
        }

        public static CoverageMethodResult FindMethod(this CoverageResult cov, string asm, string type, string method)
        {
            return cov.FindType(asm, type)?.Methods.FirstOrDefault(x => x.FullName == method);
        }
    }
}
