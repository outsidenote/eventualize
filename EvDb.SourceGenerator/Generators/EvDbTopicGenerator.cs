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
public partial class EvDbTopicGenerator : BaseGenerator
{
    internal const string TOPIC_ATT = "EvDbOutboxAttribute";
    internal const string TOPIC_TABLES_ATT = "EvDbAttachOutboxTablesAttribute";
    private const string MESSAGE_TYPES = "EvDbMessageTypes";
    private const string MESSAGE_TYPES_ATT = "EvDbMessageTypesAttribute";
    protected override string EventTargetAttribute { get; } = TOPIC_ATT;

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

        AttributeData? attOfTopicTables = typeSymbol.GetAttributes()
                          .FirstOrDefault(att => att.AttributeClass?.Name == TOPIC_TABLES_ATT);

        ITypeSymbol? tablesSymbol = attOfTopicTables?.AttributeClass?.TypeArguments.First();

        string? tableEnum = tablesSymbol?.Name;
        string? tableEnumNs = tablesSymbol?.ContainingNamespace.ToDisplayString();

        AttributeData attOfTopic = typeSymbol.GetAttributes()
                          .First(att => att.AttributeClass?.Name == TOPIC_ATT);

        string outboxsName = typeSymbol.Name;

        ITypeSymbol? factoryTypeSymbol = attOfTopic.AttributeClass?.TypeArguments.First();
        #region Validation

        if (factoryTypeSymbol == null)
            context.Throw(
                EvDbErrorsNumbers.FactoryTypeNotFound,
                $"The factory type for outbox [{outboxsName}]",
                syntax);

        #endregion //  Validation

        string factoryOriginName = factoryTypeSymbol!.Name;
        string factoryName = $"EvDb{factoryOriginName}";
        string streamName = factoryName;
        if (factoryName.EndsWith("Factory"))
            streamName = factoryName.Substring(0, factoryName.Length - 7);
        if (streamName == factoryOriginName)
            streamName = $"{streamName}1";

        IEnumerable<PayloadInfo> eventsPayloads = factoryTypeSymbol.GetPayloadsFromFactory();

        #region TopicTypeInfo[] messageTypes = ..

        TopicTypeInfo[] messageTypes = typeSymbol.GetAttributes()
                                  .Where(att =>
                                  {
                                      string? name = att.AttributeClass?.Name;
                                      bool match = MESSAGE_TYPES_ATT == name || MESSAGE_TYPES == name;
                                      return match;
                                  })
                                  .Select(a =>
                                        new TopicTypeInfo(context, syntax, a)).ToArray();
        #endregion // TopicTypeInfo[] messageTypes = ..

        var multiTopics = messageTypes
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
                    var tableNames = _outboxToTables.TopicToTables({{streamName}}OutboxOptions.DEFAULT);
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
                    var tableNames = _outboxToTables.TopicToTables({{streamName}}OutboxOptions.{{info.Topics[0].FixNameForClass()}});
                    foreach (var tableName in tableNames)
                    {
                        base.Add(payload, "{{info.Topics[0]}}", tableName); 
                    }
                }
            
            """);

        #endregion //  addMessageTypesSingleTopic = ...

        #region addMessageTypesMultiTopic = ...

        var addMessageTypesMultiTopic = multiTopics.Select((info, i) =>
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
            
                    {{streamName}}OutboxOptions outboxTextEnum = outbox switch
                        {
            {{string.Join(",", info.Topics.Select(t =>
            $$"""

                            OutboxOf{{info.TypeName}}.{{t.FixNameForClass()}} => {{streamName}}OutboxOptions.{{t.FixNameForClass()}}
            """))}}                
                        };

                    var tableNames = _outboxToTables.TopicToTables(outboxTextEnum);
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
                    public sealed class {{outboxsName}}Context : EvDbTopicContextBase
                    {
                        private readonly I{{streamName}}TopicToTables _outboxToTables;

                        public {{outboxsName}}Context(
                            EvDbStream evDbStream,
                            IEvDbEventMeta relatedEventMeta,
                            I{{streamName}}TopicToTables outboxToTables)
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

        #region AllTopicsEnum

        var allTopics = multiTopics.SelectMany(t => t.Topics).Distinct().OrderBy(x => x).ToList();
        if (hasDefaultTopic) allTopics.Insert(0, "DEFAULT");
        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();
        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    public enum {{streamName}}OutboxOptions
                    {
                    {{string.Join(",", allTopics.Select(t =>
                    $$"""

                            {{t.FixNameForClass()}}   
                        """))}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{streamName}OutboxOptions"), builder.ToString());

        #endregion // AllTopicsEnum

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
                            public {{streamName}}TopicDefinitionContext CreateTopicGroup(string groupName, {{streamName}}OutboxOptions outboxs, params {{streamName}}OutboxOptions[] additionalTopics)
                            {
                                return {{streamName}}TopicDefinitionContext.Instance;
                            }
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{streamName}TopicExtensions"), builder.ToString());

        #endregion Stream Topic Extensions

        #region Multi Topics Enum

        foreach (var info in multiTopics)
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
                    {{outboxsName}}Context outboxs)
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
                    IEnumerable<EvDbTableName> I{streamName}TopicToTables.TopicToTables({streamName}OutboxOptions outbox) => [DefaultTableNameForTopic];
                """
            : $$"""
                    IEnumerable<EvDbTableName> I{{streamName}}TopicToTables.TopicToTables({{streamName}}OutboxOptions outbox) 
                    {
                        {{tableEnum}}Preferences[] tbls = TopicToTables(outbox);
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
                    protected virtual {{tableEnum}}Preferences[] TopicToTables({{streamName}}OutboxOptions outbox) => [];
                """;

        #endregion //  string outboxToTables = ...

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        if (hasTables)
            builder.AppendLine($"using {tableEnumNs};");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public abstract class {{outboxsName}}Base : IEvDbTopicProducer, I{{streamName}}TopicToTables
                    {
                        private readonly {{streamName}} _evDbStream;
                        private readonly JsonSerializerOptions? _serializationOptions;
                        protected static readonly EvDbTableName DefaultTableNameForTopic = EvDbTableName.Default;

                        public {{outboxsName}}Base({{streamName}} evDbStream)
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
                            {{outboxsName}}Context outboxs = new(_evDbStream, e, this);
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
                    partial class {{outboxsName}} : {{outboxsName}}Base
                    {
                        public {{outboxsName}}({{streamName}} evDbStream): base(evDbStream)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(), builder.ToString());

        #endregion // Topic

        #region TopicToTables

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    /// <summary>
                    /// Map outbox to tables.
                    /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
                    /// </summary>
                    public interface I{{streamName}}TopicToTables
                    {
                        /// <summary>
                        /// Map outbox to tables.
                        /// Keep in mind that the actual table names will be prefixed according to the context specified in the `EvDbStorageContext` 
                        /// </summary>
                        /// <param name="outbox">The outbox.</param>
                        /// <returns></returns>
                        IEnumerable<EvDbTableName> TopicToTables({{streamName}}OutboxOptions outbox);
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"I{streamName}TopicToTables"),
                    builder.ToString());

        #endregion // TopicToTables
    }

    #endregion // OnGenerate
}
