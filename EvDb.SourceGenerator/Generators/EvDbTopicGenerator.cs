﻿// Ignore Spelling: Topic

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

        var multiTopics = messageTypes
                .Where(m => m.Topics.Length > 1 || m.Topics.Length == 1 && m.HasDefaultTopic);

        #region Topic Context

        #region addMessageTypes = ...

        var addMessageTypes =
            messageTypes
                .Where(m => m.Topics.Length == 0 || m.HasDefaultTopic)
                .Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    base.Add(payload, EvDbTopic.DEFAULT_TOPIC);
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
                    base.Add(payload, "{{info.Topics[0]}}");
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
                    base.Add(payload, topicText);
                }
            
            """);

        #endregion //  addMessageTypesMultiTopic = ...

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
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
