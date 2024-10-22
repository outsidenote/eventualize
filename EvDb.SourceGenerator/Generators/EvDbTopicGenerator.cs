// Ignore Spelling: Topic

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EvDbTopicGenerator : BaseGenerator
{
    internal const string TOPIC_ATT = "EvDbTopicAttribute";
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

        AttributeData attOfTopic = typeSymbol.GetAttributes()
                          .First(att => att.AttributeClass?.Name == TOPIC_ATT);

        string topicsName = typeSymbol.Name;

        ITypeSymbol? factoryTypeSymbol = attOfTopic.AttributeClass?.TypeArguments.First();
        #region Validation

            if (factoryTypeSymbol == null)
                context.Throw(
                    EvDbErrorsNumbers.FactoryTypeNotFound,
                    $"The factory type for topic [{topicsName}]",
                    syntax);

        #endregion //  Validation

        string factoryOriginName = factoryTypeSymbol!.Name;
        string factoryName = $"EvDb{factoryOriginName}";
        string streamName = factoryName;
        string ns = typeSymbol.ContainingNamespace.ToDisplayString();
        if (factoryName.EndsWith("Factory"))
            streamName = factoryName.Substring(0, factoryName.Length - 7);
        if (streamName == factoryOriginName)
            streamName = $"{streamName}1";

        IEnumerable<PayloadInfo> eventsPayloads = factoryTypeSymbol.GetPayloadsFromFatory();

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

        bool hasDefaultTopic = messageTypes
        .Any(m => m.HasDefaultTopic || m.Topics.Length == 0);

        #region Topic Context

        #region addMessageTypes = ...

        var addMessageTypes =
            messageTypes
                .Where(m => m.Topics.Length == 0 || m.HasDefaultTopic)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    var tableNames = TopicToTables(EvDbTopic.DEFAULT_TOPIC);
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
                    var tableNames = TopicToTables("{{{info.Topics[0]}}}");
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

                public void Add({{info.FullTypeName}} payload, TopicsOf{{info.TypeName}} topic)
                {
                    string topicText = topic switch
                        {
            {{string.Join(",", info.Topics.Select(t =>
            $$"""

                            TopicsOf{{info.TypeName}}.{{t.FixNameForClass()}} => "{{t}}"
            """))}}                
                        };

                    var tableNames = TopicToTables(topicText);
                    foreach (var tableName in tableNames)
                    {
                        base.Add(payload, topicText, tableName);
                    }
                }
            
            """);

        #endregion //  addMessageTypesMultiTopic = ...

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine($"using static {ns}.{streamName}TableMatching;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    public sealed class {{topicsName}}Context : EvDbTopicContextBase
                    {
                        public {{topicsName}}Context(
                            EvDbStream evDbStream,
                            IEvDbEventMeta relatedEventMeta)
                                    : base(evDbStream, relatedEventMeta)
                        {
                        }
                    {{string.Join("", addMessageTypes)}}
                    {{string.Join("", addMessageTypesSingleTopic)}}
                    {{string.Join("", addMessageTypesMultiTopic)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix( "Context"), builder.ToString());

        #endregion // Topic Context

        #region AllTopicsEnum
        var allTopics = multiTopics.SelectMany(t => t.Topics).Distinct().OrderBy(x => x).ToList();
        if (hasDefaultTopic) allTopics.Insert(0, "DEFAULT");
        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();
        builder.AppendLine($$"""
                    public enum {{streamName}}TopicOptions
                    {
                    {{string.Join(",", allTopics.Select(t =>
                    $$"""

                            {{t.FixNameForClass()}}   
                        """))}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{streamName}TopicOptions"), builder.ToString());
        #endregion AllTopicsEnum

        #region Stream Topic Extensions

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();
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

                            public {{streamName}}TopicDefinition WithTransformation<T>() where T : IEvDbTopicTransformer
                            {
                                return this;
                            }
                        }

                        public class {{streamName}}TopicDefinition
                        {
                            public {{streamName}}TopicDefinitionContext CreateTopicGroup(string groupName, {{streamName}}TopicOptions topics, params {{streamName}}TopicOptions[] additionalTopics)
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

            builder.AppendLine($$"""
                    public enum TopicsOf{{info.TypeName}}
                    {
                    {{string.Join(",", info.Topics.Select(t =>
                        $$"""

                            {{t.FixNameForClass()}}   
                        """))}}
                    }
                    """);
            context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"TopicsOf{info.TypeName}"), builder.ToString());
        }

        #endregion // Multi Topics Enum

        #region Topic Base

        var addTopicHandlers =
            eventsPayloads.Select((info, i) =>
            $$"""

                protected virtual void ProduceTopicMessages(
                    {{info.FullTypeName}} payload,
                    IEvDbEventMeta meta,
                    {{streamName}}Views views,
                    {{topicsName}}Context topics)
                {
                }
            
            """);

        var addTopicCases =
            eventsPayloads.Select((info, i) =>
            $$"""

                        case "{{info.SnapshotStorageName}}":
                            {
                                var payload = c.GetData<{{info.FullTypeName}}>(_serializationOptions);
                                ProduceTopicMessages(payload, e, states, topics);
                                break;
                            }
                        
            """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    public abstract class {{topicsName}}Base : IEvDbTopicProducer
                    {
                        private readonly {{streamName}} _evDbStream;
                        private readonly JsonSerializerOptions? _serializationOptions;
                        protected const string DefaultTableNameForTopic = "ev-db-topic";

                        public {{topicsName}}Base({{streamName}} evDbStream)
                        {
                            _evDbStream = evDbStream;
                            _serializationOptions = _evDbStream.Options;
                        }

                                            /// <summary>
                        /// Keep in mind the table name will be prefixed using the EvDbStorageContext conventions.
                        /// </summary>
                        /// <param name="topic"></param>
                        /// <remarks></remarks>
                        /// <returns></returns>
                        protected virtual string[] TopicToTables(string topic) => [DefaultTableNameForTopic];

                        void IEvDbTopicProducer.OnProduceTopicMessages(
                            EvDbEvent e,
                            IImmutableList<IEvDbViewStore> views)
                        {
                            {{streamName}}Views states = _evDbStream.Views;
                            {{topicsName}}Context topics = new(_evDbStream, e);
                            IEvDbEventConverter c = e;
                            switch (e.EventType)
                            {
                    {{string.Join("", addTopicCases)}}
                            }
                        }
                    {{string.Join("", addTopicHandlers)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix( "Base"), builder.ToString());

        #endregion // Topic Base

        #region Topic

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial class {{topicsName}} : {{topicsName}}Base
                    {
                        public {{topicsName}}({{streamName}} evDbStream): base(evDbStream)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(), builder.ToString());

        #endregion // Topic
    }

    #endregion // OnGenerate
}
