// Ignore Spelling: Topic

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
            AttributeData topicMessageAttribute)
    {
        #region string attachedViewTypeFullName = .., attachedViewTypeName = ..

        ImmutableArray<ITypeSymbol> attachedViewArgs = topicMessageAttribute.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol attachedViewFirstArgSymbol = attachedViewArgs[0];
        string attachedViewTypeFullName = attachedViewFirstArgSymbol.ToDisplayString();
        string attachedViewTypeName = attachedViewFirstArgSymbol.Name;

        #endregion //  string attachedViewTypeFullName = .., attachedViewTypeName = ..


        string ns = attachedViewFirstArgSymbol.ContainingNamespace.ToDisplayString();

        var attributes = attachedViewFirstArgSymbol.GetAttributes();
        var messageTypeAtt = attributes
                                .First(m => m.AttributeClass?.Name == "EvDbDefinePayloadAttribute");
        string? storageName = messageTypeAtt?.ConstructorArguments.First().Value?.ToString();
        if (storageName == null)
        {
            context.Throw(EvDbErrorsNumbers.MissingViewName, "message name on store is missing", syntax);
        }

        HasDefaultTopic = attributes.Any(m => m.AttributeClass?.Name == "EvDbAttachDefaultChannelAttribute");
        Topics = attributes.Where(m => m.AttributeClass?.Name == "EvDbAttachChannelAttribute")
                                .Select(m => m.ConstructorArguments.First().Value?.ToString() ?? "")
                                .ToArray();
        FullTypeName = attachedViewTypeFullName;
        TypeName = attachedViewTypeName;
        Namespace = ns;
        StorageName = storageName!;
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
    public string StorageName { get; }

    /// <summary>
    /// Gets the attached topics.
    /// </summary>
    public string[] Topics { get; }

    /// <summary>
    /// Gets a value indicating whether it has a default topic.
    /// </summary>
    public bool HasDefaultTopic { get; }
}