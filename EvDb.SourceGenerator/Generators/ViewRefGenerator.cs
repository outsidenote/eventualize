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
    private const string _eventTarget = "EvDbAttachView";
    protected override string EventTargetAttribute { get; } = $"{_eventTarget}Attribute";

    #region OnGenerate

    protected override void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            TypeDeclarationSyntax syntax,
            CancellationToken cancellationToken)
    {
        context.ThrowIfNotPartial(typeSymbol, syntax);

        StringBuilder builder = new StringBuilder();

        AssemblyName asm = GetType().Assembly.GetName();

        string type = typeSymbol.ToType(syntax, cancellationToken);

        AttributeData attOfFactory = typeSymbol.GetAttributes()
                          .First(att => att.AttributeClass?.Name == FactoryGenerator.EVENT_TARGET_ATTRIBUTE);

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
                                      bool match = _eventTarget == name || EventTargetAttribute == name;
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

        builder.AppendHeader(syntax, typeSymbol);
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

        string factoryName = $"EvDb{typeSymbol.Name}";
        string rootName = factoryName;
        if (factoryName.EndsWith("Factory"))
            rootName = factoryName.Substring(0, factoryName.Length - 7);
        if (rootName == typeSymbol.Name)
            rootName = $"{rootName}_";
        string interfaceType = $"I{rootName}";

        var propsColInterface = propsNames.Select((p, i) =>
                                        $$"""
                                                    public {{p.StateType}} {{p.Name}} => _view{{p.Name}}.State;

                                                """);


        builder.Clear();

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

        builder.AppendHeader(syntax, typeSymbol);
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

        builder.Clear();

        #region Stream Interface

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial interface {{interfaceType}}
                    { 
                        {{rootName}}Views Views { get; }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(interfaceType), builder.ToString());

        #endregion // Stream Interface

        #region Views Factory

        foreach (var viewRef in propsNames)
        {
            builder.Clear();
            builder.AppendHeader(syntax, typeSymbol, viewRef.Namespace);
            builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            builder.AppendLine("using Microsoft.Extensions.Logging;");
            builder.AppendLine();

            builder.AppendLine($$"""
                    internal partial class {{viewRef.TypeName}}Factory: IEvDbViewFactory
                    { 
                        private readonly IEvDbStorageSnapshotAdapter _storageAdapter;                        
                        private readonly ILogger _logger;
                    
                        public {{viewRef.TypeName}}Factory(
                                        [FromKeyedServices("{{domain}}:{{partition}}:{{viewRef.Name}}")]IEvDbStorageSnapshotAdapter storageAdapter,  
                                        ILogger<{{viewRef.TypeName}}> logger)
                        {
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

        #region Views Factory DI

        var dis = propsNames.Select(viewRef =>
                        $$"""
                            services.AddKeyedScoped<IEvDbViewFactory, {{viewRef.Type}}Factory>("{{domain}}:{{partition}}:{{viewRef.Name}}");

                        """);
            builder.Clear();
            builder.AppendHeader(syntax, typeSymbol, "Microsoft.Extensions.DependencyInjection");
            builder.AppendLine($"using {typeSymbol.ContainingNamespace.ToDisplayString()};");
            builder.AppendLine();

            builder.AppendLine($$"""
                    public static class {{factoryName}}ViewsRegistration
                    { 
                        public static IServiceCollection Add{{rootName}}ViewsFactories(
                                                                this IServiceCollection services)
                        {
                        {{string.Join("\t", dis)}}
                            return services;
                        }
                    }
                    """);
            context.AddSource(typeSymbol.StandardPath("DI", $"{factoryName}ViewsRegistration"), builder.ToString());

        #endregion // Views Factory DI
    }

    #endregion // OnGenerate
}