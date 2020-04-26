using System;

namespace SG.CodeCoverage.Recorder
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class InstrumentationUniqueIdAttribute : Attribute
    {
        public InstrumentationUniqueIdAttribute(string uniqueIdGuidStr)
        {
            UniqueIdString = uniqueIdGuidStr;
        }

        public string UniqueIdString { get; }
        public Guid UniqueId => Guid.Parse(UniqueIdString);
    }
}
