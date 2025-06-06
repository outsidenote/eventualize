﻿#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EventTypesGenerator : BaseGenerator
{
    internal const string EventTarget = "EvDbAttachEventTypeAttribute";
    protected override string EventTargetAttribute { get; } = EventTarget;

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

        builder.ClearAndAppendHeader(syntax, typeSymbol);

        builder.AppendLine();

        var attributes = from att in typeSymbol.GetAttributes()
                         let text = att.AttributeClass?.Name
                         where text == EventTargetAttribute
                         let fullName = att.AttributeClass?.ToString()
                         let genStart = fullName.IndexOf('<') + 1
                         let genLen = fullName.Length - genStart - 1
                         let generic = fullName.Substring(genStart, genLen)
                         select generic;
        var adds = attributes.Select(m => $"ValueTask<IEvDbEventMeta> AppendAsync({m} payload, string? capturedBy = null);");

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""       
                    partial interface {{typeSymbol.Name}}: IEvDbEventTypes
                    {
                    """);
        foreach (var add in adds)
        {
            builder.AppendLine($"\t{add}");
        }
        builder.AppendLine("}");
        context.AddSource(typeSymbol.StandardPath(), builder.ToString());
    }

    #endregion // OnGenerate
}