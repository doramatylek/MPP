using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace TestGenerator.Core;

public static class CodeGenerator
{
    public static IEnumerable<TestClassInfo> GenerateTests(string sourceCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            return Enumerable.Empty<TestClassInfo>();
        }
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = syntaxTree.GetRoot();

        var sourceNamespace = root.DescendantNodes()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Select(n => n.Name.ToString())
            .FirstOrDefault() ?? "";

        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        var results = new List<TestClassInfo>();

        foreach (var classDeclaration in classes)
        {

            var className = classDeclaration.Identifier.Text;

            if (string.IsNullOrWhiteSpace(className) || className == "<missing>")
            {
                continue;
            }
            var testClassName = $"{className}Tests";

            var currentClassNamespace = classDeclaration.Ancestors()
                .OfType<BaseNamespaceDeclarationSyntax>() 
                .Select(n => n.Name.ToString())
                .FirstOrDefault() ?? "";

            var constructor = classDeclaration.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .OrderByDescending(c => c.ParameterList.Parameters.Count)
                .FirstOrDefault();

            var dependencies = new List<(string Type, string Name)>();
            if (constructor != null)
            {
                foreach (var param in constructor.ParameterList.Parameters)
                {
                    var typeName = param.Type?.ToString();
                    if (typeName != null && typeName.StartsWith("I"))
                    {
                        dependencies.Add((typeName, param.Identifier.Text));
                    }
                }
            }

            var methods = classDeclaration.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)));

            string generatedCode = BuildTestClass(className, testClassName, dependencies, methods, currentClassNamespace);

            var formattedCode = CSharpSyntaxTree.ParseText(generatedCode)
                .GetRoot()
                .NormalizeWhitespace()
                .ToFullString();

            results.Add(new TestClassInfo
            {
                FileName = $"{testClassName}.cs",
                Code = formattedCode
            });
        }

        return results;
    }

    private static string BuildTestClass(string className, string testClassName,
        List<(string Type, string Name)> dependencies, IEnumerable<MethodDeclarationSyntax> methods, string sourceNamespace)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Xunit;");
        sb.AppendLine("using Moq;");

        if (!string.IsNullOrEmpty(sourceNamespace))
        {
            sb.AppendLine($"using {sourceNamespace};");
        }

        sb.AppendLine();
        sb.AppendLine("namespace GeneratedTests");
        sb.AppendLine("{");
        sb.AppendLine($"    public class {testClassName}");
        sb.AppendLine("    {");

        sb.AppendLine($"        private readonly {className} _myClassUnderTest;");
        foreach (var dep in dependencies)
        {
            sb.AppendLine($"        private readonly Mock<{dep.Type}> _{dep.Name}Mock;");
        }
        sb.AppendLine();

        sb.AppendLine($"        public {testClassName}()");
        sb.AppendLine("        {");
        foreach (var dep in dependencies)
        {
            sb.AppendLine($"            _{dep.Name}Mock = new Mock<{dep.Type}>();");
        }

        var args = string.Join(", ", dependencies.Select(d => $"_{d.Name}Mock.Object"));
        sb.AppendLine($"            _myClassUnderTest = new {className}({args});");
        sb.AppendLine("        }");
        sb.AppendLine();

        var methodCounts = new Dictionary<string, int>();

        foreach (var method in methods)
        {
            var methodName = method.Identifier.Text;
            if (!methodCounts.ContainsKey(methodName)) methodCounts[methodName] = 0;
            methodCounts[methodName]++;

            var testMethodName = methodCounts[methodName] > 1
                ? $"{methodName}Test{methodCounts[methodName]}"
                : $"{methodName}Test";

            sb.AppendLine("        [Fact]");
            sb.AppendLine($"        public void {testMethodName}()");
            sb.AppendLine("        {");

            var parameters = method.ParameterList.Parameters;
            var callArgs = new List<string>();
            foreach (var param in parameters)
            {
                var pType = param.Type?.ToString() ?? "object";
                var pName = param.Identifier.Text;

                string defValue = pType switch
                {
                    "int" or "double" or "float" or "decimal" or "long" => "0",
                    "string" => "\"autogenerated\"",
                    "bool" => "false",
                    _ => "default"
                };

                sb.AppendLine($"            {pType} {pName} = {defValue};");
                callArgs.Add(pName);
            }

            var returnTypeStr = method.ReturnType.ToString();
            var isVoid = returnTypeStr == "void";
            var methodCall = $"_myClassUnderTest.{methodName}({string.Join(", ", callArgs)});";

            if (!isVoid)
            {
                sb.AppendLine($"            var actual = {methodCall}");
            }
            else
            {
                sb.AppendLine($"            {methodCall}");
            }

            if (!isVoid)
            {
                sb.AppendLine($"            {returnTypeStr} expected = default;");
                sb.AppendLine($"            Assert.Equal(expected, actual);");
            }

            sb.AppendLine("            Assert.Fail(\"autogenerated\");");

            sb.AppendLine("        }");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}