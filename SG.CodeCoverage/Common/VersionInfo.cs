using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Common
{
    public class VersionInfo
    {
        public const string CurrentVersionString = "0.10.0";
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public static VersionInfo Current { get; }

        public VersionInfo(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        static VersionInfo()
        {
            var parts = CurrentVersionString.Split('.');
            Current = new VersionInfo(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }
    }
}
