﻿#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EventPayloadGenerator : BaseGenerator
{
    protected override string EventTargetAttribute { get; } = "EvDbEventPayloadAttribute";

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
                `type {typeSymbol.Name}` MUST BE A partial interface!
                """);
            context.AddSource(typeSymbol.GenFileName("payload-not-partial"), builder.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        #endregion // Exception Handling

        string type = typeSymbol.ToType(syntax, cancellationToken);
        string name = typeSymbol.Name;
        var asm = GetType().Assembly.GetName();
        var payloadName = from atts in syntax.AttributeLists
                          from att in atts.Attributes
                          let fn = att.Name.ToFullString()
                          where fn.StartsWith("EvDbEventPayload")
                          select att.ArgumentList?.Arguments[0].ToString();
        var key = payloadName.FirstOrDefault();
        if (key == null)
            return;

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine("#pragma warning disable SYSLIB1037 // Deserialization of init-only properties is currently not supported in source generation mode.");
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    partial {{type}} {{name}}: IEvDbEventPayload
                    {
                        [System.Text.Json.Serialization.JsonIgnore]
                        string IEvDbEventPayload.EventType { get; } = {{key}};
                    }                
                    """);

        context.AddSource(typeSymbol.GenFileName("payload"), builder.ToString());
    }

    #endregion // OnGenerate
}