// Ignore Spelling: Outbox

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
public partial class EvDbOutboxGenerator : BaseGenerator
{
    internal const string OUTBOX_ATT = "EvDbOutboxAttribute";
    private const string OUTBOX_TYPES = "EvDbOutboxTypes";
    private const string OUTBOX_TYPES_ATT = "EvDbOutboxTypesAttribute";
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

        string factoryOriginName = factoryTypeSymbol!.Name;
        string factoryName = $"EvDb{factoryOriginName}";
        string streamName = factoryName;
        if (factoryName.EndsWith("Factory"))
            streamName = factoryName.Substring(0, factoryName.Length - 7);
        if (streamName == factoryOriginName)
            streamName = $"{streamName}1";

        IEnumerable<PayloadInfo> eventsPayloads = factoryTypeSymbol.GetPayloadsFromFatory();

        #region OutboxTypeInfo[] outboxTypes = ..

        OutboxTypeInfo[] outboxTypes = typeSymbol.GetAttributes()
                                  .Where(att =>
                                  {
                                      string? name = att.AttributeClass?.Name;
                                      bool match = OUTBOX_TYPES_ATT == name || OUTBOX_TYPES == name;
                                      return match;
                                  })
                                  .Select(a =>
                                        new OutboxTypeInfo(context, syntax, a)).ToArray();
        #endregion // OutboxTypeInfo[] outboxTypes = ..

        #region Outbox Context

        var addOutboxTypes =
            outboxTypes.Select((info, i) =>
            $$"""

                public void Add({{info.FullTypeName}} payload)
                {
                    base.Add(payload);
                }
            
            """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    public sealed class {{outboxName}}Context : OutboxHandlerBase
                    {
                        public {{outboxName}}Context(
                            EvDbStream evDbStream,
                            IEvDbEventMeta relatedEventMeta)
                                    : base(evDbStream, relatedEventMeta)
                        {
                        }
                    {{string.Join("", addOutboxTypes)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix( "Context"), builder.ToString());

        #endregion // Outbox Context

        #region Outbox Base

        var addOutboxHandlers =
            eventsPayloads.Select((info, i) =>
            $$"""

                protected virtual void OutboxHandler(
                    {{info.FullTypeName}} payload,
                    {{streamName}}Views views,
                    IEvDbEventMeta meta,
                    {{outboxName}}Context outbox)
                {
                }
            
            """);

        var addOutboxCases =
            eventsPayloads.Select((info, i) =>
            $$"""

                        case "{{info.SnapshotStorageName}}":
                            {
                                var payload = c.GetData<{{info.FullTypeName}}>(_serializationOptions);
                                OutboxHandler(payload, states, e, outbox);
                                break;
                            }
                        
            """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    public abstract class {{outboxName}}Base : IEvDbOutboxHandler
                    {
                        private readonly EvDbSchoolStream _evDbStream;
                        private readonly JsonSerializerOptions? _serializationOptions;

                        public {{outboxName}}Base({{streamName}} evDbStream)
                        {
                            _evDbStream = evDbStream;
                            _serializationOptions = _evDbStream.Options;
                        }

                        void IEvDbOutboxHandler.OnOutboxHandler(
                            EvDbEvent e,
                            IImmutableList<IEvDbViewStore> views)
                        {
                            {{streamName}}Views states = _evDbStream.Views;
                            {{outboxName}}Context outbox = new(_evDbStream, e);
                            IEvDbEventConverter c = e;
                            switch (e.EventType)
                            {
                    {{string.Join("", addOutboxCases)}}
                            }
                        }
                    {{string.Join("", addOutboxHandlers)}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardSuffix( "Base"), builder.ToString());

        #endregion // Outbox Base

        #region Outbox

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial class {{outboxName}} : {{outboxName}}Base
                    {
                        public {{outboxName}}({{streamName}} evDbStream): base(evDbStream)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(), builder.ToString());

        #endregion // Outbox
    }

    #endregion // OnGenerate
}
