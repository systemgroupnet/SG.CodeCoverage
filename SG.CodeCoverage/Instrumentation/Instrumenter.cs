using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using SG.CodeCoverage.Common;
using System.IO;
using Newtonsoft.Json;
using Mono.Cecil.Cil;
using SG.CodeCoverage.Collection;
using SG.CodeCoverage.Metadata;

namespace SG.CodeCoverage.Instrumentation
{
    public class Instrumenter
    {
        private int _currentTypeIndex;
        private readonly ILogger _logger;
        private readonly ReaderParameters _readerParams;
        private readonly WriterParameters _writerParams;
        private static readonly string _delegateBaseTypeFullName = typeof(MulticastDelegate).FullName;

        public InstrumentationOptions Options { get; }
        public string OutputMapFilePath { get; }
        public string BackupFolder { get; set; }
        public Guid UniqueId { get; private set; }
        public string RecorderLogFilePath { get; set; }

        public Instrumenter(
            InstrumentationOptions options,
            string outputMapFilePath,
            ILogger logger)
        {
            Options = options;
            OutputMapFilePath = outputMapFilePath;
            _logger = logger;

            _readerParams = new ReaderParameters()
            {
                AssemblyResolver = new AssemblyResolver(Options.AdditionalReferencePaths),
                ReadSymbols = true,
                ReadWrite = true
            };

            _writerParams = new WriterParameters()
            {
                WriteSymbols = true
            };
        }

        public Instrumenter(
            InstrumentationOptions options,
            ILogger logger)
            : this(options, null, logger)
        {
        }

        public Collection.IRecordingController Instrument()
        {
            UniqueId = Guid.NewGuid();

            if (!string.IsNullOrEmpty(BackupFolder))
                Directory.CreateDirectory(BackupFolder);

            var map = InstrumentInternal();

            if (!string.IsNullOrEmpty(OutputMapFilePath))
                SaveMapFile(map, OutputMapFilePath);

            return CreateRecordingController(map);
        }

        private IRecordingController CreateRecordingController(InstrumentationMap map)
        {
            if (Options.ControllerPortNumber > 0)
            {
                return new FixedPortRecordingController(
                    "localhost", Options.ControllerPortNumber, map, _logger);
            }
            else
            {
                var dir = Options.RuntimeConfigOutputPath;
                if (string.IsNullOrEmpty(dir))
                    dir = Options.RecorderAssemblyCopyPath;
                var fullPath = Path.Combine(dir, Options.RuntimeConfigFileName);
                return new DynamicPortRecordingController(fullPath , map, _logger);
            }
        }

        private InstrumentationMap InstrumentInternal()
        {
            _currentTypeIndex = 0;

            var assemblyMaps = new List<InstrumentedAssemblyMap>();

            foreach (var asmFile in Options.AssemblyFileNames)
            {
                try
                {
                    BackupIfFolderProvided(asmFile);

                    using (var asm = AssemblyDefinition.ReadAssembly(asmFile, _readerParams))
                    {
                        if (IsAssemblySigned(asm))
                        {
                            _logger.LogWarning($"The assembly \"{asm.FullName}\" is signed! Instrumenting signed assemblies is not currently supported. Assembly skipped.");
                            continue;
                        }

                        var assemblyMap = InstrumentAssembly(asm);
                        assemblyMaps.Add(assemblyMap);
                        asm.Write(_writerParams);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"Error while processing assembly '{asmFile}'. Assembly skipped. The error was:\r\n" +
                        ex.Message);
                }
            }

            CopyAndModifyRecorderAssembly(_currentTypeIndex);

            return new InstrumentationMap(VersionInfo.Current, UniqueId, assemblyMaps);
        }

        private void BackupIfFolderProvided(string asmFile)
        {
            if (!string.IsNullOrEmpty(BackupFolder))
            {
                File.Copy(asmFile, Path.Combine(BackupFolder, Path.GetFileName(asmFile)), true);
                File.Copy(GetPdbFileName(asmFile), GetPdbFileName(Path.Combine(BackupFolder, Path.GetFileName(asmFile))), true);
            }
        }

