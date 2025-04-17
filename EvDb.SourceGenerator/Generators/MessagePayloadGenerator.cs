#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class MessagePayloadGenerator : EventPayloadGenerator
{
    private const string MESSAGE_PAYLOAD = "EvDbDefineMessagePayload";
    public const string MESSAGE_PAYLOAD_ATTRIBUTE = MESSAGE_PAYLOAD + "Attribute";
    private const string CHANNEL_ATT = "EvDbAttachChannelAttribute";
    protected override string EventTargetAttribute { get; } = MESSAGE_PAYLOAD_ATTRIBUTE;
    protected override string StartWith { get; } = MESSAGE_PAYLOAD;

    #region BeforeClassDeclaration

    protected override void BeforeClassDeclaration(StringBuilder builder)
    {
        // builder.AppendLine("using EvDb.Core.Internals;");
    }

    #endregion //  BeforeClassDeclaration

    #region GetAdditions

    protected override string GetAdditions(INamedTypeSymbol typeSymbol, string type, string name)
    {
        var allAttributes = typeSymbol.GetAttributes();
        string[] channels = allAttributes
                          .Where(att => att.AttributeClass?.Name == CHANNEL_ATT)
                          .Select(m => m.ConstructorArguments.First().Value?.ToString() ?? "")
                                .OrderBy(m => m)
                                .ToArray();

        if (channels.Length == 0)
            return string.Empty;

        string channelEnums = string.Join(",", channels.Select(t =>
                    $$"""

                                {{t.FixNameForClass()}}   
                        """));

        string result = ($$"""
                        public enum Channels
                        {
                    {{channelEnums}}
                        }
                    """);
        return result;
    }

    #endregion //  GetAdditions
}