using Microsoft.CodeAnalysis;

namespace EvDb.SourceGenerator.Helpers;
internal static class GeneralExtensions
{
    public static string GenFileNameSuffix(
        this INamedTypeSymbol typeSymbol,
        string name,
        string generator,
        string suffix = "",
        string prefix = "",
        string? overrideNamespace = null)
    {
        string namespaceName = overrideNamespace ?? typeSymbol.ContainingNamespace.ToDisplayString();

        var result = $"{namespaceName}\\{name}{suffix}.{generator}.generated.cs";
        return result;
    }

    public static string GenFileName(
        this INamedTypeSymbol typeSymbol,
        string generator,
        string suffix = "",
        string prefix = "",
        string? overrideNamespace = null)
    {
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

        var result = $"{namespaceName}\\{prefix}{typeSymbol.Name}{suffix}.{generator}.generated.cs";
        return result;
    }
}
