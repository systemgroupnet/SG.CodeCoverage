using SG.CodeCoverage.Recorder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Instrumentation
{
    public class InstrumentationOptions
    {
        /// <summary>
        /// </summary>
        /// <param name="assemblyFileNames">Path to all the assembly files that should be instrumented.</param>
        /// <param name="additionalReferencePaths">
        /// Path to directories containing additional dependencies required by instrumented assemblies.
        /// </param>
        /// <param name="recorderAssemblyCopyPath">
        /// Path to a folder that the modified "SG.CodeCoverage.Recorder.dll" will be copied.
        /// This folder should be accessible by the system under test. The instrumented assemblies will reference the
        /// recorder assembly and should be able to load it.
        /// The system under test should probe this folder for it's dependencies, or the file "SG.CodeCoverage.Recorder.dll"
        /// should manually copied to somewhere accessible by it.
        /// </param>
        /// <param name="controllerPortNumber">
        /// The port that `RecorindController.Server` will listen on, to accept
        /// commands. If 0 is passed, it uses a random available port. The port will be written to specified
        /// RuntimeConfig file.
        /// </param>
        public InstrumentationOptions(
            IReadOnlyCollection<string> assemblyFileNames,
            IReadOnlyCollection<string> additionalReferencePaths,
            string recorderAssemblyCopyPath,
            int controllerPortNumber)
        {
            AssemblyFileNames = assemblyFileNames;
            AdditionalReferencePaths = additionalReferencePaths;
            RecorderAssemblyCopyPath = recorderAssemblyCopyPath;
            ControllerPortNumber = controllerPortNumber;
        }

        public static readonly string DefaultRuntimeConfigFileName = InjectedConstants.RuntimeConfigFileName;

        /// <summary>
        /// Full path of the assemblies to instrument.
        /// </summary>
        public IReadOnlyCollection<string> AssemblyFileNames { get; set; }
        /// <summary>
        /// Path to directories containing additional dependencies required by instrumented assemblies.
        /// </summary>
        public IReadOnlyCollection<string> AdditionalReferencePaths { get; set; }
        /// <summary>
        /// Path to a folder that the modified "SG.CodeCoverage.Recorder.dll" will be copied.
        /// This folder should be accessible by the system under test. The instrumented assemblies
        /// will reference the recorder assembly and should be able to load it.
        /// The system under test should probe this folder for it's dependencies, or the file
        /// "SG.CodeCoverage.Recorder.dll" should manually be copied to somewhere accessible by it.
        /// </summary>
        public string RecorderAssemblyCopyPath { get; set; }
        /// The port that `RecorindController.Server` will listen on, to accept
        /// commands. If 0 is passed, it uses a random available port. The port will be written to specified
        /// "Runtime Config" file.
        /// Default value: 0
        public int ControllerPortNumber { get; set; } = 0;
        /// <summary>
        /// Name of the Runtime Config file. Used to store listening port of the controller server.
        /// Default value: "CodeCoverageRecorderRuntimeConfig.cfg"
        /// </summary>
        public string RuntimeConfigFileName { get; set; } = DefaultRuntimeConfigFileName;
        /// <summary>
        /// Path to store Runtime Config file.
        /// If not specified or empty, the file will be stored in the same directory as where the recorder assembly
        /// is loaded from.
        /// Default value: "".
        /// </summary>
        public string RuntimeConfigOutputPath { get; set; } = InjectedConstants.RuntimeConfigOutputPath;
    }
}
