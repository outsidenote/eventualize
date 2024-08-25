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
public partial class ViewRefGenerator : BaseGenerator
{
    private const string EVENT_TARGET = "EvDbAttachView";
    protected override string EventTargetAttribute { get; } = $"{EVENT_TARGET}Attribute";

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
        AssemblyName asm = GetType().Assembly.GetName();

        StringBuilder builder = new StringBuilder();

        string type = typeSymbol.ToType(syntax, cancellationToken);

        AttributeData attOfFactory = typeSymbol.GetAttributes()
                          .First(att => att.AttributeClass?.Name == FactoryGenerator.EVENT_TARGET_ATTRIBUTE);

        string factoryName = $"EvDb{typeSymbol.Name}";
        string rootName = factoryName;
        if (factoryName.EndsWith("Factory"))
            rootName = factoryName.Substring(0, factoryName.Length - 7);
        if (rootName == typeSymbol.Name)
            rootName = $"{rootName}_";
        string interfaceType = $"I{rootName}";
        string factoryInterfaceType = $"{interfaceType}Factory";

        ImmutableArray<ITypeSymbol> args = attOfFactory.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol eventTypeSymbol = args[0];
        string supportedEventsTypeName = eventTypeSymbol.ToDisplayString();

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

        #region propsNames = .., props = .., propsCreates = ..

        var propsNames = typeSymbol.GetAttributes()
                                  .Where(att =>
                                  {
                                      string? name = att.AttributeClass?.Name;
                                      bool match = EVENT_TARGET == name || EventTargetAttribute == name;
                                      return match;
                                  })
                                  .Select(att =>
                                  {
                                      ImmutableArray<ITypeSymbol> args = att.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
                                      ITypeSymbol viewTypeSymbol = args[0];
                                      string viewTypeName = viewTypeSymbol.ToDisplayString();
                                      string typeName = viewTypeSymbol.Name;

                                      TypedConstant propName = att.ConstructorArguments.FirstOrDefault();
                                      string? pName = propName.Value?.ToString();
                                      if (pName == null)
                                      {
                                          pName = viewTypeSymbol.Name;
                                          if (pName.EndsWith("View") && pName.Length != 4)
                                              pName = pName.Substring(0, pName.Length - 4);
                                      }

                                      var viewAtt = viewTypeSymbol.GetAttributes().First(vatt => vatt.AttributeClass?.Name == ViewGenerator.EVENT_TARGET);
                                      ImmutableArray<ITypeSymbol> argsState = viewAtt.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
                                      string stateType = argsState[0].ToDisplayString();
                                      string ns = viewTypeSymbol.ContainingNamespace.ToDisplayString();

                                      if (!viewAtt.TryGetValue("name", out string storageName))
                                      {
                                          var diagnostic = syntax.CreateDiagnostic(14, "view-name on store is missng", typeSymbol.Name);
                                          context.ReportDiagnostic(diagnostic);
                                      }

                                      return (Name: pName, Type: viewTypeName, TypeName: typeName, StateType: stateType, Namespace: ns, StorageName: storageName);
                                  }).ToArray();
        #endregion // propsNames = .., props = .., viewFactoriesYield = ..

        var propsColInterface = propsNames.Select((p, i) =>
                                        $$"""
                                                    public {{p.StateType}} {{p.Name}} => _view{{p.Name}}.State;

                                                """);

        #region Factory Interface

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public interface {{factoryInterfaceType}}: IEvDbStreamFactory<{{interfaceType}}>
                    { 
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(factoryInterfaceType), builder.ToString());

        #endregion // Factory Interface

        #region FactoryBase

        #region var eventsPayloads = ...

