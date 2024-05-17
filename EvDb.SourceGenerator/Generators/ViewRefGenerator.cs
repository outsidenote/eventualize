#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

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
        StringBuilder builder = new StringBuilder();

        #region Exception Handling

        if (!syntax.IsPartial())
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor("EvDb: 013", "class must be partial",
                $"{typeSymbol.Name}, Must be partial", "EvDb",
                DiagnosticSeverity.Error, isEnabledByDefault: true),
                Location.Create(syntax.SyntaxTree, syntax.Span));
            builder.AppendLine($"`{typeSymbol.Name}` MUST BE A partial interface!");
            context.AddSource(typeSymbol.GenFileName("partial-is-missing"), builder.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        #endregion // Exception Handling

        AssemblyName asm = GetType().Assembly.GetName();

        string type = typeSymbol.ToType(syntax, cancellationToken);

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

                                      return (Name: pName, Type: viewTypeName, StateType: stateType);
                                  }).ToArray();
        var propsCreates = propsNames.Select(p =>
                                                $$"""

                                                            (IEvDbViewFactory)(new {{p.Type}}Factory(_storageAdapter))
                                                """);

        #endregion // propsNames = .., props = .., propsCreates = ..

        #region Stream Factory

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{typeSymbol.Name}}
                    {
                        #region Ctor
                        
                        public {{typeSymbol.Name}}(
                                IEvDbStorageAdapter storageAdapter,
                                TimeProvider? timeProvider = null) : base(storageAdapter, timeProvider)
                        {
                            ViewFactories = new []
                            {{{string.Join(",", propsCreates)}}
                            };
                        }
                        
                        #endregion // Ctor
                                                             
                        protected override IEvDbViewFactory[] ViewFactories { get; }
                    }
                    """);
        context.AddSource(typeSymbol.GenFileName( "view-ref"), builder.ToString());

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
                                                    public {{p.StateType}} {{p.Name}} => ((IEvDbViewStore<{{p.StateType}}>)_views[{{i}}]).State;

                                                """);


        builder.Clear();

        #region Views Encapsulation

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    public class {{rootName}}Views
                    { 
                        private readonly IImmutableList<IEvDbViewStore> _views;
                    
                        public {{rootName}}Views(IImmutableList<IEvDbViewStore> views)
                        {
                            _views = views;
                        }
                    {{string.Join("", propsColInterface)}}
                    
                        public IEnumerable<IEvDbView> ToMetadata()
                        {
                            foreach (var view in _views)
                            {
                                yield return view;
                            }
                        }
                    }
                    """);
        context.AddSource(typeSymbol.GenFileNameSuffix($"{rootName}Views", "view-ref"), builder.ToString());

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
        context.AddSource(typeSymbol.GenFileNameSuffix(interfaceType, "view-ref"), builder.ToString());

        #endregion // Stream Interface
    }

    #endregion // OnGenerate
}