using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EvDb.Core.Analyzer;

internal static class RoslynExtensions
{
    #region CreateDiagnostic

    public static Diagnostic CreateDiagnostic(
        this INamedTypeSymbol namedTypeSymbol,
        int errorNumber,
        string description,
        string? typeName = "",
        DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        DiagnosticDescriptor descriptor = new($"EvDb: {errorNumber}",
                                                    description,
                                        $"{typeName}: {description}",
                                            "EvDb",
                                                    severity,
                                                    isEnabledByDefault: true);

        var diagnostic = Diagnostic.Create(descriptor, namedTypeSymbol.Locations[0]);
        return diagnostic;
    }

    public static Diagnostic CreateDiagnostic(
        this INamedTypeSymbol namedTypeSymbol,
        DiagnosticDescriptor descriptor)
    {
        var diagnostic = Diagnostic.Create(descriptor, namedTypeSymbol.Locations[0]);
        return diagnostic;
    }

    #endregion //  CreateDiagnostic

    #region IsPartial

    public static bool IsPartial(this TypeDeclarationSyntax syntax) =>
        syntax.Modifiers.Any(SyntaxKind.PartialKeyword);

    public static bool IsPartial(this INamedTypeSymbol symbol)
    {
        bool isPartial = symbol.DeclaringSyntaxReferences
              .Select(syntaxRef => syntaxRef.GetSyntax())
              .OfType<TypeDeclarationSyntax>()
              .Any(declaration => declaration.IsPartial());
        return isPartial;
    }

    #endregion // IsPartial
}