        #endregion // var eventsPayloads = ...


        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] 
                    public abstract class {{factoryName}}Base:
                        EvDbStreamFactoryBase<{{interfaceType}}>
                    {                
                        #region Ctor
                            
                        public {{factoryName}}Base(
                                    IEvDbStorageStreamAdapter storageAdapter, 
                                    TimeProvider timeProvider):
                                        base(storageAdapter, timeProvider)
                        {
                        }

                        #endregion // Ctor

                        #region OnCreate

                        protected override {{rootName}} OnCreate(
                                string streamId,
                                IImmutableList<IEvDbViewStore> views,
                                long lastStoredEventOffset)
                        {
                            {{rootName}} stream =
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
        context.AddSource(typeSymbol.StandardPath($"{factoryName}Base"), builder.ToString());

        #endregion // FactoryBase

        #region Stream Factory

        var viewFactoriesCtorInjection =
            propsNames.Select((viewRef,i) =>
            $$"""
                        [FromKeyedServices("{{domain}}:{{partition}}:{{viewRef.Name}}")]IEvDbViewFactory factoryOf{{viewRef.TypeName}}{{i}},

            """);

        var viewFactoriesYield = propsNames.Select((viewRef,i) =>
            $$"""

                        factoryOf{{viewRef.TypeName}}{{i}}
            """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{typeSymbol.Name}}
                    {
                        #region Ctor
                        
                        public {{typeSymbol.Name}}(
                                [FromKeyedServices("{{domain}}:{{partition}}")]IEvDbStorageStreamAdapter storageAdapter,                    
                    {{string.Join("", viewFactoriesCtorInjection)}}       
                                ILogger<{{typeSymbol.Name}}> logger,
                                TimeProvider? timeProvider = null) : base(storageAdapter, timeProvider ?? TimeProvider.System)
                        {
                            ViewFactories = new IEvDbViewFactory[] 
                            {{{string.Join(",", viewFactoriesYield)}}       
                            };
                        }
                        
                        #endregion // Ctor
                                                             
                        protected override IEvDbViewFactory[] ViewFactories { get; }
                    }
                    """);
        context.AddSource(typeSymbol.StandardDefaultAndPath("partial-view-ref"), builder.ToString());

        #endregion // Stream Factory

        #region Views Encapsulation

        var viewsCtor2Props = propsNames.Select((p, i) =>
                                        $$"""
                                                _view{{p.Name}} = ((IEvDbViewStore<{{p.StateType}}>)_views[{{i}}]);
                                        
                                        """);
        var viewsFields = propsNames.Select((p, i) =>
                                        $$"""
                                            private readonly IEvDbViewStore<{{p.StateType}}> _view{{p.Name}};
                                        
                                        """);
        var viewsYield = propsNames.Select((p, i) =>
                                        $$"""
                                                yield return _view{{p.Name}};
                                        
                                        """);

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    public class {{rootName}}Views
                    { 
                        private readonly IImmutableList<IEvDbViewStore> _views;
                    {{string.Join("", viewsFields)}}
                        public {{rootName}}Views(IImmutableList<IEvDbViewStore> views)
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
        context.AddSource(typeSymbol.StandardPath($"{rootName}Views"), builder.ToString());

        #endregion // Views Encapsulation

        #region Stream Interface

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public partial interface {{interfaceType}}: IEvDbStreamStore, {{supportedEventsTypeName}}
                    { 
                        {{rootName}}Views Views { get; }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(interfaceType), builder.ToString());

        #endregion // Stream Interface

        #region EvDb{typeSymbol.Name}SnapshotEntry 

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    public readonly record struct EvDb{{typeSymbol.Name}}Entry (IServiceCollection Services)
                    {
                        internal EvDb{{typeSymbol.Name}}Entry(
                                EvDbRegistrationEntry parent)
                            : this(parent.Services)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath($"EvDb{typeSymbol.Name}Entry"),
                    builder.ToString());

        #endregion // EvDb{typeSymbol.Name}SnapshotEntry 

        #region IEvDb{}ViewRegistrationEntry Interface

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    public interface IEvDb{{typeSymbol.Name}}SnapshotEntry
                    {
                        EvDbPartitionAddress Address { get; }
                        EvDbStorageContext? Context { get; }
                        IServiceCollection Services { get; }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath($"IEvDb{typeSymbol.Name}ViewRegistrationEntry"),
                    builder.ToString());

        #endregion // IEvDb{}ViewRegistrationEntry Interface

        #region EvDb{typeSymbol.Name}SnapshotEntry

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    public readonly record struct EvDb{{typeSymbol.Name}}SnapshotEntry(
                        EvDbStorageContext? Context,
                        EvDbPartitionAddress Address,
                        IServiceCollection Services) : IEvDb{{typeSymbol.Name}}SnapshotEntry
                    {
                        public EvDb{{typeSymbol.Name}}SnapshotEntry(EvDbStreamStoreRegistrationContext context)
                            : this(context.Context, context.Address, context.Services)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath($"EvDb{typeSymbol.Name}SnapshotEntry"),
                    builder.ToString());

        #endregion // EvDb{typeSymbol.Name}SnapshotEntry 

        #region Views Factory

        foreach (var viewRef in propsNames)
        {
            builder.ClearAndAppendHeader(syntax, typeSymbol, viewRef.Namespace);
            builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            builder.AppendLine("using Microsoft.Extensions.Logging;");
            builder.AppendLine();

            builder.AppendLine($$"""
                    internal partial class {{viewRef.TypeName}}Factory: IEvDbViewFactory
                    { 
                        private readonly IEvDbStorageSnapshotAdapter _storageAdapter;                        
                        private readonly ILogger _logger;
                             
                        
                        /// <summary>
                        /// Using `IServiceProvider` in order to have either a specific store provider or the default one.
                        /// </summary>
                        public {{viewRef.TypeName}}Factory(
                                        IServiceProvider serviceProvider,
                                        ILogger<{{viewRef.TypeName}}> logger)
                        {
                            IEvDbStorageSnapshotAdapter storageAdapter = 
                                serviceProvider.GetKeyedService<IEvDbStorageSnapshotAdapter>("{{domain}}:{{partition}}:{{viewRef.Name}}") ??
                                serviceProvider.GetRequiredKeyedService<IEvDbStorageSnapshotAdapter>("{{domain}}:{{partition}}");

                            _storageAdapter = storageAdapter;
                            _logger = logger;
                        } 

                        string IEvDbViewFactory.ViewName { get; } = "{{viewRef.StorageName}}";
                    
                        IEvDbViewStore IEvDbViewFactory.CreateEmpty(
                                                EvDbStreamAddress address, 
                                                JsonSerializerOptions? options, 
                                                TimeProvider? timeProvider) 
                        {
                            return new {{viewRef.TypeName}}(
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
                            return new {{viewRef.TypeName}}(
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
            context.AddSource(typeSymbol.StandardNsAndPath(folder, $"{viewRef.TypeName}Factory"), builder.ToString());
        }

        #endregion // Views Factory

        #region DI

        var addViewsFactories = propsNames.Select(viewRef =>
                        $$"""
                            services.AddKeyedScoped<IEvDbViewFactory, {{viewRef.Type}}Factory>("{{domain}}:{{partition}}:{{viewRef.Name}}");

                        """);

        var forViews = propsNames.Select(viewRef =>
                        $$"""
                                /// <summary>
                                /// Register a snapshot store provider for `{{viewRef.Name}}` view
                                /// </summary>
                                /// <param name="entry"></param>
                                /// <param name="registrationAction">The registration action.</param>
                                /// <returns></returns>
                                public static IEvDb{{typeSymbol.Name}}SnapshotEntry For{{viewRef.Name}}(
                                    this EvDb{{typeSymbol.Name}}SnapshotEntry entry,
                                    Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
                                {
                                    IEvDb{{typeSymbol.Name}}SnapshotEntry e = entry;
                                    return e.For{{viewRef.Name}}(registrationAction);
                                }


                                /// <summary>
                                /// Register a snapshot store provider for `{{viewRef.Name}}` view
                                /// </summary>
                                /// <param name="entry"></param>
                                /// <param name="registrationAction">The registration action.</param>
                                /// <returns></returns>
                                public static IEvDb{{typeSymbol.Name}}SnapshotEntry For{{viewRef.Name}}(
                                    this IEvDb{{typeSymbol.Name}}SnapshotEntry entry,
                                    Action<EvDbSnapshotStoreRegistrationContext> registrationAction)
                                {
                                    var viewAdress = new EvDbViewBasicAddress(entry.Address, "{{viewRef.Name}}");
                                    var ctx = new EvDbSnapshotStoreRegistrationContext(
                                        entry.Context,
                                        viewAdress,
                                        entry.Services);

                                    registrationAction(ctx);

                                    return entry;
                                }
                        """);

        var addViewFactories = propsNames.Select(viewRef =>
                                            $$"""
                                                    services.AddKeyedScoped<IEvDbViewFactory,{{viewRef.Type}}Factory>("{{domain}}:{{partition}}:{{viewRef.Name}}");

                                            """);

        builder.Clear();

        builder.ClearAndAppendHeader(syntax, typeSymbol, "Microsoft.Extensions.DependencyInjection");
        builder.AppendLine($"using {typeSymbol.ContainingNamespace.ToDisplayString()};");
        // builder.AppendLine($"using {typeSymbol.ContainingNamespace.ToDisplayString()}.Generated;");
        builder.AppendLine("using EvDb.Core.Internals;");
        builder.AppendLine("using EvDb.Core.Store.Internals;");
        builder.AppendLine();
        builder.AppendLine($$"""
                    public static class {{factoryName}}Registration 
                    { 
                        public static EvDb{{typeSymbol.Name}}SnapshotEntry Add{{typeSymbol.Name}}(
                            this EvDbRegistrationEntry instance,
                            Action<EvDbStreamStoreRegistrationContext> registrationAction,
                            EvDbStorageContext? context = null)
                        { 
                            IServiceCollection services = instance.Services;
                            services.Add{{rootName}}ViewsFactories();

                    {{string.Join("", addViewFactories)}}

                            services.AddScoped<I{{factoryName}},{{typeSymbol.Name}}>();     
                        
                            var storageContext = new EvDbStreamStoreRegistrationContext(context,
                                new EvDbPartitionAddress("{{domain}}","{{partition}}"),
                                services);

                            registrationAction(storageContext);
                            var viewContext = new EvDb{{typeSymbol.Name}}SnapshotEntry(
                                                    storageContext.Context, 
                                                    storageContext.Address,
                                                    services);
                            return viewContext;
                        }

                        internal static IServiceCollection Add{{rootName}}ViewsFactories(
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
                        public static IEvDb{{typeSymbol.Name}}SnapshotEntry DefaultSnapshotConfiguration(
                            this EvDb{{typeSymbol.Name}}SnapshotEntry entry,
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


        context.AddSource(typeSymbol.StandardPath("DI", $"{factoryName}Registration"), builder.ToString());

        #endregion //  DI
    }

    #endregion // OnGenerate
}