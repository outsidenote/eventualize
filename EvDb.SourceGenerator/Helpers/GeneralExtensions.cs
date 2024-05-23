using Microsoft.CodeAnalysis;

namespace EvDb.SourceGenerator.Helpers;
internal static class GeneralExtensions
{
    public static string GenFileNameSuffix(
        this INamedTypeSymbol typeSymbol,
        string name,
        string generator,
        string suffix = "",
        string prefix = "")
    {
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

        var result = $"{name}{suffix}.{namespaceName}.{generator}.generated.cs";
        return result;
    }

    public static string GenFileName(
        this INamedTypeSymbol typeSymbol,
        string generator,
        string suffix = "",
        string prefix = "")
    {
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

        var result = $"{prefix}{typeSymbol.Name}{suffix}.{namespaceName}.{generator}.generated.cs";
        return result;
    }
}
