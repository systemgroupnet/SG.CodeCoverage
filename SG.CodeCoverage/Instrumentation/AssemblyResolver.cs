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
        private readonly IReadOnlyCollection<string> _additionalReferencePaths;
        private readonly ILogger _logger;

        public AssemblyResolver(IReadOnlyCollection<string> additionalReferencePaths, ILogger logger)
        {
            _defaultResolver = new DefaultAssemblyResolver();
            _additionalReferencePaths = additionalReferencePaths;
            _logger = logger;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition assembly;
            try
            {
                return assembly = _defaultResolver.Resolve(name);
            }
            catch (AssemblyResolutionException rex)
            {
                foreach (var path in _additionalReferencePaths)
                {
                    var asmPath = Path.Combine(path, name.Name) + ".dll";
                    if (File.Exists(asmPath))
                    {
                        try
                        {
                            return AssemblyDefinition.ReadAssembly(asmPath);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogWarning($"Found assembly \"{asmPath}\" but was unable to load it: {ex.Message}");
                        }
                    }
                }

                _logger.LogWarning("Could not resolve assembly " + name + ".\r\n" + rex.ToString());
                return null;
            }
        }
    }
}
