using Xunit;
using TestGenerator.Core;
using System.Linq;

namespace TestGenerator.Tests
{
    public class CodeGeneratorTests
    {
        [Fact]
        public void GenerateTests_ShouldCreateMultipleFiles_WhenMultipleClassesExist()
        {
            string sourceCode = @"
                namespace NamespaceA { public class Class1 { public void Method1() {} } }
                namespace NamespaceB { public class Class2 { public void Method2() {} } }";

            var results = CodeGenerator.GenerateTests(sourceCode).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal("Class1Tests.cs", results[0].FileName);
            Assert.Equal("Class2Tests.cs", results[1].FileName);
        }

        [Fact]
        public void GenerateTests_ShouldIncludeCorrectUsings_BasedOnNamespace()
        {
            string sourceCode = "namespace TargetNamespace { public class MyClass { public void Do() {} } }";

            var result = CodeGenerator.GenerateTests(sourceCode).First();

            Assert.Contains("using Xunit;", result.Code);
            Assert.Contains("using Moq;", result.Code);
            Assert.Contains("using TargetNamespace;", result.Code);
        }

        [Fact]
        public void GenerateTests_ShouldCreateMocks_ForInterfaceDependencies()
        {
            string sourceCode = @"
                public class Service {
                    public Service(IDatabase db) {}
                    public void Save() {}
                }";

            var result = CodeGenerator.GenerateTests(sourceCode).First();

            Assert.Contains("private readonly Mock<IDatabase> _dbMock;", result.Code);
            Assert.Contains("_dbMock = new Mock<IDatabase>();", result.Code);
        }

        [Fact]
        public void GenerateTests_ShouldOnlyGenerateTests_ForPublicMethods()
        {
            string sourceCode = @"
                public class MyClass {
                    public void PublicMethod() {}
                    private void PrivateMethod() {}
                }";

            var result = CodeGenerator.GenerateTests(sourceCode).First();

            Assert.Contains("PublicMethodTest()", result.Code);
            Assert.DoesNotContain("PrivateMethodTest()", result.Code);
        }

        [Fact]
        public void GenerateTests_ShouldHandleMethodOverloads()
        {
            string sourceCode = @"
                public class MyClass {
                    public void Process() {}
                    public void Process(int id) {}
                }";

            var result = CodeGenerator.GenerateTests(sourceCode).First();

            Assert.Contains("ProcessTest()", result.Code);
            Assert.Contains("ProcessTest2()", result.Code);
        }
        [Fact]
        public void GenerateTests_ShouldReturnEmpty_WhenSourceCodeIsInvalid()
        {

            string invalidSource = "public class { !!! @@@ ### } some random text 123";

            var exception = Record.Exception(() =>
            {
                var results = CodeGenerator.GenerateTests(invalidSource).ToList();

                Assert.Empty(results);
            });

            Assert.Null(exception);
        }

        [Fact]
        public void GenerateTests_ShouldHandleEmptyString()
        {
            string emptySource = "";

            var results = CodeGenerator.GenerateTests(emptySource);

            Assert.Empty(results);
        }
    }
}