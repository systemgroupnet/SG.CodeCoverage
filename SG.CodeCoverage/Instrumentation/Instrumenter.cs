using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Mono.Cecil;
using SG.CodeCoverage.Common;
using System.IO;
using Newtonsoft.Json;
using Mono.Cecil.Cil;

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

        public Instrumenter(
            IReadOnlyCollection<string> assemblyFileNames,
            string workingDirectory,
            int controllerPortNumber,
            ILogger logger)
            : this(assemblyFileNames, workingDirectory, null, controllerPortNumber, logger)
        {
        }

        public IReadOnlyCollection<Map.Assembly> Instrument()
        {
            var maps = InstrumentInternal();

            if (!string.IsNullOrEmpty(OutputMapFilePath))
                SaveMapFile(maps, OutputMapFilePath);

            return maps;
        }

        private IReadOnlyCollection<Map.Assembly> InstrumentInternal()
        {
            _currentTypeIndex = 0;

            var assemblyMaps = new List<Map.Assembly>();

            foreach (var asmFile in AssemblyFileNames)
            {
                try
                {
                    using (var asm = AssemblyDefinition.ReadAssembly(asmFile, _readerParams))
                    {
                        var assemblyMap = InstrumentAssembly(asm);
                        assemblyMaps.Add(assemblyMap);
                        asm.Write(_writerParams);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(
                        $"Error while processing assembly '{asmFile}'. Assembly skipped. The error was:\r\n" +
                        ex.ToString());
                }
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
            File.Copy(Path.ChangeExtension(path, "pdb"), Path.ChangeExtension(newPath, "pdb"));

            EditFieldInitializations(newPath, typesCount);
        }

        private void EditFieldInitializations(string recorderAsmPath, int typesCount)
        {
            using (var asm = AssemblyDefinition.ReadAssembly(recorderAsmPath, _readerParams))
            {
                var constantsType = FindType(asm, nameof(Recorder.InjectedConstants));
                var cctor = constantsType.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
                var instructions = cctor.Body.Instructions;

                ChangeFieldSetterLoadedValue(
                    instructions,
                    nameof(Recorder.InjectedConstants.TypesCount),
                    typesCount);

                ChangeFieldSetterLoadedValue(
                    instructions,
                    nameof(Recorder.InjectedConstants.ControllerServerPort),
                    ControllerPortNumber);

                asm.Write(_writerParams);
            }
        }

        private void ChangeFieldSetterLoadedValue(
            Mono.Collections.Generic.Collection<Instruction> instructions,
            string fieldName, object value)
        {
            for (int i = 0; i < instructions.Count; i++)
            {
                var ins = instructions[i];
                if (ins.OpCode.Code == Code.Stsfld &&
                    ins.Operand is FieldDefinition fieldDef &&
                    fieldDef.Name == fieldName)
                {
                    var valueLoader = instructions[i - 1];
                    if (!valueLoader.OpCode.Name.StartsWith("ld"))
                        throw new Exception("Unexpected instruction.");
                    valueLoader.Operand = value;
                    switch (value)
                    {
                        case int _:
                            valueLoader.OpCode = OpCodes.Ldc_I4;
                            break;
                        case string _:
                            valueLoader.OpCode = OpCodes.Ldstr;
                            break;
                        default:
                            throw new NotSupportedException("Incorrect field type.");
                    }
                }
            }
        }

        private void SaveMapFile(IEnumerable<Map.Assembly> assemblyMaps, string outputFilePath)
        {
            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
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
