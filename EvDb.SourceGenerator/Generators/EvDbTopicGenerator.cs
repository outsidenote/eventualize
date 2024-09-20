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

        #region Topic Context

        var addMessageTypes =
            messageTypes
                .Where(m => m.Topics.Length == 0)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    base.Add(payload, DEFAULT_TOPIC);
                }
            
            """);

        var addTopics = from info in messageTypes
                        where info.Topics.Length != 0
                        from topic in info.Topics
                        group info by topic into g
                        let tpc = g.Key
                        let clsName = tpc.FixNameForClass()
                        select
                            $$"""
                                public EvDb{{clsName}}Producer {{clsName}} { get; } // = new EvDb{{clsName}}Producer(this);

                                public class EvDb{{clsName}}Producer
                                {
                                    private readonly IEvDbTopicProducerGeneric _producer;
                                    internal EvDb{{clsName}}Producer(IEvDbTopicProducerGeneric producer)
                                    {
                                        _producer = producer; 
                                    }
                            {{string.Join("", g.Select(inf =>
                              $$"""
                                    public void Add({{inf.FullTypeName}} payload)
                                    {
                                        _producer.Add(payload, "{{tpc}}");
                                    }

                            """))}}

                                }
            
                            """;

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    public sealed class {{topicsName}}Context : EvDbTopicContextBase
                    {
                        private const string DEFAULT_TOPIC = "DEFAULT";

                        public {{topicsName}}Context(
                            EvDbStream evDbStream,
                            IEvDbEventMeta relatedEventMeta)
                                    : base(evDbStream, relatedEventMeta)
                        {
                        }
                    {{string.Join("", addMessageTypes)}}
                    {{string.Join("", addTopics)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix( "Context"), builder.ToString());

        #endregion // Topic Context

        #region Topic Base

        var addTopicHandlers =
            eventsPayloads.Select((info, i) =>
            $$"""

                protected virtual void ProduceTopicMessages(
                    {{info.FullTypeName}} payload,
                    {{streamName}}Views views,
                    IEvDbEventMeta meta,
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
                                ProduceTopicMessages(payload, states, e, topics);
                                break;
                            }
                        
            """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    public abstract class {{topicsName}}Base : IEvDbTopicProducer
                    {
                        private readonly EvDbSchoolStream _evDbStream;
                        private readonly JsonSerializerOptions? _serializationOptions;

                        public {{topicsName}}Base({{streamName}} evDbStream)
                        {
                            _evDbStream = evDbStream;
                            _serializationOptions = _evDbStream.Options;
                        }

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
