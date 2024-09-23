#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class PayloadGenerator : BaseGenerator
{
    protected override string EventTargetAttribute { get; } = "EvDbDefinePayloadAttribute";

    #region OnGenerate

    protected override void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            TypeDeclarationSyntax syntax,
            CancellationToken cancellationToken)
    {
        context.ThrowIfNotPartial(typeSymbol, syntax);

        StringBuilder builder = new StringBuilder();

        string type = typeSymbol.ToType(syntax, cancellationToken);
        string name = typeSymbol.Name;
        var asm = GetType().Assembly.GetName();
        var payloadName = from atts in syntax.AttributeLists
                          from att in atts.Attributes
                          let fn = att.Name.ToFullString()
                          where fn.StartsWith("EvDbDefinePayload")
                          select att.ArgumentList?.Arguments[0].ToString();
        var key = payloadName.FirstOrDefault();
        if (key == null)
            return;

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("#pragma warning disable SYSLIB1037 // Deserialization of init-only properties is currently not supported in source generation mode.");
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] 
                    partial {{type}} {{name}}: IEvDbPayload
                    {
                        [System.Text.Json.Serialization.JsonIgnore]
                        string IEvDbPayload.PayloadType { get; } = {{key}};
                    }                
                    """);

        context.AddSource(typeSymbol.StandardPath(), builder.ToString());
    }

    #endregion // OnGenerate
}