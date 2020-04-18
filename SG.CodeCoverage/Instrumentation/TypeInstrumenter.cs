using Mono.Cecil;
using SG.CodeCoverage.Common;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using Mono.Cecil.Cil;

namespace SG.CodeCoverage.Instrumentation
{
    internal class TypeInstrumenter
    {
        private readonly int _index;
        private readonly TypeDefinition _type;
        private readonly ModuleDefinition _module;
        private readonly ILogger _logger;
        private int _currentMethodIndex;
        private readonly MethodReference _addHitMethodRef;

        public TypeInstrumenter(int index, TypeDefinition type, ILogger logger)
        {
            _index = index;
            _type = type;
            _logger = logger;
            _module = _type.Module;
            var addHitMethod = new Action<int, int>(Recorder.HitsRepository.AddHit).Method;
            _addHitMethodRef = _module.ImportReference(addHitMethod);
        }

        public Map.Type Instrument()
        {
            var typeMap = new Map.Type()
            {
                FullName = _type.FullName,
                Index = _index
            };

            _currentMethodIndex = 0;

            foreach (var method in _type.Methods)
            {
                var methodMap = InstrumentMethod(method);
                if (methodMap != null)
                    typeMap.Methods.Add(methodMap);
            }

            AddInitializerCall(_currentMethodIndex);

            return typeMap;
        }

        private Map.Method InstrumentMethod(MethodDefinition method)
        {
            if (!method.HasBody)
                return null;
            var methodBody = method.Body;
            if (methodBody.Instructions.Count == 0)
                return null;
            var debugInformation = method.DebugInformation;
            if (debugInformation == null || !debugInformation.HasSequencePoints)
                return null;

            var sourceFile = debugInformation.SequencePoints.Select(s => s.Document.Url).FirstOrDefault();
            var startLine = debugInformation.SequencePoints[0].StartLine;

            int methodIndex = _currentMethodIndex++;
            var methodMap = new Map.Method()
            {
                Index = methodIndex,
                Name = method.FullName,
                Source = sourceFile,
                StartLine = startLine
            };

            InjectCall(methodBody, _addHitMethodRef, _index, methodIndex);

            return methodMap;
        }

        private void AddInitializerCall(int totalMethods)
        {
            var cctor = _type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            if (cctor == null)
                cctor = CreateCCtor();

            var initTypeMethod = new Action<int, int>(Recorder.HitsRepository.InitType).Method;
            var initTypeMethodRef = _module.ImportReference(initTypeMethod);
            InjectCall(cctor.Body, initTypeMethodRef, _index, totalMethods);
        }

        private MethodDefinition CreateCCtor()
        {
            var cctor = new MethodDefinition(".cctor",
                MethodAttributes.Static | MethodAttributes.Private |
                MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                _module.ImportReference(typeof(void)));

            cctor.Body.GetILProcessor().Emit(OpCodes.Ret);
            _type.Methods.Add(cctor);

            return cctor;
        }

        private void InjectCall(MethodBody body, MethodReference methodToCall, int param1, int param2)
        {
            var ilProcessor = body.GetILProcessor();
            var firstInstruction = body.Instructions[0];
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, param1));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ldc_I4, param2));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, methodToCall));
        }
    }
}
