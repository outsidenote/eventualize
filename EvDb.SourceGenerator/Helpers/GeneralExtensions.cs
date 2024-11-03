using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;

namespace EvDb.SourceGenerator.Helpers;
internal static class GeneralExtensions
{
    public static string StandardPathWithNsIgnoreSymbolName(
        this INamedTypeSymbol typeSymbol,
        string @namespace,
        params string[] path)
    {
        return typeSymbol.StandardPath(@namespace, (IEnumerable<string>)path);
    }

    private static string StandardPath(
        this INamedTypeSymbol typeSymbol,
        string @namespace,
        IEnumerable<string> path,
        bool useSymbolNameAsPrefix = false,
        string seperator = @"\")
    {
        path = path ?? Array.Empty<string>();
        if (useSymbolNameAsPrefix)
        {
            path = new[] { typeSymbol.Name }.Concat(path);
        }
        else if (!(path?.Any() ?? false))
        {
            path = new[] { typeSymbol.Name };
        }

        var flattenPath = string.Join(seperator, path);
        var result = $@"{@namespace}\{flattenPath}.g.cs";
        return result;
    }

    /// <summary>
    /// Produce a standards name from the symbol's name and additional path.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static string StandardPath(
        this INamedTypeSymbol typeSymbol,
        params string[] path)
    {
        string @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var result = typeSymbol.StandardPath(@namespace, path, true);
        return result;
    }

    /// <summary>
    /// Produce a standards name from the symbol's name and additional path.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="separator">The separator.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static string StandardSuffix(
        this INamedTypeSymbol typeSymbol,
        params string[] path)
    {
        string @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var result = typeSymbol.StandardPath(@namespace, path, true, "");
        return result;
    }

    /// <summary>
    /// Produce a standards name from a path, if path is empty, use the synbol's name.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static string StandardPathIgnoreSymbolName(
        this INamedTypeSymbol typeSymbol,
        params string[] path)
    {
        string @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var result = typeSymbol.StandardPathWithNsIgnoreSymbolName(@namespace, path);
        return result;
    }

    public static void ThrowIfNotPartial(
        this SourceProductionContext context,
        INamedTypeSymbol typeSymbol,
        TypeDeclarationSyntax syntax)
    {
        context.ThrowIf(!syntax.IsPartial(),
            EvDbErrorsNumbers.MissingPartialClass,
            $"{typeSymbol.Name}, Must be partial",
            syntax);
    }

    public static void ThrowIf(
        this SourceProductionContext context,
        bool condition,
        EvDbErrorsNumbers errorNumber,
        string description,
        TypeDeclarationSyntax syntax)
    {
        if (!condition)
            return;
        context.Throw(errorNumber, description, syntax);
    }

    public static void Throw(
        this SourceProductionContext context,
        EvDbErrorsNumbers errorNumber,
        string description,
        TypeDeclarationSyntax syntax)
    {
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor($"EvDb ({(int)errorNumber}): {errorNumber}", description,
            description, "EvDb",
            DiagnosticSeverity.Error, isEnabledByDefault: true),
            Location.Create(syntax.SyntaxTree, syntax.Span));
        context.ReportDiagnostic(diagnostic);
    }

    public static string FixNameForClass(this string input)
    {
        // Step 1: Remove invalid characters
        var sanitized = Regex.Replace(input, @"[^a-zA-Z0-9_]", "");
        if (string.IsNullOrEmpty(sanitized))
            return "_";

        // Step 2: Ensure it starts with a letter or underscore
        if (!char.IsLetter(sanitized, 0))
        {
            sanitized = "_" + sanitized;
        }
        else if (char.IsLower(sanitized, 0))
        {
            sanitized = $"{Char.ToUpper(sanitized[0])}{sanitized.Substring(1)}";
        }

        // Step 3: Check if it's a valid identifier (Roslyn check)
        if (!SyntaxFacts.IsValidIdentifier(sanitized))
        {
            throw new ArgumentException($"The name '{sanitized}' is not a valid C# identifier.");
        }

        return sanitized;
    }
}
