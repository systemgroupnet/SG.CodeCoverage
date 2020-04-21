using Mono.Cecil;
using System.Collections.Generic;

namespace SG.CodeCoverage.Instrumentation
{
    internal class AssemblyResolver : BaseAssemblyResolver
    {
        private readonly ReaderParameters _readerParams;

        public AssemblyResolver(IReadOnlyCollection<string> additionalReferencePaths)
        {
            _readerParams = new ReaderParameters() { InMemory = true };

            foreach (var path in additionalReferencePaths)
                AddSearchDirectory(path);
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            return base.Resolve(name, _readerParams);
        }
    }
}
