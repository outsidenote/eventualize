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
public partial class EventPayloadTypesGenerator : BaseGenerator
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
                Location.None);
            builder.AppendLine($"""
                `interface {typeSymbol.Name}` MUST BE A partial interface!
                """);
            context.AddSource($"{typeSymbol.Name}.generated.cs", builder.ToString());
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
                  select att.ArgumentList.Arguments[0].ToString();
        var key = payloadName.FirstOrDefault();
        if (key == null)
            return;

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    partial {{type}} {{name}}: IEvDbEventPayload
                    {
                        string IEvDbEventPayload.EventType { get; } = {{key}};
                    }                
                    """);

        context.AddSource($"{typeSymbol.Name}.generated.payload.cs", builder.ToString());
    }

    #endregion // OnGenerate
}