        private bool IsAssemblySigned(AssemblyDefinition asmDef)
        {
            return asmDef.Name.HasPublicKey || asmDef.Name.PublicKey.Length > 0 ||
                asmDef.MainModule.Attributes.HasFlag(ModuleAttributes.StrongNameSigned);
        }

        private InstrumentedAssemblyMap InstrumentAssembly(AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;
            var typesMaps = new List<InstrumentedTypeMap>();

            foreach (var type in module.GetTypes())
            {
                if (type.IsInterface || type.IsEnum || IsDelegateType(type))
                    continue;

                var typeMap = InstrumentType(type);

                if (typeMap != null)
                    typesMaps.Add(typeMap);
            }

            AddUniqueIdAssemblyAttribute(assembly);

            return new InstrumentedAssemblyMap(assembly.FullName, typesMaps.AsReadOnly());
        }

        private InstrumentedTypeMap InstrumentType(TypeDefinition type)
        {
            if (!type.HasMethods)
                return null;

            var typeIndex = _currentTypeIndex++;
            return new TypeInstrumenter(typeIndex, type, _logger).Instrument();
        }

        private void AddUniqueIdAssemblyAttribute(AssemblyDefinition assembly)
        {
            var module = assembly.MainModule;
            var attribType = typeof(Recorder.InstrumentationUniqueIdAttribute);
            var attribTypeRef = module.ImportReference(attribType);
            var attrib = assembly.CustomAttributes.FirstOrDefault(a => a.AttributeType == attribTypeRef);
            var attribArgument = new CustomAttributeArgument(module.ImportReference(typeof(string)), UniqueId.ToString());
            if (attrib == null)
            {
                var constructor = attribType.GetConstructors()[0];
                attrib = new CustomAttribute(module.ImportReference(constructor));
                attrib.ConstructorArguments.Add(attribArgument);
                assembly.CustomAttributes.Add(attrib);
            }
            else
            {
                attrib.ConstructorArguments[0] = attribArgument;
            }
        }

        private void CopyAndModifyRecorderAssembly(int typesCount)
        {
            var path = typeof(Recorder.HitsRepository).Assembly.Location;
            var newPath = Path.Combine(Options.RecorderAssemblyCopyPath, Path.GetFileName(path));
            File.Copy(path, newPath, true);
            File.Copy(GetPdbFileName(path), GetPdbFileName(newPath), true);

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
                    Options.ControllerPortNumber);

                ChangeFieldSetterLoadedValue(
                    instructions,
                    nameof(Recorder.InjectedConstants.InstrumentationUniqueId),
                    UniqueId.ToString());

                if (!string.IsNullOrWhiteSpace(RecorderLogFilePath))
                    ChangeFieldSetterLoadedValue(
                        instructions,
                        nameof(Recorder.InjectedConstants.RecorderLogFileName),
                        RecorderLogFilePath);

                ChangeFieldSetterLoadedValue(
                    instructions,
                    nameof(Recorder.InjectedConstants.RuntimeConfigFileName),
                    Options.RuntimeConfigFileName);

                ChangeFieldSetterLoadedValue(
                    instructions,
                    nameof(Recorder.InjectedConstants.RuntimeConfigOutputPath),
                    Options.RuntimeConfigOutputPath);

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

        private void SaveMapFile(InstrumentationMap map, string outputFilePath)
        {
            var serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            using (var writer = new JsonTextWriter(new StreamWriter(outputFilePath)))
                serializer.Serialize(writer, map);
        }

        private static TypeDefinition FindType(AssemblyDefinition asmDef, string typeName)
        {
            return asmDef.MainModule.Types.First(t => t.Name == typeName);
        }

        private static FieldDefinition FindField(TypeDefinition typeDef, string fieldName)
        {
            return typeDef.Fields.First(f => f.Name == fieldName);
        }

        private string GetPdbFileName(string assemblyFile)
        {
            return Path.ChangeExtension(assemblyFile, "pdb");
        }

        private static bool IsDelegateType(TypeDefinition typeDef)
        {
            return typeDef.BaseType != null &&
                typeDef.BaseType.FullName == _delegateBaseTypeFullName;
        }
    }
}
