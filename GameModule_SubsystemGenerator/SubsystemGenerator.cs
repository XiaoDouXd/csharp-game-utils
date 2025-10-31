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
        private struct ClassTypeSymbol
        {
            // ReSharper disable NotAccessedField.Local
            public INamedTypeSymbol Class;
            public SemanticModel ClassModel;
            public ClassDeclarationSyntax ClassSyntax;

            public INamedTypeSymbol SubsystemAttr;
            public INamedTypeSymbol LazySubsystemAttr;

            public AttributeData SubsystemAttrData;
            // ReSharper restore NotAccessedField.Local
        }

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

            var symbol = new ClassTypeSymbol
            {
                SubsystemAttr = context.Compilation.GetTypeByMetadataName("XD.GameModule.Subsystem.SubsystemAttribute")!,
                LazySubsystemAttr = context.Compilation.GetTypeByMetadataName("XD.GameModule.Subsystem.SubsystemLazyAttribute")!
            };

            if (symbol.SubsystemAttr == null || symbol.LazySubsystemAttr == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SG002", "Generation Error",
                        $"Code generation failed: can't find subsystem attributes", "Subsystem",
                        DiagnosticSeverity.Warning, true),
                    Location.None));
                return;
            }

            foreach (var classDecl in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);
                symbol.ClassModel = model;
                symbol.ClassSyntax = classDecl;
                symbol.Class = model.GetDeclaredSymbol(classDecl)!;
                if (symbol.Class == null) continue;

                var attrData = HasSubsystemAttribute(symbol.Class, symbol.SubsystemAttr);
                if (attrData == null) continue;
                symbol.SubsystemAttrData = attrData;
                GenerateRegistrationCode(context, symbol);
            }
        }

        private static ITypeSymbol? GetParentTypeFromAttribute(AttributeData attr)
        {
            if (attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is ITypeSymbol symbol)
                return symbol;
            return null;
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

        private static AttributeData? HasSubsystemAttribute(INamedTypeSymbol classSymbol, INamedTypeSymbol attrSymbol)
        {
            foreach (var attr in classSymbol.GetAttributes())
            {
                if (attr.AttributeClass?.Equals(attrSymbol, SymbolEqualityComparer.Default) ?? false)
                    return attr;
            }
            return null;
        }

        private static void GenerateRegistrationCode(GeneratorExecutionContext context, in ClassTypeSymbol symbol)
        {
            if (!InheritsSubsystemBase(symbol.ClassSyntax, symbol.ClassModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SG001", "Generation Error",
                        $"Code generation failed: subsystem must inherits SubsystemBase", "Subsystem",
                        DiagnosticSeverity.Error, true),
                    Location.None));
                return;
            }

            if (!HasParameterlessConstructor(symbol.ClassSyntax, symbol.ClassModel))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("SG001", "Generation Error",
                        $"Code generation failed: subsystem must have parameter-less constructor", "Subsystem",
                        DiagnosticSeverity.Error, true),
                    symbol.ClassSyntax.Identifier.GetLocation()));
                return;
            }

            try
            {
                // 3. 获取类元数据
                var className = symbol.ClassSyntax.Identifier.Text;
                var instName = "____subsystem_g_" + className;

                var ns = GetNamespace(symbol.ClassSyntax);
                var layerValue = GetLayerValue(symbol.ClassSyntax);
                var isLazy = HasSubsystemAttribute(symbol.Class, symbol.LazySubsystemAttr) != null;

                var parenType = GetParentTypeFromAttribute(symbol.SubsystemAttrData);
                if (parenType == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("SG002", "Generation Error",
                            $"Code generation failed: parse parent type failed", "Subsystem",
                            DiagnosticSeverity.Error, true),
                        symbol.ClassSyntax.Identifier.GetLocation()));
                    return;
                }

                if (!parenType.IsStatic)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("SG003", "Generation Error",
                            $"Code generation failed: parent type is not static, {parenType.ToDisplayString()}", "Subsystem",
                            DiagnosticSeverity.Error, true),
                        symbol.ClassSyntax.Identifier.GetLocation()));
                    return;
                }

                // 4. 构建注册代码
                var opNamespace = parenType.ContainingNamespace.ToDisplayString();
                var code = isLazy ? $@"namespace {opNamespace}
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
}}" : $@"namespace {opNamespace}
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
                        $"Code generation failed: {ex.Message}, stack:\n{ex.StackTrace}", "Subsystem",
                        DiagnosticSeverity.Error, true),
                    symbol.ClassSyntax.GetLocation()));
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
                    if (attribute.ArgumentList == null) continue;
                    var layer = attribute.ArgumentList.Arguments
                        .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Layer");
                    return layer == null ? 0 : int.Parse(layer.Expression.ToString());
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