// Ignore Spelling: Topic

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EvDbOutboxGenerator : BaseGenerator
{
    internal const string OUTBOX_ATT = "EvDbOutboxAttribute";
    internal const string OUTBOX_TABLES_ATT = "EvDbAttachOutboxTablesAttribute";
    private const string OUTBOX_SERIALIZER_ATT = "EvDbApplyOutboxSerializationAttribute";
    private const string MESSAGE_TYPES = "EvDbMessageTypes";
    private const string MESSAGE_TYPES_ATT = "EvDbMessageTypesAttribute";
    protected override string EventTargetAttribute { get; } = OUTBOX_ATT;

    #region OnGenerate

    /// <summary>
    /// Called when [generate].
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="syntax">The syntax.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    protected override void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            TypeDeclarationSyntax syntax,
            CancellationToken cancellationToken)
    {
        context.ThrowIfNotPartial(typeSymbol, syntax);
        StringBuilder builder = new StringBuilder();

        AttributeData? attOfOutboxTables = typeSymbol.GetAttributes()
                          .FirstOrDefault(att => att.AttributeClass?.Name == OUTBOX_TABLES_ATT);

        ITypeSymbol? tablesSymbol = attOfOutboxTables?.AttributeClass?.TypeArguments.First();

        string? tableEnum = tablesSymbol?.Name;
        string? tableEnumNs = tablesSymbol?.ContainingNamespace.ToDisplayString();

        AttributeData attOfOutbox = typeSymbol.GetAttributes()
                          .First(att => att.AttributeClass?.Name == OUTBOX_ATT);

        string outboxName = typeSymbol.Name;

        ITypeSymbol? factoryTypeSymbol = attOfOutbox.AttributeClass?.TypeArguments.First();
        #region Validation

        if (factoryTypeSymbol == null)
            context.Throw(
                EvDbErrorsNumbers.FactoryTypeNotFound,
                $"The factory type for outbox [{outboxName}]",
                syntax);

        #endregion //  Validation

        AttributeData? attOfOutboxSerializer = typeSymbol.GetAttributes()
                          .FirstOrDefault(att => att.AttributeClass?.Name == OUTBOX_SERIALIZER_ATT);

        string factoryOriginName = factoryTypeSymbol!.Name;
        string factoryName = $"EvDb{factoryOriginName}";
        string streamName = factoryName;
        if (factoryName.EndsWith("Factory"))
            streamName = factoryName.Substring(0, factoryName.Length - 7);
        if (streamName == factoryOriginName)
            streamName = $"{streamName}1";

        IEnumerable<PayloadInfo> eventsPayloads = factoryTypeSymbol.GetPayloadsFromFactory();

        #region OutboxTypeInfo[] messageTypes = ..

        OutboxTypeInfo[] messageTypes = typeSymbol.GetAttributes()
                                  .Where(att =>
                                  {
                                      string? name = att.AttributeClass?.Name;
                                      bool match = MESSAGE_TYPES_ATT == name || MESSAGE_TYPES == name;
                                      return match;
                                  })
                                  .Select(a =>
                                        new OutboxTypeInfo(context, syntax, a)).ToArray();
        #endregion // OutboxTypeInfo[] messageTypes = ..

        var multiChannel = messageTypes
                .Where(m => m.Topics.Length > 1 || m.Topics.Length == 1 && m.HasDefaultTopic);

        bool hasDefaultTopic = Array.Exists(messageTypes, m => m.HasDefaultTopic || m.Topics.Length == 0);

        #region Topic Context

        #region addMessageTypes = ...

        var addMessageTypes =
            messageTypes
                .Where(m => m.Topics.Length == 0 || m.HasDefaultTopic)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    var tableNames = _outboxToTables.ChannelToTables({{outboxName}}Channels.DEFAULT);
                    foreach (var tableName in tableNames)
                    {
                        base.Add(payload, EvDbTopic.DEFAULT_TOPIC, tableName); 
                    }
                }
            
            """);

        #endregion //  addMessageTypes = ...

        #region addMessageTypesSingleTopic = ...

        var addMessageTypesSingleTopic =
            messageTypes
                .Where(m => m.Topics.Length == 1 && !m.HasDefaultTopic)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    var tableNames = _outboxToTables.ChannelToTables({{outboxName}}Channels.{{info.Topics[0].FixNameForClass()}});
                    foreach (var tableName in tableNames)
                    {
                        base.Add(payload, "{{info.Topics[0]}}", tableName); 
                    }
                }
            
            """);

        #endregion //  addMessageTypesSingleTopic = ...

        #region addMessageTypesMultiTopic = ...

        var addMessageTypesMultiTopic = multiChannel.Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload, OutboxOf{{info.TypeName}} outbox)
                {
                    string outboxText = outbox switch
                        {
            {{string.Join(",", info.Topics.Select(t =>
            $$"""

                            OutboxOf{{info.TypeName}}.{{t.FixNameForClass()}} => "{{t}}"
            """))}},
                            _ => throw new NotImplementedException()
                        };
            
                    {{outboxName}}Channels outboxTextEnum = outbox switch
                        {
            {{string.Join(",", info.Topics.Select(t =>
            $$"""

                            OutboxOf{{info.TypeName}}.{{t.FixNameForClass()}} => {{outboxName}}Channels.{{t.FixNameForClass()}}
            """))}}                
                        };

                    var tableNames = _outboxToTables.ChannelToTables(outboxTextEnum);
                    foreach (var tableName in tableNames)
                    {
                        base.Add(payload, outboxText, tableName);
                    }
                }
            
            """);

        #endregion //  addMessageTypesMultiTopic = ...

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public sealed class {{outboxName}}Context : EvDbTopicContextBase
                    {
                        private readonly I{{streamName}}ChannelToTables _outboxToTables;

                        public {{outboxName}}Context(
                            EvDbStream evDbStream,
                            IEvDbEventMeta relatedEventMeta,
                            I{{streamName}}ChannelToTables outboxToTables)
                                    : base(evDbStream, relatedEventMeta)
                        {
                            _outboxToTables = outboxToTables;
                        }
                    {{string.Join("", addMessageTypes)}}
                    {{string.Join("", addMessageTypesSingleTopic)}}
                    {{string.Join("", addMessageTypesMultiTopic)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix("Context"), builder.ToString());

        #endregion // Topic Context

        #region AllChannelsEnum

        var allChannels = multiChannel.SelectMany(t => t.Topics).Distinct().OrderBy(x => x).ToList();
        if (hasDefaultTopic) allChannels.Insert(0, "DEFAULT");
        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();
        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    public enum {{outboxName}}Channels
                    {
                    {{string.Join(",", allChannels.Select(t =>
                    $$"""

                            {{t.FixNameForClass()}}   
                        """))}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{outboxName}Channels"), builder.ToString());

        #endregion // AllTopiAllChannelsEnumcsEnum

        #region Stream Topic Extensions

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();
        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public static class {{streamName}}TopicExtensions
                    {

                        public static {{streamName}}FactorySnapshotEntry AddTopics(this {{streamName}}FactorySnapshotEntry context, Action<{{streamName}}TopicDefinition> createTopicGroup)
                        {
                            return context;
                        }

                        public class {{streamName}}TopicDefinitionContext : {{streamName}}TopicDefinition
                        {
                            static internal readonly {{streamName}}TopicDefinitionContext Instance = new();

                            public {{streamName}}TopicDefinition WithTransformation(Func<byte[], byte[]> transform)
                            {
                                return this;
                            }

                            public {{streamName}}TopicDefinition WithTransformation<T>() where T : IEvDbOutboxTransformer
                            {
                                return this;
                            }
                        }

                        public class {{streamName}}TopicDefinition
                        {
                            public {{streamName}}TopicDefinitionContext CreateTopicGroup(string groupName, {{outboxName}}Channels outboxs, params {{outboxName}}Channels[] additionalTopics)
                            {
                                return {{streamName}}TopicDefinitionContext.Instance;
                            }
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{streamName}TopicExtensions"), builder.ToString());

        #endregion Stream Topic Extensions

        #region Multi Topics Enum

        foreach (var info in multiChannel)
        {
            builder.ClearAndAppendHeader(syntax, typeSymbol);
            builder.AppendLine("using EvDb.Core.Internals;");
            builder.AppendLine();

            builder.DefaultsOnType(typeSymbol, false);
            builder.AppendLine($$"""
                    public enum OutboxOf{{info.TypeName}}
                    {
                    {{string.Join(",", info.Topics.Select(t =>
                        $$"""

                            {{t.FixNameForClass()}}   
                        """))}}
                    }
                    """);
            context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"OutboxOf{info.TypeName}"), builder.ToString());
        }

        #endregion // Multi Topics Enum

        #region Topic Base

        #region var addTopicHandlers = ...

        var addTopicHandlers =
            eventsPayloads.Select((info, i) =>
            $$"""

                protected virtual void ProduceTopicMessages(
                    {{info.FullTypeName}} payload,
                    IEvDbEventMeta meta,
                    {{streamName}}Views views,
                    {{outboxName}}Context outboxs)
                {
                }
            
            """);

        #endregion //  var addTopicHandlers = ...

        #region var addTopicCases = ...

        var addTopicCases =
            eventsPayloads.Select((info, i) =>
            $$"""

                        case "{{info.SnapshotStorageName}}":
                            {
                                var payload = c.GetData<{{info.FullTypeName}}>(_serializationOptions);
                                ProduceTopicMessages(payload, e, states, outboxs);
                                break;
                            }
                        
            """);

        #endregion //  var addTopicCases = ...

        bool hasTables = string.IsNullOrEmpty(tableEnum);
        #region string outboxToTables = ...
        string outboxToTables = hasTables
            ? $"""      
                    IEnumerable<EvDbTableName> I{streamName}ChannelToTables.ChannelToTables({outboxName}Channels outbox) => [DefaultTableNameForTopic];
                """
            : $$"""
                    IEnumerable<EvDbTableName> I{{streamName}}ChannelToTables.ChannelToTables({{outboxName}}Channels outbox) 
                    {
                        {{tableEnum}}Preferences[] tbls = ChannelToTables(outbox);
                        if(tbls.Length == 0)
                            return [DefaultTableNameForTopic];
                        IEnumerable<EvDbTableName> result = tbls.ToTablesName();
                        return result;
                    }
                       
                    /// <summary>
                    /// Keep in mind the table name will be prefixed using the EvDbStorageContext conventions.
                    /// </summary>
                    /// <param name="outbox"></param>
                    /// <remarks></remarks>
                    /// <returns></returns>
                    protected virtual {{tableEnum}}Preferences[] ChannelToTables({{outboxName}}Channels outbox) => [];
                """;

        #endregion //  string outboxToTables = ...

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        if (hasTables)
            builder.AppendLine($"using {tableEnumNs};");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public abstract class {{outboxName}}Base : IEvDbTopicProducer, I{{streamName}}ChannelToTables
                    {
                        private readonly {{streamName}} _evDbStream;
                        private readonly JsonSerializerOptions? _serializationOptions;
                        protected static readonly EvDbTableName DefaultTableNameForTopic = EvDbTableName.Default;

                        public {{outboxName}}Base({{streamName}} evDbStream)
                        {
                            _evDbStream = evDbStream;
                            _serializationOptions = _evDbStream.Options;
                        }
                    
                    {{outboxToTables}}

                        void IEvDbTopicProducer.OnProduceTopicMessages(
                            EvDbEvent e,
                            IImmutableList<IEvDbViewStore> views)
                        {
                            {{streamName}}Views states = _evDbStream.Views;
                            {{outboxName}}Context outboxs = new(_evDbStream, e, this);
                            IEvDbEventConverter c = e;
                            switch (e.EventType)
                            {
                    {{string.Join("", addTopicCases)}}
                            }
                        }
                    {{string.Join("", addTopicHandlers)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix("Base"), builder.ToString());

        #endregion // Topic Base

        #region Topic

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    partial class {{outboxName}} : {{outboxName}}Base
                    {
                        public {{outboxName}}({{streamName}} evDbStream): base(evDbStream)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(), builder.ToString());

        #endregion // Topic

        #region ChannelToTables

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    /// <summary>
                    /// Map outbox to tables.
                    /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
                    /// </summary>
                    public interface I{{streamName}}ChannelToTables
                    {
                        /// <summary>
                        /// Map outbox to tables.
                        /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
                        /// </summary>
                        /// <param name="outbox">The outbox.</param>
                        /// <returns></returns>
                        IEnumerable<EvDbTableName> ChannelToTables({{outboxName}}Channels outbox);
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"I{streamName}ChannelToTables"),
                    builder.ToString());

        #endregion // ChannelToTables
    }

    #endregion // OnGenerate
}
