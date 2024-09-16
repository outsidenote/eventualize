// Ignore Spelling: Outbox

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace EvDb.SourceGenerator;

internal struct OutboxTypeInfo
{
    public OutboxTypeInfo(
            SourceProductionContext context,
            TypeDeclarationSyntax syntax,
            AttributeData outboxTypeAttribute)
    {
        #region string attachedViewTypeFullName = .., attachedViewTypeName = ..

        ImmutableArray<ITypeSymbol> attachedViewArgs = outboxTypeAttribute.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol attachedViewFirstArgSymbol = attachedViewArgs[0];
        string attachedViewTypeFullName = attachedViewFirstArgSymbol.ToDisplayString();
        string attachedViewTypeName = attachedViewFirstArgSymbol.Name;

        #endregion //  string attachedViewTypeFullName = .., attachedViewTypeName = ..


        string ns = attachedViewFirstArgSymbol.ContainingNamespace.ToDisplayString();

        var outBoxTypeAtt = attachedViewFirstArgSymbol.GetAttributes()
                                .First(m => m.AttributeClass?.Name == "EvDbPayloadAttribute");
        string? snapshotStorageName = outBoxTypeAtt?.ConstructorArguments.First().Value?.ToString();
        if (snapshotStorageName == null)
        {
            context.Throw(EvDbErrorsNumbers.MissingViewName, "outbox-type-name on store is missing", syntax);
        }
        FullTypeName = attachedViewTypeFullName;
        TypeName = attachedViewTypeName;
        Namespace = ns;
        SnapshotStorageName = snapshotStorageName;

    }

    /// <summary>
    /// Gets the full name of the view type.
    /// </summary>
    public string FullTypeName { get; }

    /// <summary>
    /// Gets the short name of the view type.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the namespace of the view.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Gets the name of the snapshot type define for usage at the storage level.
    /// Decoupled from the view/snapshot type that is subject for refactoring.
    /// </summary>
    public string SnapshotStorageName { get; }
}