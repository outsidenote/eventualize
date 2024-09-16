#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace EvDb.SourceGenerator;

internal struct ViewInfo
{
    public ViewInfo(
            SourceProductionContext context,
            TypeDeclarationSyntax syntax,
            AttributeData viewAttribute)
    {
        #region string attachedViewTypeFullName = .., attachedViewTypeName = ..

        ImmutableArray<ITypeSymbol> attachedViewArgs = viewAttribute.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol attachedViewFirstArgSymbol = attachedViewArgs[0];
        string attachedViewTypeFullName = attachedViewFirstArgSymbol.ToDisplayString();
        string attachedViewTypeName = attachedViewFirstArgSymbol.Name;

        #endregion //  string attachedViewTypeFullName = .., attachedViewTypeName = ..

        #region attachedViewPropName = ..

        TypedConstant attachedViewPropNameConst = viewAttribute.ConstructorArguments.FirstOrDefault();
        string? attachedViewPropName = attachedViewPropNameConst.Value?.ToString();
        if (attachedViewPropName == null)
        {
            attachedViewPropName = attachedViewFirstArgSymbol.Name;
            if (attachedViewPropName.EndsWith("View") && attachedViewPropName.Length != 4)
                attachedViewPropName = attachedViewPropName.Substring(0, attachedViewPropName.Length - 4);
        }

        #endregion //  attachedViewPropName = ..

        var viewAtt = attachedViewFirstArgSymbol.GetAttributes()
                                  .First(a =>
                                          a.AttributeClass?.Name == ViewGenerator.VIEW_TYPE_ATT);
        string ns = attachedViewFirstArgSymbol.ContainingNamespace.ToDisplayString();

        ImmutableArray<ITypeSymbol> viewStateTypeSymbol = viewAtt.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        string viewStateType = viewStateTypeSymbol[0].ToDisplayString();

        if (!viewAtt.TryGetValue("name", out string snapshotStorageName))
        {
            context.Throw(EvDbErrorsNumbers.MissingViewName, "view-name on store is missing", syntax);
        }
        ViewPropName = attachedViewPropName;
        ViewTypeFullName = attachedViewTypeFullName;
        ViewTypeName = attachedViewTypeName;
        ViewStateTypeFullName = viewStateType;
        Namespace = ns;
        SnapshotStorageName = snapshotStorageName;

        RelatedEventsSymbol = viewStateTypeSymbol[1];
        RelatedEventsTypeFullName = RelatedEventsSymbol.ToDisplayString();
    }

    /// <summary>
    /// Gets the name of the view property.
    /// It either the user defined property or a convention based on the view type name
    /// </summary>
    public string ViewPropName { get; }

    /// <summary>
    /// Gets the full name of the view type.
    /// </summary>
    public string ViewTypeFullName { get; }

    /// <summary>
    /// Gets the short name of the view type.
    /// </summary>
    public string ViewTypeName { get; }

    /// <summary>
    /// Gets the full name of the type of the view's state.
    /// </summary>
    public string ViewStateTypeFullName { get; }

    /// <summary>
    /// Gets the namespace of the view.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// Gets the name of the snapshot type define for usage at the storage level.
    /// Decoupled from the view/snapshot type that is subject for refactoring.
    /// </summary>
    public string SnapshotStorageName { get; }

    /// <summary>
    /// Gets the related events that is part of the folding logic.
    /// </summary>
    public string RelatedEventsTypeFullName { get; }

    /// <summary>
    /// Gets the related events that is part of the folding logic.
    /// </summary>
    public ITypeSymbol RelatedEventsSymbol { get; }
}
