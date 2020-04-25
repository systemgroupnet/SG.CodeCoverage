using Mono.Cecil;
using SG.CodeCoverage.Common;
using System.Linq;
using System.Collections.Generic;
using System;
using Mono.Cecil.Cil;
using SG.CodeCoverage.Metadata;

namespace SG.CodeCoverage.Instrumentation
{
    internal class TypeInstrumenter
    {
        private readonly int _index;
        private readonly TypeDefinition _type;
        private readonly ModuleDefinition _module;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger _logger;
#pragma warning restore IDE0052 // Remove unread private members
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

        public InstrumentedTypeMap Instrument()
        {
            var methodsMaps = new List<InstrumentedMethodMap>();

            _currentMethodIndex = 0;

            foreach (var method in _type.Methods)
            {
                var methodMap = InstrumentMethod(method);
                if (methodMap != null)
                    methodsMaps.Add(methodMap);
            }

            AddInitializerCall(_currentMethodIndex);

            return new InstrumentedTypeMap(_type.FullName, _index, methodsMaps.AsReadOnly());
        }

        private InstrumentedMethodMap InstrumentMethod(MethodDefinition method)
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

            RemoveAllCalls(methodBody, _addHitMethodRef);
            InjectCall(methodBody, _addHitMethodRef, _index, methodIndex);

            return new InstrumentedMethodMap(method.FullName, methodIndex, sourceFile, startLine);
        }

        private void AddInitializerCall(int totalMethods)
        {
            var cctor = _type.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);
            if (cctor == null)
                cctor = CreateCCtor();

            // We need to remove this flag, so we can be sure that the type will be initialized
            // before its first use (i.e. it's static constructor is called before any method
            // of the type is used).
            _type.IsBeforeFieldInit = false;

            var initTypeMethod = new Action<int, int>(Recorder.HitsRepository.InitType).Method;
            var initTypeMethodRef = _module.ImportReference(initTypeMethod);
            RemoveAllCalls(cctor.Body, initTypeMethodRef);
            InjectCall(cctor.Body, initTypeMethodRef, _index, totalMethods);
        }

        private MethodDefinition CreateCCtor()
        {
            var cctor = new MethodDefinition(".cctor",
                MethodAttributes.Static | MethodAttributes.Private |
                MethodAttributes.SpecialName | MethodAttributes.RTSpecialName |
                MethodAttributes.HideBySig,
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

        private void RemoveAllCalls(MethodBody body, MethodReference methodToRemove)
        {
            var intName = typeof(int).Name;
            if (
                methodToRemove.Parameters.Count != 2 ||
                methodToRemove.Parameters[0].ParameterType.Name != intName ||
                methodToRemove.Parameters[1].ParameterType.Name != intName)
            {
                throw new Exception("The method does not match the expected signature. We are only interested in methods with `void (int, int)` signature.");
            }

            var instructions = body.Instructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                var ins = instructions[i];
                if (
                    ins.OpCode.Code == Code.Call &&
                    ins.Operand is MethodReference mr &&
                    mr.FullName == methodToRemove.FullName)
                {
                    Instruction ip1, ip2;
                    if (
                        i < 2 ||
                        (ip1 = instructions[i - 1]).OpCode.Code != Code.Ldc_I4 ||
                        (ip2 = instructions[i - 2]).OpCode.Code != Code.Ldc_I4)
                        throw new Exception("Invalid opcodes for method parameters.");
                    var ilProcessor = body.GetILProcessor();
                    ilProcessor.Remove(ip1);
                    ilProcessor.Remove(ip2);
                    ilProcessor.Remove(ins);
                    return;
                }
            }
        }
    }
}
