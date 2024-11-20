using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace EvDb.SourceGenerator.Helpers;

internal static class RoslynExtensions
{
    #region CreateDiagnostic

    public static Diagnostic CreateDiagnostic(
        this TypeDeclarationSyntax syntax,
        int errorNumber,
        string description,
        string? typeName = "",
        DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor($"EvDb: {errorNumber}", description,
            $"{typeName}: {description}", "EvDb",
            severity, isEnabledByDefault: true),
            Location.Create(syntax.SyntaxTree, syntax.Span));
        return diagnostic;
    }

    #endregion //  CreateDiagnostic

    #region MatchAttribute

    /// <summary>
    /// Check if a type matches an attribute.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static bool MatchAttribute(
                            this SyntaxNode node,
                            string attributeName,
                            CancellationToken cancellationToken)
    {
        if (node is TypeDeclarationSyntax type)
            return type.MatchAttribute(attributeName, cancellationToken);
        return false;
    }

    /// <summary>
    /// Check if a type matches an attribute.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static bool MatchAttribute(
                            this TypeDeclarationSyntax type,
                            string attributeName,
                            CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return false;

        var (attributeName1, attributeName2) = RefineAttributeNames(attributeName);

        bool hasAttributes = type.AttributeLists.Any
                               (m => m.Attributes.Any(m1 =>
                               {
                                   string name = m1.Name.ToString();
                                   int len = name.IndexOf('<');
                                   if (len != -1)
                                       name = name.Substring(0, len);
                                   bool match = name == attributeName1 || name == attributeName2;
                                   return match;
                               }));
        return hasAttributes;
    }

    #endregion // MatchAttribute

    #region RefineAttributeNames

    private static (string attributeName1, string attributeName2) RefineAttributeNames(
                                                                        string attributeName)
    {
        int len = attributeName.LastIndexOf(".");
        if (len != -1)
            attributeName = attributeName.Substring(len + 1);

        string attributeName2 = attributeName.EndsWith("Attribute")
            ? attributeName.Substring(0, attributeName.Length - "Attribute".Length)
            : $"{attributeName}Attribute";
        return (attributeName, attributeName2);
    }

    #endregion // RefineAttributeNames

    #region GetUsingWithinNamespace

    /// <summary>
    /// Gets the using declared within a namespace.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration.</param>
    /// <returns></returns>
    public static IImmutableList<string> GetUsingWithinNamespace(this TypeDeclarationSyntax typeDeclaration)
    {

        var fileScope = typeDeclaration.Parent! as FileScopedNamespaceDeclarationSyntax;
        return fileScope?.Usings.Select(u => u.ToFullString()).ToImmutableList() ?? ImmutableList<string>.Empty;
    }

    #endregion // GetUsingWithinNamespace

    #region GetUsing

    /// <summary>
    /// Gets the using statements (declared before the namespace).
    /// </summary>
    /// <param name="syntaxNode">The syntax node.</param>
    /// <returns></returns>
    public static IEnumerable<string> GetUsing(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is CompilationUnitSyntax m)
        {
            foreach (var u in m.Usings)
            {
                var match = u.ToString();
                yield return match;
            }
        }

        if (syntaxNode.Parent == null)
            yield break;

        foreach (var u in syntaxNode.Parent.GetUsing())
        {
            yield return u;
        }
    }

    #endregion // GetUsing

    #region TryGetValue

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
    public static bool TryGetValue<T>(
                        this INamedTypeSymbol symbol,
                        string attributeName,
                        string name,
                        out T value)
    {
        value = default;
        var atts = symbol.GetAttributes()
                                    .Where(a => a.AttributeClass?.Name == attributeName);
        foreach (var att in atts)
        {
            if (att.TryGetValue(name, out value))
                return true;
        }
        return false;
    }

    public static bool TryGetValue<T>(
                        this IMethodSymbol symbol,
                        string attributeName,
                        string name,
                        out T value)
    {
        value = default;
        var atts = symbol.GetAttributes()
                                    .Where(a => a.AttributeClass?.Name == attributeName);
        foreach (var att in atts)
        {
            if (att.TryGetValue(name, out value))
                return true;
        }
        return false;
    }

    public static bool TryGetValue<T>(
                        this AttributeData? attributeData,
                        string name,
                        out T value)
    {
        value = default;
        if (attributeData == null)
            return false;

        var names = attributeData
                        .AttributeConstructor
                        ?.Parameters
                        .Select(p => p.Name)
                        .ToArray() ?? Array.Empty<string>();

        var prms = attributeData.ConstructorArguments;
        for (int i = 0; i < prms.Length; i++)
        {
            if (string.Compare(names[i], name, true) != 0)
            {
                continue;
            }

            var prm = prms[i];
            value = (T)prm.Value;
            return true;
        }

        var prop = attributeData.NamedArguments
                                .FirstOrDefault(m => m.Key == name);
        var val = prop.Value;
        if (val.IsNull)
            return false;
        value = (T)val.Value;
        return true;
    }
