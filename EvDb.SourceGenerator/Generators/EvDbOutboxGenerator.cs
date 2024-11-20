// Ignore Spelling: Topic

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;
//using static EvDb.SourceGenerator.EvDbShardsGenerator;

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
        if (!typeSymbol.IsPartial())
            return;

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
        if (outboxArgs.Length > 1)
            shardsSymbol = outboxArgs[1];
        string? shardEnumNs = shardsSymbol?.ContainingNamespace.ToDisplayString();

        (string streamName,
        string streamInterface,
        string factoryOriginName,
        string factoryName,
        string factoryInterface) =
                EvDbGenerator.GetStreamName(factoryTypeSymbol!);

        IEnumerable<PayloadInfo> eventsPayloads = factoryTypeSymbol!.GetPayloadsFromFactory();

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

        OutboxTypeInfo[] multiChannel = messageTypes
                .Where(m => m.Channels.Length > 1 || m.Channels.Length == 1 && m.HasDefaultChannel)
                .ToArray();

        bool hasDefaultChannel = Array.Exists(messageTypes, m => m.HasDefaultChannel || m.Channels.Length == 0);

        #region Outbox Context

        #region addMessageTypes = ...

        var addMessageTypes =
            messageTypes
                .Where(m => m.Channels.Length == 0 || m.HasDefaultChannel)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    var shardNames = _outboxToShards.ChannelToShards({{outboxName}}.Channels.DEFAULT);
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
                    var shardNames = _outboxToShards.ChannelToShards({{outboxName}}.Channels.{{info.Channels[0].FixNameForClass()}});
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

                public void Add({{info.FullTypeName}} payload, {{info.TypeName}}.Channels outbox)
                {
                    string outboxText = outbox switch
                        {
            {{string.Join(",", info.Channels.Select(t =>
            $$"""

                            {{info.TypeName}}.Channels.{{t.FixNameForClass()}} => "{{t}}"
            """))}},
                            _ => throw new NotImplementedException()
                        };
            
                    {{outboxName}}.Channels outboxTextEnum = outbox switch
                        {
            {{string.Join(",", info.Channels.Select(t =>
            $$"""

                            {{info.TypeName}}.Channels.{{t.FixNameForClass()}} => {{outboxName}}.Channels.{{t.FixNameForClass()}}
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
                        private readonly I{{outboxName}}ChannelToShards _outboxToShards;

                        protected override IImmutableList<IEvDbOutboxSerializer> OutboxSerializers { get; } = 
                                                    ImmutableArray<IEvDbOutboxSerializer>.Empty.AddRange(
                                                                        (IEnumerable<IEvDbOutboxSerializer>)
                                                                        [{{string.Join(",", crateSerializers)}}]);

                        public {{outboxName}}Context(
                            ILogger logger, 
                            EvDbStream evDbStream,
                            IEvDbEventMeta relatedEventMeta,
                            I{{outboxName}}ChannelToShards outboxToShards)
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
                            public {{streamName}}OutboxDefinitionContext CreateShard(string groupName, {{outboxName}}.Channels outboxs, params {{outboxName}}.Channels[] additionalOutboxs)
                            {
                                return {{streamName}}OutboxDefinitionContext.Instance;
                            }
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{streamName}OutboxExtensions"), builder.ToString());

        #endregion Stream Outbox Extensions

        #region Outbox Base

        #region var addProduceOutboxMessages = ...

        var addProduceOutboxMessages =
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

        #endregion //  var addProduceOutboxMessages = ...

        #region var addChannelsCases = ...

        var addChannelsCases =
            eventsPayloads.Select((info, i) =>
            $$"""

                        case "{{info.SnapshotStorageName}}":
                            {
                                var payload = c.GetData<{{info.FullTypeName}}>(_serializationOptions);
                                ProduceOutboxMessages(payload, e, states, outboxs);
                                break;
                            }
                        
            """);

        #endregion //  var addChannelsCases = ...

        #region string shardEnum = ..., shardConvert = ...

        string shardEnum = string.Empty;
        string shardConvert = string.Empty;
        bool hasShards = shardsSymbol != null;
        if (hasShards)
        {
            string shardName = shardsSymbol!.Name;
            ShardInfo[] shardsConstants = shardsSymbol.GetMembers()
                         .Where(m => m.Kind == SymbolKind.Field)
                         .Select(m => (IFieldSymbol)m)
                         .Select(m => new ShardInfo(m))
                         .ToArray();

            shardEnum = $$"""
                        public enum Shards
                        {
                    {{string.Join(",", shardsConstants.Select(t =>
                        $$"""

                                {{t.Name}}   
                        """))}}
                        }
                    """;

            string shardConvertSwitch = string.Join("", shardsConstants.Select(t =>
                    $$"""

                                    Shards.{{t.Name}} => (string){{shardName}}.{{t.Name}},
                        """));

            shardConvert = $$"""
                        private IEnumerable<EvDbShardName> ToShardsName(Shards[] options) => options.Select(m => ToShardName(m));

                        private EvDbShardName ToShardName(Shards option)
                        {
                            string table = option switch
                            {
                    {{shardConvertSwitch}}
                                _ => throw new NotSupportedException(),
                            };
                            return table;
                        }
                    """;
        }

        #endregion // string shardEnum = , shardConvert = ...

        #region string outboxToShards = ...

        string outboxToShards = !hasShards
            ? $"""      
                    IEnumerable<EvDbShardName> I{outboxName}ChannelToShards.ChannelToShards({outboxName}Channels outbox) => [DefaultShardNameForTopic];
                """
            : $$"""
                    IEnumerable<EvDbShardName> I{{outboxName}}ChannelToShards.ChannelToShards(Channels outbox) 
                    {
                        Shards[] shards = ChannelToShards(outbox);
                        if(shards.Length == 0)
                            return [DefaultShardNameForTopic];
                        IEnumerable<EvDbShardName> result = ToShardsName(shards);
                        return result;
                    }
                       
                    /// <summary>
                    /// Keep in mind the table name will be prefixed using the EvDbStorageContext conventions.
                    /// </summary>
                    /// <param name="outbox"></param>
                    /// <remarks></remarks>
                    /// <returns></returns>
                    protected virtual Shards[] ChannelToShards(Channels outbox) => [];
                """;

        #endregion //  string outboxToShards = ...

        #region AllChannelsEnum

        List<string> allChannels = multiChannel.SelectMany(t => t.Channels).Distinct().OrderBy(x => x).ToList();
        if (hasDefaultChannel) allChannels.Insert(0, "DEFAULT");
        string channelsEnum = string.Empty;
        if (allChannels.Count != 0)
        {
            string channelsEnumItems = string.Join(",", allChannels.Select(t =>
                        $$"""

                                {{t.FixNameForClass()}}   
                        """));
            channelsEnum = $$"""
                            public enum Channels
                            {
                        {{channelsEnumItems}}
                            }
                        
                        """;
        }

        #endregion // AllTopiAllChannelsEnumcsEnum

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        if (hasShards)
            builder.AppendLine($"using {shardEnumNs};");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public abstract class {{outboxName}}Base : IEvDbOutboxProducer, I{{outboxName}}ChannelToShards
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
                    {{string.Join("", addChannelsCases)}}
                            }
                        }
                    {{string.Join("", addProduceOutboxMessages)}}

                    {{shardConvert}}
                    {{channelsEnum}}

                    {{shardEnum}}
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
                    public interface I{{outboxName}}ChannelToShards
                    {
                        /// <summary>
                        /// Map outbox to tables.
                        /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
                        /// </summary>
                        /// <param name="outbox">The outbox.</param>
                        /// <returns></returns>
                        IEnumerable<EvDbShardName> ChannelToShards({{outboxName}}.Channels outbox);
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"I{outboxName}ChannelToShards"),
                    builder.ToString());

        #endregion // ChannelToShards
    }

    #endregion // OnGenerate
}
