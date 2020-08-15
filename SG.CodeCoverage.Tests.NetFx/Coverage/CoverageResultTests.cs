using Microsoft.VisualStudio.TestTools.UnitTesting;
using SG.CodeCoverage.Coverage;
using SG.CodeCoverage.Tests.NetFx;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.CodeCoverage.Coverage.Tests
{
    [TestClass]
    public class CoverageResultTests
    {
        [TestMethod]
        [Description("Merge should return first assemblies if second one has nothing in it")]
        public void MergeCoverageResultsScenario1()
        {
            // Arrange
            var ver = new Common.VersionInfo(1, 0, 0);
            var guid = Guid.NewGuid();

            var cov1 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 5)
                        .WithMethod("Meth_B", 10)
                    .WithType("Type_B")
                        .WithMethod("Meth_C", 20)
                .WithAssembly("ASM_B")
                    .WithType("Type_B")
                        .WithMethod("Meth_A", 100)
                .Mock();

            var cov2 = CoverageMock.Mock();

            var asms2 = new List<CoverageAssemblyResult>().AsReadOnly();

            // Act
            var merged = cov1.MergeWith(cov2);

            // Assert

            Assert.IsTrue(merged.Assemblies.Count == cov1.Assemblies.Count);
            Assert.IsTrue(merged.Assemblies.Sum(x => x.Types.Count) == cov1.Assemblies.Sum(x => x.Types.Count));
        }

        [TestMethod]
        [Description("Testing a rich scenario")]
        public void MergeCoverageResultsScenario2()
        {
            // Arrange
            var cov1 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 5)
                        .WithMethod("Meth_B", 10)
                    .WithType("Type_B")
                        .WithMethod("Meth_A", 10)
                        .WithMethod("Meth_C", 5)
                .WithAssembly("ASM_B")
                    .WithType("Type_B")
                        .WithMethod("Meth_D", 0)
                 .Mock();

            var cov2 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 0)
                    .WithType("Type_B")
                        .WithMethod("Meth_A", 5)
                .WithAssembly("ASM_C")
                    .WithType("Type_D")
                        .WithMethod("Meth_R", 0)
                .Mock();

            // Act
            var merged = cov1.MergeWith(cov2);

            // Assert
            Assert.IsTrue(merged.FindAssembly("ASM_C") is CoverageAssemblyResult);
            Assert.IsTrue(merged.FindType("ASM_C", "Type_D") is CoverageTypeResult);
            Assert.IsTrue(merged.FindMethod("ASM_C", "Type_D", "Meth_R") is CoverageMethodResult);

            Assert.IsTrue(merged.FindMethod("ASM_A", "Type_A", "Meth_A").VisitCount == 5);
            Assert.IsFalse(merged.FindMethod("ASM_B", "Type_B", "Meth_D").IsVisited);
            Assert.IsTrue(merged.FindMethod("ASM_A", "Type_B", "Meth_A").VisitCount == 15);
        }

        [TestMethod]
        [Description("Merge should sum up visit counts")]
        public void MergeCoverageResultsScenario3()
        {
            // Arrange
            var cov1 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 5)
                 .Mock();

            var cov2 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 10)
                .Mock();

            // Act
            var merged = cov1.MergeWith(cov2);

            // Assert
            Assert.IsTrue(merged.FindMethod("ASM_A", "Type_A", "Meth_A").VisitCount == 15);
        }

        [TestMethod]
        [Description("Merge should add assemblies from both sides")]
        public void MergeCoverageResultsScenario4()
        {
            // Arrange
            var cov1 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 5)
                 .Mock();

            var cov2 = CoverageMock
                .WithAssembly("ASM_B")
                    .WithType("Type_B")
                        .WithMethod("Meth_B", 10)
                .Mock();

            // Act
            var merged = cov1.MergeWith(cov2);

            // Assert
            Assert.IsTrue(merged.Assemblies.Count == 2);
        }

        [TestMethod]
        [Description("Merge should add types from both sides")]
        public void MergeCoverageResultsScenario5()
        {
            // Arrange
            var cov1 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 5)
                 .Mock();

            var cov2 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_B")
                        .WithMethod("Meth_B", 10)
                .Mock();

            // Act
            var merged = cov1.MergeWith(cov2);

            // Assert
            Assert.IsTrue(merged.FindAssembly("ASM_A").Types.Count == 2);
        }

        [TestMethod]
        [Description("Merge should add methods from both sides")]
        public void MergeCoverageResultsScenario6()
        {
            // Arrange
            var cov1 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_A", 5)
                 .Mock();

            var cov2 = CoverageMock
                .WithAssembly("ASM_A")
                    .WithType("Type_A")
                        .WithMethod("Meth_B", 10)
                .Mock();

            // Act
            var merged = cov1.MergeWith(cov2);

            // Assert
            Assert.IsTrue(merged.FindType("ASM_A", "Type_A").Methods.Count == 2);
        }
    }
}