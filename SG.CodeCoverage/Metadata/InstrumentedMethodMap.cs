namespace SG.CodeCoverage.Metadata
{
    public class InstrumentedMethodMap
    {
        public InstrumentedMethodMap(string fullName, int index, string source, int startLine, int startColumn, int endLine, int endColumn)
        {
            FullName = fullName;
            Index = index;
            Source = source;
            StartLine = startLine;
            StartColumn = startColumn;
            EndLine = endLine;
            EndColumn = endColumn;
        }

        public string FullName { get; }
        public int Index { get; }
        public string Source { get; }
        public int StartLine { get; }
        public int StartColumn { get; }
        public int EndLine { get; }
        public int EndColumn { get; }
    }
}
