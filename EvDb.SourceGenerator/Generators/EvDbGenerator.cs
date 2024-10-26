#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EvDbGenerator : BaseGenerator
{
    internal const string STREAM_FACTORY_ATT = "EvDbStreamFactoryAttribute";
    private const string ATTACHE_VIEW = "EvDbAttachView";
    private const string ATTACHE_VIEW_ATT = "EvDbAttachViewAttribute";
    protected override string EventTargetAttribute { get; } = STREAM_FACTORY_ATT;

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

        string type = typeSymbol.ToType(syntax, cancellationToken);

        AttributeData attOfFactory = typeSymbol.GetAttributes()
                          .First(att => att.AttributeClass?.Name == STREAM_FACTORY_ATT);

        string factoryOriginName = typeSymbol.Name;
        string factoryName = $"EvDb{factoryOriginName}";
        string streamName = factoryName;
        if (factoryName.EndsWith("Factory"))
            streamName = factoryName.Substring(0, factoryName.Length - 7);
        if (streamName == factoryOriginName)
            streamName = $"{streamName}1";
        string streamInterface = $"I{streamName}";
        string factoryInterface = $"{streamInterface}Factory";

        ImmutableArray<ITypeSymbol> args = attOfFactory.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol relatedEventsTypeSymbol = args[0];
        string relatedEventsTypesFullName = relatedEventsTypeSymbol.ToDisplayString();

        string? relatedTopicTypesFullName = null;
        if (args.Length == 2)
        {
            ITypeSymbol relatedTopicTypeSymbol = args[1];
            relatedTopicTypesFullName = relatedTopicTypeSymbol.ToDisplayString();
        }

        #region string domain = ..., string partition = ...

        if (!attOfFactory.TryGetValue("domain", out string domain))
        {
            var diagnostic = syntax.CreateDiagnostic(14, "domain is missng", typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        if (!attOfFactory.TryGetValue("partition", out string partition))
        {
            var diagnostic = syntax.CreateDiagnostic(14, "partition is missng", typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        #endregion //  string domain = ..., string partition = ...

        #region ViewInfo[] viewsInfo = ..

        ViewInfo[] viewsInfo = typeSymbol.GetAttributes()
                                  .Where(att =>
                                  {
                                      string? name = att.AttributeClass?.Name;
                                      bool match = ATTACHE_VIEW_ATT == name || ATTACHE_VIEW == name;
                                      return match;
                                  })
                                  .Select(a => new ViewInfo(context, syntax, a)).ToArray();

        #endregion // ViewInfo[] viewsInfo = ..

        var symbolEqualityComparer = SymbolEqualityComparer.Default;

        #region Validation

        foreach (var v in viewsInfo)
        {
            if (!symbolEqualityComparer.Equals(v.RelatedEventsSymbol, relatedEventsTypeSymbol))
                context.Throw(
                    EvDbErrorsNumbers.EventsNotMatch,
                    $"[{v.ViewTypeName}] events [{v.RelatedEventsTypeFullName}] not match the [{factoryOriginName}] events [{relatedEventsTypesFullName}]",
                    syntax);
        }

        #endregion //  Validation

        #region Factory Interface

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    public interface {{factoryInterface}}: IEvDbStreamFactory<{{streamInterface}}>
                    { 
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("Factories", "Interfaces", factoryInterface), builder.ToString());

        #endregion // Factory Interface

        #region Stream Factory

        var viewFactoriesCtorInjection =
            viewsInfo.Select((viewRef, i) =>
            $$"""
                        [FromKeyedServices("{{domain}}:{{partition}}:{{viewRef.ViewPropName}}")]IEvDbViewFactory factoryOf{{viewRef.ViewTypeName}}{{i}},

            """);

        var viewFactoriesYield = viewsInfo.Select((viewRef, i) =>
            $$"""

                        factoryOf{{viewRef.ViewTypeName}}{{i}}
            """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    partial {{type}} {{factoryOriginName}}: EvDbStreamFactoryBase<{{streamInterface}}>,
                            {{factoryInterface}}
                    {
                        #region Ctor
                        
                        public {{factoryOriginName}}(
                                [FromKeyedServices("{{domain}}:{{partition}}")]IEvDbStorageStreamAdapter storageAdapter,                    
                    {{string.Join("", viewFactoriesCtorInjection)}}       
                                ILogger<{{factoryOriginName}}> logger,
                                TimeProvider? timeProvider = null) : base(storageAdapter, timeProvider ?? TimeProvider.System)
                        {
                            ViewFactories = new IEvDbViewFactory[] 
                            {{{string.Join(",", viewFactoriesYield)}}       
                            };
                        }
                        
                        #endregion // Ctor
                                                             
                        protected override IEvDbViewFactory[] ViewFactories { get; }
                        #region Partition
                    
                        public override EvDbPartitionAddress PartitionAddress { get; } = 
                            new EvDbPartitionAddress("{{domain}}", "{{partition}}");
                    
                        #endregion // PartitionAddress
                    
                        #region OnCreate
                    
                        protected override {{streamName}} OnCreate(
                                string streamId,
                                IImmutableList<IEvDbViewStore> views,
                                long lastStoredEventOffset)
                        {
                            {{streamName}} stream =
                                new(
                                    this,
                                    views,
                                    _storageAdapter,
                                    streamId,
                                    lastStoredEventOffset);
                    
                            return stream;
                        }
                    
                        #endregion // OnCreate                    
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath("Factories"), builder.ToString());

        #endregion // Stream Factory

        #region Views Encapsulation

        var propsColInterface = viewsInfo.Select((p, i) =>
                                        $$"""
                                                    public {{p.ViewStateTypeFullName}} {{p.ViewPropName}} => _view{{p.ViewPropName}}.State;

                                                """);

        var viewsCtor2Props = viewsInfo.Select((p, i) =>
                                        $$"""
                                                _view{{p.ViewPropName}} = ((IEvDbViewStore<{{p.ViewStateTypeFullName}}>)_views[{{i}}]);
                                        
                                        """);
        var viewsFields = viewsInfo.Select((p, i) =>
                                        $$"""
                                            private readonly IEvDbViewStore<{{p.ViewStateTypeFullName}}> _view{{p.ViewPropName}};
                                        
                                        """);
        var viewsYield = viewsInfo.Select((p, i) =>
                                        $$"""
                                                yield return _view{{p.ViewPropName}};
                                        
                                        """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public class {{streamName}}Views
                    { 
                        private readonly IImmutableList<IEvDbViewStore> _views;
                    {{string.Join("", viewsFields)}}
                        public {{streamName}}Views(IImmutableList<IEvDbViewStore> views)
                        {
                            _views = views;
                    {{string.Join("", viewsCtor2Props)}}
                        }
                    {{string.Join("", propsColInterface)}}                    
                        public IEnumerable<IEvDbView> ToMetadata()
                        {
                    {{string.Join("", viewsYield)}}
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("Views", $"{streamName}Views"), builder.ToString());

        #endregion // Views Encapsulation

        #region Stream Interface

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    public partial interface {{streamInterface}}: IEvDbStreamStore, {{relatedEventsTypesFullName}}
                    { 
                        {{streamName}}Views Views { get; }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("Streams", "Interfaces", streamInterface), builder.ToString());

        #endregion // Stream Interface

        IEnumerable<PayloadInfo> eventsPayloads = relatedEventsTypeSymbol.GetPayloads();

        #region Stream

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        string setTopic = relatedTopicTypesFullName != null
                        ? $"TopicProducer = new {relatedTopicTypesFullName}(this);"
                        : string.Empty;

        var adds = eventsPayloads.Select(ep =>
                    $$"""
                        async ValueTask<IEvDbEventMeta> {{relatedEventsTypesFullName}}.AddAsync(
                                {{ep.FullTypeName}} payload, 
                                string? capturedBy)
                        {
                            IEvDbEventMeta meta = await AddEventAsync(payload, capturedBy);
                            return meta;
                        }

                    """);
        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public partial class {{streamName}}: 
                            EvDbStream,
                            {{relatedEventsTypesFullName}},
                            {{streamInterface}}
                    { 
                        #region Ctor

                        public {{streamName}}(
                            IEvDbStreamConfig stramConfiguration,
                            IImmutableList<IEvDbViewStore> views,
                            IEvDbStorageStreamAdapter storageAdapter,
                            string streamId,
                            long lastStoredOffset) : 
                                base(stramConfiguration, views, storageAdapter, streamId, lastStoredOffset)
                        {
                            Views = new {{streamName}}Views(views);
                            {{setTopic}}
                        }

                        #endregion // Ctor

                        protected override IEvDbTopicProducer? TopicProducer { get; } 
                    
                        public {{streamName}}Views Views { get; }
                    
                        #region Add

                    {{string.Join("", adds)}}
                        #endregion // Add
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("Streams", streamName), builder.ToString());

        #endregion // Stream

        #region EvDb{factoryOriginName}SnapshotEntry 

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public readonly record struct EvDb{{factoryOriginName}}Entry (IServiceCollection Services)
                    {
                        internal EvDb{{factoryOriginName}}Entry(
                                EvDbRegistrationEntry parent)
                            : this(parent.Services)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("DI", $"EvDb{factoryOriginName}Entry"),
                    builder.ToString());

        #endregion // EvDb{factoryOriginName}SnapshotEntry 

        #region IEvDb{}ViewRegistrationEntry Interface

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    public interface IEvDb{{factoryOriginName}}SnapshotEntry
                    {
                        EvDbPartitionAddress Address { get; }
                        EvDbStorageContext? Context { get; }
                        IServiceCollection Services { get; }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("DI", $"IEvDb{factoryOriginName}SnapshotEntry"),
                    builder.ToString());

        #endregion // IEvDb{}ViewRegistrationEntry Interface

        #region EvDb{factoryOriginName}SnapshotEntry

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();

        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public readonly record struct EvDb{{factoryOriginName}}SnapshotEntry(
                        EvDbStorageContext? Context,
                        EvDbPartitionAddress Address,
                        IServiceCollection Services) : IEvDb{{factoryOriginName}}SnapshotEntry
                    {
                        public EvDb{{factoryOriginName}}SnapshotEntry(EvDbStreamStoreRegistrationContext context)
                            : this(context.Context, context.Address, context.Services)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("DI", $"EvDb{factoryOriginName}SnapshotEntry"),
                    builder.ToString());

        #endregion // EvDb{factoryOriginName}SnapshotEntry 

        #region Views Factory

        foreach (var viewRef in viewsInfo)
        {
            builder.ClearAndAppendHeader(syntax, typeSymbol, viewRef.Namespace);
            builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            builder.AppendLine("using Microsoft.Extensions.Logging;");
            builder.AppendLine();

            builder.DefaultsOnType(typeSymbol);
            builder.AppendLine($$"""
                    internal partial class {{viewRef.ViewTypeName}}Factory: IEvDbViewFactory
                    { 
                        private readonly IEvDbStorageSnapshotAdapter _storageAdapter;                        
                        private readonly ILogger _logger;
                             
                        
                        /// <summary>
                        /// Using `IServiceProvider` in order to have either a specific store provider or the default one.
                        /// </summary>
                        public {{viewRef.ViewTypeName}}Factory(
                                        IServiceProvider serviceProvider,
                                        ILogger<{{viewRef.ViewTypeName}}> logger)
                        {
                            IEvDbStorageSnapshotAdapter storageAdapter = 
                                serviceProvider.GetKeyedService<IEvDbStorageSnapshotAdapter>("{{domain}}:{{partition}}:{{viewRef.ViewPropName}}") ??
                                serviceProvider.GetRequiredKeyedService<IEvDbStorageSnapshotAdapter>("{{domain}}:{{partition}}");

                            _storageAdapter = storageAdapter;
                            _logger = logger;
                        } 

                        string IEvDbViewFactory.ViewName { get; } = "{{viewRef.SnapshotStorageName}}";
                    
                        IEvDbViewStore IEvDbViewFactory.CreateEmpty(
                                                EvDbStreamAddress address, 
                                                JsonSerializerOptions? options, 
                                                TimeProvider? timeProvider) 
                        {
                            return new {{viewRef.ViewTypeName}}(
                                                    address, 
                                                    _storageAdapter, 
                                                    timeProvider ?? TimeProvider.System, 
                                                    _logger, 
                                                    options);
                        }
                    
                        IEvDbViewStore IEvDbViewFactory.CreateFromSnapshot(EvDbStreamAddress address,
                            EvDbStoredSnapshot snapshot,
                            JsonSerializerOptions? options, 
                                                TimeProvider? timeProvider)
                        {
                            return new {{viewRef.ViewTypeName}}(
                                                address,
                                                _storageAdapter, 
                                                timeProvider ?? TimeProvider.System, 
                                                _logger, 
                                                snapshot,
                                                options);
                        }
                    
                        IEvDbStorageSnapshotAdapter IEvDbViewFactory.StoreAdapter => _storageAdapter;                    
                    }
                    """);
            var folder = viewRef.Namespace;
            context.AddSource(typeSymbol.StandardPathWithNsIgnoreSymbolName(folder, "Views", "Factories", $"{viewRef.ViewTypeName}Factory"), builder.ToString());
        }

        #endregion // Views Factory

        #region DI

        var addViewsFactories = viewsInfo.Select(viewRef =>
                        $$"""
                            services.AddKeyedScoped<IEvDbViewFactory, {{viewRef.ViewTypeFullName}}Factory>("{{domain}}:{{partition}}:{{viewRef.ViewPropName}}");

                        """);

        var forViews = viewsInfo.Select(viewRef =>
                        $$"""
                                /// <summary>
                                /// Register a snapshot store provider for `{{viewRef.ViewPropName}}` view
                                /// </summary>
                                /// <param name="entry"></param>
                                /// <param name="registrationAction">The registration action.</param>
                                /// <returns></returns>
                                public static IEvDb{{factoryOriginName}}SnapshotEntry For{{viewRef.ViewPropName}}(
                                    this EvDb{{factoryOriginName}}SnapshotEntry entry,
                                    Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
                                {
                                    IEvDb{{factoryOriginName}}SnapshotEntry e = entry;
                                    return e.For{{viewRef.ViewPropName}}(registrationAction);
                                }


                                /// <summary>
                                /// Register a snapshot store provider for `{{viewRef.ViewPropName}}` view
                                /// </summary>
                                /// <param name="entry"></param>
                                /// <param name="registrationAction">The registration action.</param>
                                /// <returns></returns>
                                public static IEvDb{{factoryOriginName}}SnapshotEntry For{{viewRef.ViewPropName}}(
                                    this IEvDb{{factoryOriginName}}SnapshotEntry entry,
                                    Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
                                {
                                    var viewAdress = new EvDbViewBasicAddress(entry.Address, "{{viewRef.ViewPropName}}");
                                    var ctx = new EvDbSnapshotStoreRegistrationContext(
                                        entry.Context,
                                        viewAdress,
                                        entry.Services);

                                    registrationAction(ctx);

                                    return entry;
                                }
                        """);

        var addViewFactories = viewsInfo.Select(viewRef =>
                                            $$"""
                                                    services.AddKeyedScoped<IEvDbViewFactory,{{viewRef.ViewTypeFullName}}Factory>("{{domain}}:{{partition}}:{{viewRef.ViewPropName}}");

                                            """);

        builder.ClearAndAppendHeader(syntax, typeSymbol, "Microsoft.Extensions.DependencyInjection");
        builder.AppendLine($"using {typeSymbol.ContainingNamespace.ToDisplayString()};");
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine("using EvDb.Core.Store.Internals;");
        builder.AppendLine();
        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public static class {{factoryName}}Registration 
                    { 
                        public static EvDb{{factoryOriginName}}SnapshotEntry Add{{factoryOriginName}}(
                            this EvDbRegistrationEntry instance,
                            Action<EvDbStreamStoreRegistrationContext> registrationAction,
                            EvDbStorageContext? context = null)
                        { 
                            IServiceCollection services = instance.Services;
                            services.Add{{streamName}}ViewsFactories();

                    {{string.Join("", addViewFactories)}}

                            services.AddScoped<I{{factoryName}},{{factoryOriginName}}>();     
                        
                            var storageContext = new EvDbStreamStoreRegistrationContext(context,
                                new EvDbPartitionAddress("{{domain}}","{{partition}}"),
                                services);

                            registrationAction(storageContext);
                            var viewContext = new EvDb{{factoryOriginName}}SnapshotEntry(
                                                    storageContext.Context, 
                                                    storageContext.Address,
                                                    services);
                            return viewContext;
                        }

                        internal static IServiceCollection Add{{streamName}}ViewsFactories(
                                                                this IServiceCollection services)
                        {
                        {{string.Join("\t", addViewsFactories)}}
                            return services;
                        }
                                        

                        /// <summary>
                        /// Register the defaults snapshot store provider for the snapshot snapshot.
                        /// </summary>
                        /// <param name="registrationAction">The registration action.</param>
                        /// <returns></returns>
                        public static IEvDb{{factoryOriginName}}SnapshotEntry DefaultSnapshotConfiguration(
                            this EvDb{{factoryOriginName}}SnapshotEntry entry,
                            Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
                        {
                            var viewAdress = new EvDbViewBasicAddress(entry.Address, string.Empty);
                            var ctx = new EvDbSnapshotStoreRegistrationContext(
                                entry.Context,
                                viewAdress,
                                entry.Services);

                            registrationAction(ctx);

                            return entry;
                        }

                    {{string.Join("", forViews)}}
                    }
                    """);


        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName("DI", $"{factoryName}Registration"), builder.ToString());

        #endregion //  DI
    }
    #endregion // OnGenerate
}
