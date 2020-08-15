using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SG.CodeCoverage.Common;
using SG.CodeCoverage.Coverage;


namespace SG.CodeCoverage.Tests.NetFx
{
    public static class CoverageMock
    {
        public static CoverageMockBuilder WithAssembly(string name)
        {
            var builder = new CoverageMockBuilder();
            return builder.WithAssembly(name);
        }

        public static CoverageResult Mock() =>
            new CoverageResult(CoverageMockBuilder.MockVerstion, CoverageMockBuilder.MockGuid, new List<CoverageAssemblyResult>().AsReadOnly());

    }

    public class CoverageMockBuilder
    {
        public static VersionInfo MockVerstion = new VersionInfo(1, 0, 0);
        public static Guid MockGuid = Guid.Parse("d20321ca-1435-4147-aafd-0a5822ea099e");

        private List<AssemblyMock> Assemblies { get; } = new List<AssemblyMock>();

        private AssemblyMock _currentAssembly;
        public CoverageMockBuilder WithAssembly(string name)
        {
            if (_currentAssembly != null)
            {
                Assemblies.Add(new AssemblyMock { Name =  _currentAssembly.Name, Types = _currentAssembly.Types });
            }

            _currentAssembly = new AssemblyMock { Name = name };
            return this;
        }

        public CoverageAssemblyBuilder WithType(string typeName)
        {
            var type = new TypeMock { Name = typeName };
            _currentAssembly.Types.Add(type);
            return new CoverageAssemblyBuilder(this, _currentAssembly, type);
        }

        public CoverageResult Mock()
        {
            var asms = Assemblies.ToList();
            asms.Add(_currentAssembly);
            return new CoverageResult(
                MockVerstion,
                MockGuid,
                asms.Select(a => new CoverageAssemblyResult(
                    name: a.Name,
                    types: a.Types.Select(t => new CoverageTypeResult
                        (

                            fullName: t.Name,
                            methods: t.Methods.Select(m => new CoverageMethodResult(m.Name, "MOCK.SOURCE", 1, 0, 10, 0, m.VisitCount)).ToList().AsReadOnly()
                        )).ToList().AsReadOnly()
                    )).ToList().AsReadOnly());
        }

    }

    public class CoverageAssemblyBuilder
    {
        private CoverageMockBuilder ParentBuilder { get; }

        private AssemblyMock _currentAssembly;
        private TypeMock _currentType;

        private bool _currentTypeAlreadyAdded = true;

        public List<TypeMock> Types { get; } = new List<TypeMock>();

        internal CoverageAssemblyBuilder(CoverageMockBuilder parent, AssemblyMock currentAssembly, TypeMock currentType)
        {
            ParentBuilder = parent;
            _currentAssembly = currentAssembly;
            _currentType = currentType;
        }


        public CoverageMockBuilder WithAssembly(string assemblyName)
        {
            _currentAssembly.Types.AddRange(Types);
            if (!_currentTypeAlreadyAdded)
            {
                _currentAssembly.Types.Add(_currentType);
            }
            return ParentBuilder.WithAssembly(assemblyName);
        }

        public CoverageAssemblyBuilder WithType(string typeName)
        {
            if (!_currentTypeAlreadyAdded)
            {
                Types.Add(_currentType);
            }
            _currentTypeAlreadyAdded = false;

            _currentType = new TypeMock { Name = typeName };
            return this;
        }

        public CoverageAssemblyBuilder WithMethod(string methodName, int visitCount)
        {
            _currentType.Methods.Add(new MethodMock { Name = methodName, VisitCount = visitCount });
            return this;
        }

        public CoverageResult Mock()
        {
            return ParentBuilder.Mock();
        }
    }

    public class AssemblyMock
    {
        public string Name { get; set; }

        public List<TypeMock> Types { get; set; } = new List<TypeMock>();
    }

    public class TypeMock
    {
        public string Name { get; set; }
        public List<MethodMock> Methods { get; set; } = new List<MethodMock>();
    }

    public class MethodMock
    {
        public string Name { get; set; }
        public int VisitCount { get; set; }
    }

}
