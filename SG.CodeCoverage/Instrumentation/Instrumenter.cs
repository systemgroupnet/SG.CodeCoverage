using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mono.Cecil;
using SG.CodeCoverage.Common;
using System.IO;
using Newtonsoft.Json;

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


        public void Instrument()
        {
            var maps = InstrumentInternal();

            SaveMapFile(maps, OutputMapFilePath);
        }

        private IReadOnlyCollection<Map.Assembly> InstrumentInternal()
        {
            _currentTypeIndex = 0;

            var assemblyMaps = new List<Map.Assembly>();

            foreach (var asmFile in AssemblyFileNames)
            {
                var asm = AssemblyDefinition.ReadAssembly(asmFile, _readerParams);
                var assemblyMap = InstrumentAssembly(asm);
                assemblyMaps.Add(assemblyMap);
                asm.Write(_writerParams);
            }

            CopyAndModifyRecorderAssembly(_currentTypeIndex);

            return assemblyMaps;
        }

        private Map.Assembly InstrumentAssembly(AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;
            var assemblyMap = new Map.Assembly()
            {
                Name = assembly.FullName
            };

            foreach (var type in module.GetTypes())
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

        private void CopyAndModifyRecorderAssembly(int typesCount)
        {
            var path = typeof(Recorder.HitsRepository).Assembly.Location;
            var newPath = Path.Combine(WorkingDirectory, Path.GetFileName(path));
            File.Copy(path, newPath, true);

            var asm = AssemblyDefinition.ReadAssembly(newPath, _readerParams);
            var constantsType = FindType(asm, nameof(Recorder.InjectedConstants));
            var countField = FindField(constantsType, nameof(Recorder.InjectedConstants));
            countField.InitialValue = BitConverter.GetBytes(typesCount);
            var portField = FindField(constantsType, nameof(Recorder.InjectedConstants.ControllerServerPort));
            portField.InitialValue = BitConverter.GetBytes(ControllerPortNumber);
            asm.Write(newPath, _writerParams);
        }

        private void SaveMapFile(IEnumerable<Map.Assembly> assemblyMaps, string outputFilePath)
        {
            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };

            using (var writer = new JsonTextWriter(new StreamWriter(outputFilePath)))
                serializer.Serialize(writer, assemblyMaps);
        }

        private static TypeDefinition FindType(AssemblyDefinition asmDef, string typeName)
        {
            return asmDef.MainModule.Types.First(t => t.Name == typeName);
        }

        private static FieldDefinition FindField(TypeDefinition typeDef, string fieldName)
        {
            return typeDef.Fields.First(f => f.Name == fieldName);
        }
    }
}
