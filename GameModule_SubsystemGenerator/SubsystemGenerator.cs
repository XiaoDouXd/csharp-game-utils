using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace SubsystemGenerator
{
    [Generator]
    public class SubsystemGenerator : ISourceGenerator
    {
        private const string OutputNamespace = "XD.A0.Game.Runtime.Subsystems.Base";

        public void Initialize(GeneratorInitializationContext context)
        {
            // 注册自定义语法接收器
            context.RegisterForSyntaxNotifications(() => new SubsystemSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // 获取收集到的语法节点
            if (context.SyntaxReceiver is not SubsystemSyntaxReceiver receiver)
                return;

            foreach (var classDecl in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
                if (!IsValidSubsystemClass(classDecl, model)) continue;
                GenerateRegistrationCode(context, classDecl, model);
            }
        }

        private static bool IsValidSubsystemClass(ClassDeclarationSyntax classDecl, SemanticModel model)
        {
            return HasSubsystemAttribute(classDecl, model, "XD.GameModule.Subsystem.SubsystemAttribute.SubsystemAttribute()");
        }

        private static bool InheritsSubsystemBase(ClassDeclarationSyntax classDecl, SemanticModel model)
        {
            var classSymbol = model.GetDeclaredSymbol(classDecl);
            return classSymbol?.BaseType?.ToDisplayString() == "XD.GameModule.Subsystem.SubsystemBase";
        }

        private static bool HasParameterlessConstructor(ClassDeclarationSyntax classDecl, SemanticModel model)
        {
            var classSymbol = model.GetDeclaredSymbol(classDecl);
            return classSymbol?.Constructors.Any(c =>
                c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public) ?? false;
        }

        private static bool HasSubsystemAttribute(ClassDeclarationSyntax classDecl, SemanticModel model, string attributeName)
        {
            foreach (var attributeList in classDecl.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeSymbol = model.GetSymbolInfo(attribute).Symbol;
                    if (attributeSymbol?.ToString() != attributeName) continue;
                        return true;
                }
            }
            return false;
        }

        private static void GenerateRegistrationCode(GeneratorExecutionContext context, ClassDeclarationSyntax classDecl, SemanticModel model)
        {
            if (!InheritsSubsystemBase(classDecl, model))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SG001", "Generation Error",
                        $"Code generation failed: subsystem must inherits SubsystemBase", "Subsystem",
                        DiagnosticSeverity.Error, true),
                    Location.None));
                return;
            }

            if (!HasParameterlessConstructor(classDecl, model))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SG001", "Generation Error",
                        $"Code generation failed: subsystem must have parameter-less constructor", "Subsystem",
                        DiagnosticSeverity.Error, true),
                    Location.None));
                return;
            }

            try
            {
                // 3. 获取类元数据
                var className = classDecl.Identifier.Text;
                var instName = "____subsystem_g_" + className;

                var ns = GetNamespace(classDecl);
                var layerValue = GetLayerValue(classDecl);

                var isLazy = HasSubsystemAttribute(classDecl, model, "XD.GameModule.Subsystem.SubsystemLazyAttribute.SubsystemLazyAttribute()");

                // 4. 构建注册代码
                var code = isLazy ? $@"namespace {OutputNamespace}
{{
    public static partial class Sys
    {{
        [XD.GameModule.Subsystem.SubsystemInstDisposeOnly]
        private static {ns}.{className} {instName};
        public static {ns}.{className} {className}
        {{
            get
            {{
                if ({instName} == null)
                {{
                    {instName} = new {ns}.{className}();
                    {instName}.OnInitialize();
                    System.Threading.Tasks.Task.Run(() => {instName}.OnAsyncInitialize(new System.Threading.CancellationTokenSource()));
                }}
                return {instName};
            }}

            set
            {{
                if (value != null) throw new System.ArgumentException(""subsystem cannot be initialized outside of the Sys"");
                if ({instName} == null) return;
                {instName}.Dispose();
                {instName} = null;
            }}
        }}
    }}
}}" : $@"namespace {OutputNamespace}
{{
    public static partial class Sys
    {{
        [XD.GameModule.Subsystem.SubsystemInst(Layer = {layerValue})]
        private static {ns}.{className} {instName};
        public static {ns}.{className} {className} => {instName};
    }}
}}";

                context.AddSource($"{className}_SubsystemRegistration.g.cs",
                    SourceText.From(code, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SG001", "Generation Error",
                        $"Code generation failed: {ex.Message}", "Subsystem",
                        DiagnosticSeverity.Error, true),
                    Location.None));
            }
        }

        // 辅助方法：获取命名空间
        private static string GetNamespace(SyntaxNode node)
        {
            var namespaceDecl = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            return namespaceDecl?.Name.ToString() ?? "Global";
        }

        // 辅助方法：获取Layer参数值
        private static int GetLayerValue(ClassDeclarationSyntax classDecl)
        {
            foreach (var attributeList in classDecl.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (attribute.Name.ToString() != "Subsystem") continue;
                    if (attribute.ArgumentList != null)
                        return int.Parse(attribute.ArgumentList.Arguments
                            .First(a => a.NameEquals?.Name.Identifier.Text == "Layer")
                            .Expression.ToString());
                }
            }
            return 0;
        }
    }

    // 自定义语法接收器
    internal class SubsystemSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // 收集所有包含属性的类声明
            if (syntaxNode is not ClassDeclarationSyntax classDecl || classDecl.AttributeLists.Count <= 0) return;
            CandidateClasses.Add(classDecl);
        }
    }
}