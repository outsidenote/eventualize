using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator.Helpers;

internal static class DomainRoslynExtensions
{
    #region AppendHeader

    public static void AppendHeader(this StringBuilder builder,
                                    TypeDeclarationSyntax syntax,
                                    INamedTypeSymbol typeSymbol)
    {
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

        var ns = typeSymbol.ContainingNamespace.ToDisplayString();
        if (ns != null)
            builder.AppendLine($"namespace {ns};");

        var innerusingLines = syntax.GetUsingWithinNamespace();
        foreach (var line in innerusingLines)
        {
            builder.AppendLine(line.Trim());
        }
    }

    #endregion // AppendHeader
}