#pragma warning restore CS8601
#pragma warning restore CS8600

    #endregion // TryGetValue

    #region ToSymbol

    /// <summary>
    /// Converts to symbol.
    /// </summary>
    /// <param name="declarationSyntax">The declaration syntax.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static ISymbol? ToSymbol(this SyntaxNode declarationSyntax,
                                     Compilation compilation,
                                     CancellationToken cancellationToken = default)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(declarationSyntax.SyntaxTree);
        ISymbol? symbol = semanticModel.GetDeclaredSymbol(declarationSyntax);
        return symbol;
    }

    #endregion // ToSymbol

    #region GetNestedBaseTypesAndSelf

    /// <summary>
    /// Gets the nested base types and self.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <returns></returns>
    public static IEnumerable<INamedTypeSymbol> GetNestedBaseTypesAndSelf(this INamedTypeSymbol typeSymbol)
    {
        yield return typeSymbol;
        INamedTypeSymbol? baseType = typeSymbol.BaseType;
        while (baseType != null && baseType.SpecialType != SpecialType.System_Object)
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }

    #endregion // GetNestedBaseTypesAndSelf

    #region ToSyntaxNode

    /// <summary>
    /// Converts to syntax-node.
    /// </summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static T? ToSyntaxNode<T>(this ISymbol symbol,
                                                  Compilation compilation,
                                                  CancellationToken cancellationToken = default)
        where T : SyntaxNode
    {
        SyntaxReference? syntaxReference = symbol.DeclaringSyntaxReferences.FirstOrDefault();

        if (syntaxReference == null)
            return null;

        SyntaxTree syntaxTree = syntaxReference.SyntaxTree;
        T? nodeSyntax = syntaxTree.GetRoot(cancellationToken)
            .DescendantNodes()
            .OfType<T>()
            .FirstOrDefault(node =>
            {
                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
                ISymbol? candidateSymbol = semanticModel?.GetDeclaredSymbol(node, cancellationToken);
                if (candidateSymbol == null)
                    return false;
                return SymbolEqualityComparer.Default.Equals(symbol, candidateSymbol);
            });

        return nodeSyntax;
    }

    #endregion // ToSyntaxNode

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

    #region ToType

    public static string ToType(this INamedTypeSymbol typeSymbol, Compilation compilation, CancellationToken cancellationToken)
    {
        var tSyntax = typeSymbol.ToSyntaxNode<TypeDeclarationSyntax>(compilation, cancellationToken);
        var addition = typeSymbol.IsRecord && typeSymbol.TypeKind == TypeKind.Struct ? " struct" : string.Empty;
        return $"{tSyntax!.Keyword}{addition}";
    }

    public static string ToType(this INamedTypeSymbol typeSymbol, TypeDeclarationSyntax syntax, CancellationToken cancellationToken)
    {
        string type = syntax.Keyword.Text;
        string addition = typeSymbol.IsRecord && typeSymbol.TypeKind == TypeKind.Struct ? " struct" : string.Empty;
        return $"{type}{addition}";
    }

    #endregion // ToType
}
