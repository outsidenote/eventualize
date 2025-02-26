using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace EvDb.SourceGenerator.Helpers;

internal static class DomainRoslynExtensions
{
    #region ClearAndAppendHeader

    public static void ClearAndAppendHeader(this StringBuilder builder,
                                    TypeDeclarationSyntax syntax,
                                    INamedTypeSymbol typeSymbol,
                                    string? @namespace = null)
    {
        builder.Clear();
        builder.AppendLine("#nullable enable");

        builder.AppendLine($"#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.");
        builder.AppendLine($"#pragma warning disable CS0105 // Using directive appeared previously in this namespace");
        builder.AppendLine($"#pragma warning disable CS0108 // hides inherited member.");
        builder.AppendLine();
        var usingLines = syntax.GetUsing();
        builder.AppendLine("using System.Collections.Immutable;");
        builder.AppendLine("using System.Text.Json;");
        builder.AppendLine($"// ####################  GENERATED AT: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} ####################");
        foreach (var line in usingLines)
        {
            builder.AppendLine(line.Trim());
        }

        var ns = @namespace ?? typeSymbol.ContainingNamespace.ToDisplayString();
        if (ns != null)
            builder.AppendLine($"namespace {ns};");

        var innerusingLines = syntax.GetUsingWithinNamespace();
        foreach (var line in innerusingLines)
        {
            builder.AppendLine(line.Trim());
        }
    }

    #endregion // ClearAndAppendHeader

    #region DefaultsOnType

    public static StringBuilder DefaultsOnType(
        this StringBuilder builder,
        INamedTypeSymbol typeSymbol,
        bool isClassOrStruct = true,
        string typeXmlDoc = "")
    {
        var asm = typeof(DomainRoslynExtensions).Assembly.GetName();
        builder.AppendLine("// ReSharper disable once UnusedType.Global");
        builder.AppendLine();
        if (!string.IsNullOrWhiteSpace(typeXmlDoc))
            builder.AppendLine(typeXmlDoc);
        builder.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"EvDb.SourceGenerator\",\"{asm.Version}\")]");
        if (isClassOrStruct)
        {
            builder.AppendLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
        }
        builder.AppendLine();
        return builder;
    }

    #endregion //  DefaultsOnType

    #region GetPayloadsFromFatory

    public static IEnumerable<PayloadInfo> GetPayloadsFromFactory(
                                            this ITypeSymbol factoryTypeSymbol)
    {
        AttributeData attOfFactory = factoryTypeSymbol.GetAttributes()
              .First(att => att.AttributeClass?.Name == EvDbGenerator.STREAM_FACTORY_ATT);

        ImmutableArray<ITypeSymbol> args = attOfFactory.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol relatedEventsTypeSymbol = args[0];
        IEnumerable<PayloadInfo> eventsPayloads = relatedEventsTypeSymbol.GetPayloads();
        return eventsPayloads;
    }

    #endregion //  GetPayloadsFromFactory

    #region GetPayloads

    public static IEnumerable<PayloadInfo> GetPayloads(
                                            this ITypeSymbol relatedEventsTypeSymbol)
    {
        var eventsPayloads = from a in relatedEventsTypeSymbol.GetAttributes()
                             let cls = a.AttributeClass!
                             where cls != null
                             let text = cls.Name
                             where text == EventTypesGenerator.EventTarget
                             select new PayloadInfo(cls);
        eventsPayloads = eventsPayloads.ToArray(); // run once
        return eventsPayloads;
    }

    #endregion //  GetPayloads
}
