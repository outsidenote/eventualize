// Ignore Spelling: Inline

#pragma warning disable HAA0301 // Closure Allocation Source
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
    protected override string EventTargetAttribute { get; } = "EvDbDefineEventPayloadAttribute";

    protected virtual string StartWith { get; } = "EvDbDefineEventPayload";

    protected virtual string GetInlineAdditions(INamedTypeSymbol typeSymbol, string type, string name) => string.Empty;

    protected virtual void GetExternalAdditions(SourceProductionContext context, TypeDeclarationSyntax syntax, INamedTypeSymbol typeSymbol, string type, string name) { }

    protected virtual void BeforeClassDeclaration(StringBuilder builder) { }

    #region OnGenerate

    protected override void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            TypeDeclarationSyntax syntax,
            CancellationToken cancellationToken)
    {
        if (!typeSymbol.IsPartial())
            return;

        StringBuilder builder = new StringBuilder();

        string type = typeSymbol.ToType(syntax, cancellationToken);
        string name = typeSymbol.Name;

        var payloadName = from atts in syntax.AttributeLists
                          from att in atts.Attributes
                          let fn = att.Name.ToFullString()
                          where fn.StartsWith(StartWith)
                          select att.ArgumentList?.Arguments[0].ToString();
        var key = payloadName.FirstOrDefault();
        if (key == null)
            return;

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("#pragma warning disable SYSLIB1037 // Deserialization of init-only properties is currently not supported in source generation mode.");
        BeforeClassDeclaration(builder);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    partial {{type}} {{name}}: IEvDbPayload
                    {
                        public static string PAYLOAD_TYPE => {{key}};
                        [System.Text.Json.Serialization.JsonIgnore]
                        string IEvDbPayload.PayloadType => PAYLOAD_TYPE;

                    {{GetInlineAdditions(typeSymbol, type, name)}}
                    } 
                    """);

        context.AddSource(typeSymbol.StandardPath(), builder.ToString());

        GetExternalAdditions(context, syntax, typeSymbol, type, name);
    }

    #endregion // OnGenerate
}