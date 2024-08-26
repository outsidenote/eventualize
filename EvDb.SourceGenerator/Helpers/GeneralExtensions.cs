using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EvDb.SourceGenerator.Helpers;
internal static class GeneralExtensions
{
    public static string StandardNsAndPath(
        this INamedTypeSymbol typeSymbol,
        string @namespace,
        params string[] path)
    {
        path = path ?? Array.Empty<string>();
        return typeSymbol.StandardNsAndPath(@namespace, (IEnumerable<string>)path);
    }

    private static string StandardNsAndPath(
        this INamedTypeSymbol typeSymbol,
        string @namespace,
        IEnumerable<string> path)
    {
        if (!(path?.Any() ?? false)) 
        {
            path = new[] { typeSymbol.Name };
        }

        var flattenPath = string.Join(@"\", path);
        var result = $@"{@namespace}\{flattenPath}.generated.cs";
        return result;
    }

    public static string StandardNs(
        this INamedTypeSymbol typeSymbol,
        string @namespace)
    {
        string[] path = new[] { typeSymbol.Name };
        var result = typeSymbol.StandardNsAndPath(@namespace, path);

        return result;
    }


    public static string StandardPath(
        this INamedTypeSymbol typeSymbol,
        params string[] path)
    {
        path = path ?? Array.Empty<string>();
        string @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var result = typeSymbol.StandardNsAndPath(@namespace, path);
        return result;
    }

    public static string StandardDefaultAndPath(
        this INamedTypeSymbol typeSymbol,
        params string[] path)
    {
        IEnumerable<string> concatPath = new[] { typeSymbol.Name };
        concatPath = concatPath.Concat(path ?? Array.Empty<string>());
        string @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
        var result = typeSymbol.StandardNsAndPath(@namespace, concatPath);
        return result;
    }

    //public static string StandardPathAndDefault(
    //    this INamedTypeSymbol typeSymbol,
    //    params string[] path)
    //{
    //    path = path ?? Array.Empty<string>();
    //    IEnumerable<string> concatPath = new[] { typeSymbol.Name };
    //    concatPath = path.Concat(concatPath);
    //    string @namespace = typeSymbol.ContainingNamespace.ToDisplayString();
    //    var result = typeSymbol.StandardNsAndPath(@namespace, concatPath);
    //    return result;
    //}

    public static void ThrowIfNotPartial(
        this SourceProductionContext context,
        INamedTypeSymbol typeSymbol,
        TypeDeclarationSyntax syntax)
    {
        if (!syntax.IsPartial())
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor($"EvDb: {EvDbErrorsNumbers.MissingPartialClass}", "class must be partial",
                $"{typeSymbol.Name}, Must be partial", "EvDb",
                DiagnosticSeverity.Error, isEnabledByDefault: true),
                Location.Create(syntax.SyntaxTree, syntax.Span));
            //var content = $"`Factory {typeSymbol.Name}` MUST BE A partial interface!";
            //context.AddSource(typeSymbol.StandardDefaultAndPath("partial-is-missing"), content);
            context.ReportDiagnostic(diagnostic);
        }
    }

    public static void ThrowIf(
        this SourceProductionContext context,
        bool condition,
        EvDbErrorsNumbers errorNumber,
        string desctiption,
        TypeDeclarationSyntax syntax)
    {
        if (!condition)
            return;
        context.Throw(errorNumber, desctiption, syntax);
    }

    public static void Throw(
        this SourceProductionContext context,
        EvDbErrorsNumbers errorNumber,
        string desctiption,
        TypeDeclarationSyntax syntax)
    {
        if (!syntax.IsPartial())
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor($"EvDb: {errorNumber}", desctiption,
                desctiption, "EvDb",
                DiagnosticSeverity.Error, isEnabledByDefault: true),
                Location.Create(syntax.SyntaxTree, syntax.Span));
            context.ReportDiagnostic(diagnostic);
        }
    }

}
