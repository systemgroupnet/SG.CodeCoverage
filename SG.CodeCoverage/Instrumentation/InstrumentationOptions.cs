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
        /// <param name="controllerPortNumber"></param>
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

        public IReadOnlyCollection<string> AssemblyFileNames { get; set; }
        public IReadOnlyCollection<string> AdditionalReferencePaths { get; set; }
        public string RecorderAssemblyCopyPath { get; set; }
        public int ControllerPortNumber { get; set; } = 0;
    }
}
