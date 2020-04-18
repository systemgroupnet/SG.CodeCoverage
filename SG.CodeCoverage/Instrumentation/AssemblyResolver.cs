using Mono.Cecil;
using SG.CodeCoverage.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SG.CodeCoverage.Instrumentation
{
    internal class AssemblyResolver : BaseAssemblyResolver
    {
        private readonly DefaultAssemblyResolver _defaultResolver;
        private readonly string _workingDirectory;
        private readonly ILogger _logger;

        public AssemblyResolver(string workingDirectory, ILogger logger)
        {
            _defaultResolver = new DefaultAssemblyResolver();
            _workingDirectory = workingDirectory;
            _logger = logger;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition assembly;
            try
            {
                return assembly = _defaultResolver.Resolve(name);
            }
            catch (AssemblyResolutionException ex)
            {
                var path = Path.Combine(_workingDirectory, name.Name) + ".dll";
                if (File.Exists(path))
                    return AssemblyDefinition.ReadAssembly(path);

                _logger.LogWarning("Could not resolve assembly " + name + ".\r\n" + ex.ToString());
                return null;
            }
        }
    }
}
