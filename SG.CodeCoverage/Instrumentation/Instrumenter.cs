using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mono.Cecil;
using SG.CodeCoverage.Common;

namespace SG.CodeCoverage.Instrumentation
{
    public class Instrumenter
    {
        private int _currentTypeIndex;
        private readonly ILogger _logger;
        private readonly ReaderParameters _readerParams;
        private readonly WriterParameters _writerParams;

        public IReadOnlyCollection<string> AssemblyFileNames { get; }
        public string WorkingDirectory { get; }
        public string OutputMapFilePath { get; }
        public int ControllerPortNumber { get; }

        public Instrumenter(
            IReadOnlyCollection<string> assemblyFileNames,
            string workingDirectory,
            string outputMapFilePath,
            int controllerPortNumber,
            ILogger logger)
        {
            AssemblyFileNames = assemblyFileNames.ToList().AsReadOnly();
            WorkingDirectory = workingDirectory;
            OutputMapFilePath = outputMapFilePath;
            ControllerPortNumber = controllerPortNumber;
            _logger = logger;

            _readerParams = new ReaderParameters()
            {
                AssemblyResolver = new AssemblyResolver(WorkingDirectory, _logger),
                ReadSymbols = true,
                ReadWrite = true
            };

            _writerParams = new WriterParameters()
            {
                WriteSymbols = true
            };

        }

        public IReadOnlyCollection<Map.Assembly> Instrument()
        {
            _currentTypeIndex = 0;

            var assemblyMaps = new List<Map.Assembly>();

            foreach(var asmFile in AssemblyFileNames)
            {
                var asm = AssemblyDefinition.ReadAssembly(asmFile, _readerParams);
                var assemblyMap = InstrumentAssembly(asm);
                assemblyMaps.Add(assemblyMap);
                asm.Write(_writerParams);
            }

            return assemblyMaps;
        }

        private Map.Assembly InstrumentAssembly(AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;
            var assemblyMap = new Map.Assembly()
            {
                Name = assembly.FullName
            };

            foreach(var type in module.GetTypes())
            {
                var typeMap = InstrumentType(type);

                if (typeMap != null)
                    assemblyMap.Types.Add(typeMap);
            }
            return assemblyMap;
        }

        private Map.Type InstrumentType(TypeDefinition type)
        {
            if (!type.HasMethods)
            {
                _logger.LogVerbose($"Type `{type.FullName}` has no methods. Skipped.");
                return null;
            }
            var typeIndex = _currentTypeIndex++;
            return new TypeInstrumenter(typeIndex, type, _logger).Instrument();
        }
    }
}
