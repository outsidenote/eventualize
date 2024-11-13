// Ignore Spelling: Topic

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EvDbOutboxGenerator : BaseGenerator
{
    internal const string OUTBOX_ATT = "EvDbOutboxAttribute";
    private const string OUTBOX_SERIALIZER_ATT = "EvDbUseOutboxSerializationAttribute";
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

        var allAttributes = typeSymbol.GetAttributes();
        AttributeData attOfOutbox = allAttributes
                          .First(att => att.AttributeClass?.Name == OUTBOX_ATT);
        ImmutableArray<ITypeSymbol> outboxArgs = attOfOutbox.AttributeClass!.TypeArguments;
        string outboxName = typeSymbol.Name;

        ITypeSymbol? factoryTypeSymbol = outboxArgs[0];
        #region Validation

        if (factoryTypeSymbol == null)
            context.Throw(
                EvDbErrorsNumbers.FactoryTypeNotFound,
                $"The factory type for outbox [{outboxName}]",
                syntax);

        #endregion //  Validation

        ITypeSymbol? shardsSymbol = null;
        if(outboxArgs.Length > 1)
            shardsSymbol = outboxArgs[1];
        string? tableEnum = shardsSymbol?.Name;
        string? tableEnumNs = shardsSymbol?.ContainingNamespace.ToDisplayString();

        string factoryOriginName = factoryTypeSymbol!.Name;
        string factoryName = $"EvDb{factoryOriginName}";
        string streamName = factoryName;
        if (factoryName.EndsWith("Factory"))
            streamName = factoryName.Substring(0, factoryName.Length - 7);
        if (streamName == factoryOriginName)
            streamName = $"{streamName}1";

        IEnumerable<PayloadInfo> eventsPayloads = factoryTypeSymbol.GetPayloadsFromFactory();

        #region OutboxTypeInfo[] messageTypes = ..

        OutboxTypeInfo[] messageTypes = allAttributes
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
                .Where(m => m.Channels.Length > 1 || m.Channels.Length == 1 && m.HasDefaultChannel);

        bool hasDefaultTopic = Array.Exists(messageTypes, m => m.HasDefaultChannel || m.Channels.Length == 0);

        #region Outbox Context

        #region addMessageTypes = ...

        var addMessageTypes =
            messageTypes
                .Where(m => m.Channels.Length == 0 || m.HasDefaultChannel)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    var shardNames = _outboxToShards.ChannelToShards({{outboxName}}Channels.DEFAULT);
                    foreach (var shardName in shardNames)
                    {
                        base.Add(payload, EvDbOutbox.DEFAULT_OUTBOX, shardName); 
                    }
                }
            
            """);

        #endregion //  addMessageTypes = ...

        #region addMessageTypesSingleTopic = ...

        var addMessageTypesSingleTopic =
            messageTypes
                .Where(m => m.Channels.Length == 1 && !m.HasDefaultChannel)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    var shardNames = _outboxToShards.ChannelToShards({{outboxName}}Channels.{{info.Channels[0].FixNameForClass()}});
                    foreach (var shardName in shardNames)
                    {
                        base.Add(payload, "{{info.Channels[0]}}", shardName); 
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
            {{string.Join(",", info.Channels.Select(t =>
            $$"""

                            OutboxOf{{info.TypeName}}.{{t.FixNameForClass()}} => "{{t}}"
            """))}},
                            _ => throw new NotImplementedException()
                        };
            
                    {{outboxName}}Channels outboxTextEnum = outbox switch
                        {
            {{string.Join(",", info.Channels.Select(t =>
            $$"""

                            OutboxOf{{info.TypeName}}.{{t.FixNameForClass()}} => {{outboxName}}Channels.{{t.FixNameForClass()}}
            """))}}                
                        };

                    var shardNames = _outboxToShards.ChannelToShards(outboxTextEnum);
                    foreach (var shardName in shardNames)
                    {
                        base.Add(payload, outboxText, shardName);
                    }
                }
            
            """);

        #endregion //  addMessageTypesMultiTopic = ...

        AttributeData? attOfOutboxSerialization = allAttributes
                          .FirstOrDefault(att => att.AttributeClass?.Name == OUTBOX_SERIALIZER_ATT);
        
        #region IEnumerable<string> crateSerializers = ...

        ImmutableArray<ITypeSymbol> serializersArgs = attOfOutboxSerialization?.AttributeClass!.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        IEnumerable<string> crateSerializers = serializersArgs.Select(m => $"new {m.ToDisplayString()}()");

        #endregion //  IEnumerable<string> crateSerializers = ...

        ImmutableArray<TypedConstant> attOfOutboxSerializationCtorArgs = 
                                        attOfOutboxSerialization?.ConstructorArguments ?? ImmutableArray<TypedConstant>.Empty;
        string mode = attOfOutboxSerializationCtorArgs.FirstOrDefault().Value switch
                            {
                                0 => "EvDbOutboxSerializationMode.Permissive",
                                _ => "EvDbOutboxSerializationMode.Strict"
                            };

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core;");
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public sealed class {{outboxName}}Context : EvDbOutboxContextBase
                    {
                        private readonly I{{streamName}}ChannelToShards _outboxToShards;

                        protected override IImmutableList<IEvDbOutboxSerializer> OutboxSerializers { get; } = 
                                                    ImmutableArray<IEvDbOutboxSerializer>.Empty.AddRange(
                                                                        (IEnumerable<IEvDbOutboxSerializer>)
                                                                        [{{string.Join(",", crateSerializers)}}]);

                        public {{outboxName}}Context(
                            ILogger logger, 
                            EvDbStream evDbStream,
                            IEvDbEventMeta relatedEventMeta,
                            I{{streamName}}ChannelToShards outboxToShards)
                                    : base(logger, {{mode}}, evDbStream, relatedEventMeta)
                        {
                            _outboxToShards = outboxToShards;
                        }
                    {{string.Join("", addMessageTypes)}}
                    {{string.Join("", addMessageTypesSingleTopic)}}
                    {{string.Join("", addMessageTypesMultiTopic)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix("Context"), builder.ToString());

        #endregion // Outbox Context

        #region AllChannelsEnum

        var allChannels = multiChannel.SelectMany(t => t.Channels).Distinct().OrderBy(x => x).ToList();
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

        #region Stream Outbox Extensions

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();
        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public static class {{streamName}}OutboxExtensions
                    {

                        public static {{streamName}}FactorySnapshotEntry AddOutbox(this {{streamName}}FactorySnapshotEntry context, Action<{{streamName}}OutboxDefinition> createOutboxGroup)
                        {
                            return context;
                        }

                        public class {{streamName}}OutboxDefinitionContext : {{streamName}}OutboxDefinition
                        {
                            static internal readonly {{streamName}}OutboxDefinitionContext Instance = new();

                            public {{streamName}}OutboxDefinition WithTransformation(Func<byte[], byte[]> transform)
                            {
                                return this;
                            }

                            public {{streamName}}OutboxDefinition WithTransformation<T>() where T : IEvDbOutboxTransformer
                            {
                                return this;
                            }
                        }

                        public class {{streamName}}OutboxDefinition
                        {
                            /// <summary>
                            /// Shard is a data splitting, usually for different messaging frameworks (Like Kafka, NATS, SQS, RabbitMQ, etc.)
                            /// Depending on the storage adapter `shard` might be split into different tables 
                            /// </summary>
                            public {{streamName}}OutboxDefinitionContext CreateShard(string groupName, {{outboxName}}Channels outboxs, params {{outboxName}}Channels[] additionalOutboxs)
                            {
                                return {{streamName}}OutboxDefinitionContext.Instance;
                            }
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{streamName}OutboxExtensions"), builder.ToString());

        #endregion Stream ourbox Extensions

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
                    {{string.Join(",", info.Channels.Select(t =>
                        $$"""

                            {{t.FixNameForClass()}}   
                        """))}}
                    }
                    """);
            context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"OutboxOf{info.TypeName}"), builder.ToString());
        }

        #endregion // Multi Channels Enum

        #region Outbox Base

        #region var addTopicHandlers = ...

        var addTopicHandlers =
            eventsPayloads.Select((info, i) =>
            $$"""

                protected virtual void ProduceOutboxMessages(
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
                                ProduceOutboxMessages(payload, e, states, outboxs);
                                break;
                            }
                        
            """);

        #endregion //  var addTopicCases = ...

        bool hasTables = string.IsNullOrEmpty(tableEnum);
        #region string outboxToShards = ...
        string outboxToShards = hasTables
            ? $"""      
                    IEnumerable<EvDbShardName> I{streamName}ChannelToShards.ChannelToShards({outboxName}Channels outbox) => [DefaultShardNameForTopic];
                """
            : $$"""
                    IEnumerable<EvDbShardName> I{{streamName}}ChannelToShards.ChannelToShards({{outboxName}}Channels outbox) 
                    {
                        {{tableEnum}}Preferences[] tbls = ChannelToShards(outbox);
                        if(tbls.Length == 0)
                            return [DefaultShardNameForTopic];
                        IEnumerable<EvDbShardName> result = tbls.ToShardsName();
                        return result;
                    }
                       
                    /// <summary>
                    /// Keep in mind the table name will be prefixed using the EvDbStorageContext conventions.
                    /// </summary>
                    /// <param name="outbox"></param>
                    /// <remarks></remarks>
                    /// <returns></returns>
                    protected virtual {{tableEnum}}Preferences[] ChannelToShards({{outboxName}}Channels outbox) => [];
                """;

        #endregion //  string outboxToShards = ...

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        if (hasTables)
            builder.AppendLine($"using {tableEnumNs};");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public abstract class {{outboxName}}Base : IEvDbOutboxProducer, I{{streamName}}ChannelToShards
                    {
                        private readonly {{streamName}} _evDbStream;
                        private readonly ILogger _logger;
                        private readonly JsonSerializerOptions? _serializationOptions;
                        protected static readonly EvDbShardName DefaultShardNameForTopic = EvDbShardName.Default;

                        public {{outboxName}}Base(ILogger logger, {{streamName}} evDbStream)
                        {
                            _evDbStream = evDbStream;
                            _serializationOptions = _evDbStream.Options;
                            _logger = logger;
                        }
                    
                    {{outboxToShards}}

                        void IEvDbOutboxProducer.OnProduceOutboxMessages(
                            EvDbEvent e,
                            IImmutableList<IEvDbViewStore> views)
                        {
                            {{streamName}}Views states = _evDbStream.Views;
                            {{outboxName}}Context outboxs = new(_logger, _evDbStream, e, this);
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

        #endregion // Outbox Base

        #region Outbox

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    partial class {{outboxName}} : {{outboxName}}Base
                    {
                        public {{outboxName}}(ILogger logger, {{streamName}} evDbStream): base(logger, evDbStream)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(), builder.ToString());

        #endregion // outbox

        #region ChannelToShards

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    /// <summary>
                    /// Map outbox to tables.
                    /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
                    /// </summary>
                    public interface I{{streamName}}ChannelToShards
                    {
                        /// <summary>
                        /// Map outbox to tables.
                        /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
                        /// </summary>
                        /// <param name="outbox">The outbox.</param>
                        /// <returns></returns>
                        IEnumerable<EvDbShardName> ChannelToShards({{outboxName}}Channels outbox);
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"I{streamName}ChannelToShards"),
                    builder.ToString());

        #endregion // ChannelToShards
    }

    #endregion // OnGenerate
}
