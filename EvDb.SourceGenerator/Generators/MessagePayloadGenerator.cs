// Ignore Spelling: Inline

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class MessagePayloadGenerator : EventPayloadGenerator
{
    private const string MESSAGE_PAYLOAD = "EvDbDefineMessagePayload";
    public const string MESSAGE_PAYLOAD_ATTRIBUTE = MESSAGE_PAYLOAD + "Attribute";
    private const string CHANNEL_ATT = "EvDbAttachChannelAttribute";
    private const string DEFAULT_CHANNEL_ATT = "EvDbAttachDefaultChannelAttribute";
    protected override string EventTargetAttribute { get; } = MESSAGE_PAYLOAD_ATTRIBUTE;
    protected override string StartWith { get; } = MESSAGE_PAYLOAD;

    #region BeforeClassDeclaration

    protected override void BeforeClassDeclaration(StringBuilder builder)
    {
        // builder.AppendLine("using EvDb.Core.Internals;");
    }

    #endregion //  BeforeClassDeclaration

    #region GetInlineAdditions

    protected override string GetInlineAdditions(INamedTypeSymbol typeSymbol, string type, string name)
    {
        var allAttributes = typeSymbol.GetAttributes();
        var channels = allAttributes
                          .Where(att => att.AttributeClass?.Name == CHANNEL_ATT)
                          .Select(m => m.ConstructorArguments.First().Value?.ToString() ?? "")
                                .OrderBy(m => m)
                                .Select(t => $$"""

                                        public static readonly EvDbChannelName {{t.FixNameForClass()}} = "{{t.FixNameForClass()}}";  
                                """)
                                .ToList();

        bool hasDefaultChannel = channels.Count == 0 ||
                                allAttributes.Any(att => att.AttributeClass?.Name == DEFAULT_CHANNEL_ATT);
        if (hasDefaultChannel)
        {
            channels.Insert(0, $$"""

                                        public static readonly EvDbChannelName DEFAULT = EvDbOutboxConstants.DEFAULT_OUTBOX;  
                                """);
        }

        string channelValues = string.Join("", channels);

        string result = ($$"""
                        public static class Channels
                        {
                    {{channelValues}}
                        }
                    """);
        return result;
    }

    #endregion //  GetInlineAdditions

    #region GetExternalAdditions

    protected override void GetExternalAdditions(SourceProductionContext context,
                                             TypeDeclarationSyntax syntax,
                                             INamedTypeSymbol typeSymbol,
                                             string type,
                                             string name)
    {
        string enums = GetEnum(typeSymbol);
        if (string.IsNullOrEmpty(enums))
            return;

        StringBuilder builder = new StringBuilder();

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("#pragma warning disable SYSLIB1037 // Deserialization of init-only properties is currently not supported in source generation mode.");
        BeforeClassDeclaration(builder);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    
                    {{enums}}
                    """);

        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{typeSymbol.Name}Channels.g"), builder.ToString());
    }

    #endregion //  GetExternalAdditions

    #region GetEnum

    private static string GetEnum(INamedTypeSymbol typeSymbol)
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
                    public enum {{typeSymbol.Name}}Channels
                    {
                    {{channelEnums}}
                    }
                    """);
        return result;
    }

    #endregion //  GetEnum
}