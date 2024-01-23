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
public partial class ViewGenerator : BaseGenerator
{
    internal const string EVENT_TARGET = "EvDbViewAttribute";
    protected override string EventTargetAttribute { get; } = EVENT_TARGET;

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
                Location.None);
            builder.AppendLine($"`interface {typeSymbol.Name}` MUST BE A partial interface!");
            context.AddSource($"{typeSymbol.Name}.generated.cs", builder.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        #endregion // Exception Handling

        #region string rootName = .., aggregateInterfaceType = .., stateType = .., eventType = ..

        string type = typeSymbol.ToType(syntax, cancellationToken);
        string viewName = typeSymbol.Name;
        string rootName = viewName;
        if (viewName.EndsWith("View"))
            rootName = viewName.Substring(0, viewName.Length - 4);
        else
            viewName = $"{viewName}View";

        AssemblyName asm = GetType().Assembly.GetName();

        string aggregateInterfaceType = $"I{rootName}";

        AttributeData att = typeSymbol.GetAttributes()
                                  .First(att => att.AttributeClass?.Name == EventTargetAttribute);
        ImmutableArray<ITypeSymbol> args = att.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        string stateType = args[0].ToDisplayString();
        ITypeSymbol eventTypeSymbol = args[1];
        string eventType = eventTypeSymbol.ToDisplayString();

        #endregion // string rootName = .., aggregateInterfaceType = .., stateType = .., eventType = ..

        KeyValuePair<string, TypedConstant> customPropertyName = att.NamedArguments.FirstOrDefault(m => m.Key == "CollectionName");

        string propName = customPropertyName.Value.Value?.ToString() ?? rootName;

        #region var eventsPayloads = from a in eventTypeSymbol.GetAttributes() ...

        var eventsPayloads = from a in eventTypeSymbol.GetAttributes()
                             let cls = (INamedTypeSymbol)(a.AttributeClass!)
                             where cls != null
                             let text = cls.Name
                             where text == EventAdderGenerator.EventTarget
                             let payloadType = cls.TypeArguments.First()
                             let payloadAtt = payloadType.GetAttributes().First(m => m.AttributeClass?.Name.StartsWith("EvDbEventPayload") ?? false)
                             let eventTypeValue = payloadAtt.ConstructorArguments.First().Value?.ToString()
                             let fullName = cls?.ToString()
                             let genStart = fullName.IndexOf('<') + 1
                             let genLen = fullName.Length - genStart - 1
                             let generic = fullName.Substring(genStart, genLen)
                             let attName = a.ConstructorArguments.FirstOrDefault().Value
                             select (Type: generic, Key: eventTypeValue);
        eventsPayloads = eventsPayloads.ToArray(); // run once

        #endregion // var eventsPayloads = from a in eventTypeSymbol.GetAttributes() ...

        #region ViewBase

        #region var eventsPayloads = ...

        #endregion // var eventsPayloads = ...

        var foldAbstracts = eventsPayloads.Select(p =>
                $"""
                    protected virtual {stateType} Fold(
                            {stateType} state,
                            {p.Type} payload,
                            IEvDbEventMeta meta) => state;

                """);

        var foldMap = eventsPayloads.Select(p =>
                $$"""
                    case "{{p.Key}}":
                            {
                                var payload = e.GetData<{{p.Type}}>(_jsonSerializerOptions);
                                _state = Fold(_state, payload, e);
                                break;
                            }
                        
                """);

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]                    public abstract class {{viewName}}Base:
                        IEvDbView<{{stateType}}>
                    {        
                        protected abstract {{stateType}} DefaultState { get; }
                        private readonly JsonSerializerOptions? _jsonSerializerOptions;
                        private {{stateType}} _state;
                                         
                        protected {{viewName}}Base(
                            JsonSerializerOptions? jsonSerializerOptions)
                        {
                            _state = DefaultState;
                            _jsonSerializerOptions = jsonSerializerOptions;
                        }

                        public virtual int MinEventsBetweenSnapshots => 0;

                        {{stateType}} IEvDbView<{{stateType}}>.State => _state;

                        #region FoldEvent

                        void IEvDbView.FoldEvent(IEvDbEvent e)
                        {
                            switch (e.EventType)
                            {
                            {{string.Join("", foldMap)}}
                                default:
                                    throw new NotSupportedException(e.EventType);
                            }
                        }

                        #endregion // FoldEvent

                        #region Fold

                    {{string.Join("", foldAbstracts)}}
                        #endregion // Fold
                    }
                    """);
        context.AddSource($"{viewName}Base.generated.cs", builder.ToString());

        #endregion // ViewBase

        builder.Clear();

        #region View

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{typeSymbol.Name}}: {{viewName}}Base
                    { 
                        public static IEvDbView Create(JsonSerializerOptions? jsonSerializerOptions) => new {{typeSymbol.Name}}(jsonSerializerOptions);

                        private {{typeSymbol.Name}}(
                            JsonSerializerOptions? jsonSerializerOptions):base (jsonSerializerOptions)
                        {
                        }
                    }
                    """);
        context.AddSource($"{typeSymbol.Name}.generated.cs", builder.ToString());

        #endregion // Vew
    }

    #endregion // OnGenerate
}