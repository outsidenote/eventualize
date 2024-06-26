﻿#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EventAdderGenerator : BaseGenerator
{
    internal const string EventTarget = "EvDbEventAdderAttribute";
    protected override string EventTargetAttribute { get; } = EventTarget;

    #region OnGenerate

    protected override void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            TypeDeclarationSyntax syntax,
            CancellationToken cancellationToken)
    {
        StringBuilder builder = new StringBuilder();
        #region Exception Handling

        if (!syntax.IsPartial())
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor("EvDb: 003", "interface must be partial",
                $"{typeSymbol.Name}, Must be partial", "EvDb",
                DiagnosticSeverity.Error, isEnabledByDefault: true),
                Location.Create(syntax.SyntaxTree, syntax.Span));
            builder.AppendLine($"""
                `interface {typeSymbol.Name}` MUST BE A partial interface!
                """);
            context.AddSource(typeSymbol.GenFileName("adder-not-partial"), builder.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        #endregion // Exception Handling

        builder.AppendHeader(syntax, typeSymbol);

        builder.AppendLine();

        string name = typeSymbol.Name.Substring(1);

        var asm = GetType().Assembly.GetName();
        var attributes = from att in typeSymbol.GetAttributes()
                         let text = att.AttributeClass?.Name
                         where text == EventTargetAttribute
                         let fullName = att.AttributeClass?.ToString()
                         let genStart = fullName.IndexOf('<') + 1
                         let genLen = fullName.Length - genStart - 1
                         let generic = fullName.Substring(genStart, genLen)
                         select generic;
        var adds = attributes.Select(m => $"ValueTask<IEvDbEventMeta> AddAsync({m} payload, string? capturedBy = null);");

        builder.AppendLine($$"""
                    partial interface {{typeSymbol.Name}}: IEvDbEventAdder
                    {
                    """);
        foreach (var add in adds)
        {
            builder.AppendLine($"\t{add}");
        }
        builder.AppendLine("}");
        //if(!syntax.part)
        context.AddSource(typeSymbol.GenFileName("adder"), builder.ToString());
    }

    #endregion // OnGenerate
}