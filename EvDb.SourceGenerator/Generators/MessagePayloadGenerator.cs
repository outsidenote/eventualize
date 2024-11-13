#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class MessagePayloadGenerator: EventPayloadGenerator 
{
    private const string MESSAGE_PAYLOAD = "EvDbDefineMessagePayload";
    public const string MESSAGE_PAYLOAD_ATTRIBUTE = MESSAGE_PAYLOAD + "Attribute";
    protected override string EventTargetAttribute { get; } = MESSAGE_PAYLOAD_ATTRIBUTE;
    protected override string StartWith { get; } = MESSAGE_PAYLOAD;

    #region OnGenerate

    protected override void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            TypeDeclarationSyntax syntax,
            CancellationToken cancellationToken)
    {
        base.OnGenerate(context, compilation, typeSymbol, syntax, cancellationToken);
    }

    #endregion // OnGenerate
}