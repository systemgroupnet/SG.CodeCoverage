namespace SG.CodeCoverage.Metadata
{
    public class InstrumentedMethodMap
    {
        public InstrumentedMethodMap(string fullName, int index, string source, int startLine, int endLine)
        {
            FullName = fullName;
            Index = index;
            Source = source;
            StartLine = startLine;
            EndLine = endLine;
        }

        public string FullName { get; }
        public int Index { get; }
        public string Source { get; }
        public int StartLine { get; }
        public int EndLine { get; }
    }
}
