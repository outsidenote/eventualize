using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            path = new[] { typeSymbol.Name }.Concat(path) ;
        }
        else if (!(path?.Any() ?? false))
        {
            path = new[] { typeSymbol.Name };
        }

        var flattenPath = string.Join(seperator, path);
        var result = $@"{@namespace}\{flattenPath}.generated.cs";
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
}
