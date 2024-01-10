#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using System.Collections.Immutable;
using System;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using EvDb.SourceGenerator.Helpers;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EventTypesGenerator : IIncrementalGenerator
{
    protected const string EventTargetAttribute = "EvDbEventTypeAttribute";

    private static bool AttributePredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode.MatchAttribute(EventTargetAttribute, cancellationToken);
    }

    #region Initialize

    /// <summary>
    /// Called to initialize the generator and register generation steps via callbacks
    /// on the <paramref name="context" />
    /// </summary>
    /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext" /> to register callbacks on</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<SyntaxAndSymbol> classDeclarations =
                context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: AttributePredicate,
                        transform: static (ctx, _) => ToGenerationInput(ctx))
                    .Where(static m => m is not null);

        IncrementalValueProvider<(Compilation, ImmutableArray<SyntaxAndSymbol>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // register a code generator for the triggers
        context.RegisterSourceOutput(compilationAndClasses, Generate);

        static SyntaxAndSymbol ToGenerationInput(GeneratorSyntaxContext context)
        {
            var declarationSyntax = (TypeDeclarationSyntax)context.Node;

            var symbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax);
            if (symbol is not INamedTypeSymbol namedSymbol)
            {
                throw new NullReferenceException($"Code generated symbol of {nameof(declarationSyntax)} is missing");
            }
            return new SyntaxAndSymbol(declarationSyntax, namedSymbol);
        }

        void Generate(
                       SourceProductionContext spc,
                       (Compilation compilation,
                       ImmutableArray<SyntaxAndSymbol> items) source)
        {
            var (compilation, items) = source;
            foreach (SyntaxAndSymbol item in items)
            {
                OnGenerate(spc, compilation, item);
            }
        }
    }

    #endregion // Initialize

    #region OnGenerate

    private void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            SyntaxAndSymbol input)
    {
        INamedTypeSymbol typeSymbol = input.Symbol;
        TypeDeclarationSyntax syntax = input.Syntax;
        var cancellationToken = context.CancellationToken;
        if (cancellationToken.IsCancellationRequested)
            return;

        StringBuilder builder = new StringBuilder();
        #region Exception Handling

        if (!syntax.IsPartial())
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor("EvDb: 003", "interface must be partial",
                $"{typeSymbol.Name}, Must be partial", "EvDb",
                DiagnosticSeverity.Error, isEnabledByDefault: true),
                Location.None);
            builder.AppendLine($"""
                `interface {typeSymbol.Name}` MUST BE A partial interface!
                """);
            context.AddSource($"{typeSymbol.Name}.generated.cs", builder.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        #endregion // Exception Handling

        builder.AppendHeader(syntax, typeSymbol);

        builder.AppendLine();

        string type = syntax.Keyword.Text;
        string name = typeSymbol.Name.Substring(1);

        var asm = GetType().Assembly.GetName();
        builder.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"{asm.Name}\",\"{asm.Version}\")]");
        var attributes = from att in typeSymbol.GetAttributes()
                   let text = att.AttributeClass.Name
                   where text == EventTargetAttribute
                   let fullName = att.AttributeClass.ToString()
                   let genStart = fullName.IndexOf('<') + 1
                   let genLen = fullName.Length - genStart - 1
                   let generic = fullName.Substring(genStart, genLen)
                   select generic;
        var adds = attributes.Select(m => $"void Add({m} payload, string? capturedBy = null);");

        builder.AppendLine($$"""
                    partial interface {{typeSymbol.Name}}
                    {
                    """);
        foreach (var add in adds)
        {
            builder.AppendLine($"\t{add}");
        }
        builder.AppendLine("}");
        //if(!syntax.part)
        context.AddSource($"{typeSymbol.Name}.generated.cs", builder.ToString());
    }

    #endregion // OnGenerate
}