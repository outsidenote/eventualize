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
    internal const string EVENT_TARGET = "EvDbViewTypeAttribute";
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
                Location.Create(syntax.SyntaxTree, syntax.Span));
            builder.AppendLine($"`view {typeSymbol.Name}` MUST BE A partial interface!");
            context.AddSource(typeSymbol.GenFileName("partial-needed"), builder.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        #endregion // Exception Handling

        #region string rootName = .., stateType = .., eventType = ..

        string type = typeSymbol.ToType(syntax, cancellationToken);
        string viewClassName = $"EvDb{typeSymbol.Name}";
        if (!viewClassName.EndsWith("View"))
            viewClassName = $"{viewClassName}View";

        AssemblyName asm = GetType().Assembly.GetName();

        AttributeData att = typeSymbol.GetAttributes()
                                  .First(att => att.AttributeClass?.Name == EventTargetAttribute);
        ImmutableArray<ITypeSymbol> args = att.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        string stateType = args[0].ToDisplayString();
        ITypeSymbol eventTypeSymbol = args[1];
        string eventType = eventTypeSymbol.ToDisplayString();

        #endregion // string baseName = .., stateType = .., eventType = ..

        TypedConstant nameConst = att.ConstructorArguments.First();
        string? name = nameConst.Value?.ToString();

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
                                var payload = c.GetData<{{p.Type}}>(_options);
                                State = Fold(State, payload, e);
                                break;
                            }
                        
                """);

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")] 
                    [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] 
                    public abstract class {{viewClassName}}Base: 
                        EvDbView<{{stateType}}>
                    {                                
                        public const string ViewName = "{{name}}";
                    
                        #region Ctor

                        protected {{viewClassName}}Base(
                            EvDbStreamAddress address,
                            IEvDbStorageAdapter storageAdapter, 
                            TimeProvider timeProvider,
                            ILogger logger,
                            JsonSerializerOptions? options):
                                base(new EvDbViewAddress(address, ViewName), 
                                EvDbStoredSnapshot.Empty,
                                storageAdapter, 
                                timeProvider,
                                logger,
                                options)
                        {
                        }

                        protected {{viewClassName}}Base(
                            EvDbStreamAddress address,
                            IEvDbStorageAdapter storageAdapter,
                            TimeProvider timeProvider,
                            ILogger logger,
                            EvDbStoredSnapshot snapshot, 
                            JsonSerializerOptions? options):
                                base(
                                    new EvDbViewAddress(address, ViewName), 
                                    snapshot,
                                    storageAdapter,
                                    timeProvider,
                                    logger,
                                    options)
                        {
                        }

                        #endregion // Ctor

                        #region OnFoldEvent

                        protected sealed override void OnFoldEvent(EvDbEvent e)
                        {
                            IEvDbEventConverter c = e;
                            switch (e.EventType)
                            {
                            {{string.Join("", foldMap)}}
                                default:
                                    throw new NotSupportedException(e.EventType);
                            }
                        }

                        #endregion // OnFoldEvent

                        #region Fold

                    {{string.Join("", foldAbstracts)}}
                        #endregion // Fold
                    }
                    """);
        context.AddSource(typeSymbol.GenFileName("view", "Base", "EvDb"), builder.ToString());

        #endregion // ViewBase

        builder.Clear();

        #region View

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{typeSymbol.Name}}: {{viewClassName}}Base
                    { 
                        internal {{typeSymbol.Name}}(
                            EvDbStreamAddress address,
                            IEvDbStorageAdapter storageAdapter, 
                            TimeProvider timeProvider,
                            ILogger logger,
                            JsonSerializerOptions? options):
                                    base (
                                        address, 
                                        storageAdapter, 
                                        timeProvider, 
                                        logger,
                                        options)
                        {
                        }

                        internal {{typeSymbol.Name}}(
                            EvDbStreamAddress address,
                            IEvDbStorageAdapter storageAdapter,
                            TimeProvider timeProvider,
                            ILogger logger,
                            EvDbStoredSnapshot snapshot, 
                            JsonSerializerOptions? options):
                                base (
                                    address,
                                    storageAdapter,
                                    timeProvider,
                                    logger,
                                    snapshot,
                                    options)
                        {
                        }
                    }
                    """);
        context.AddSource(typeSymbol.GenFileName("view"), builder.ToString());

        #endregion // View

        builder.Clear();

        #region View Factory

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine("using Microsoft.Extensions.Logging;");
        builder.AppendLine();

        builder.AppendLine($$"""
                    internal class {{typeSymbol.Name}}Factory: IEvDbViewFactory
                    {
                          private readonly IEvDbStorageAdapter _storageAdapter;
                          private readonly TimeProvider _timeProvider;
                          private readonly ILogger _logger;

                          public {{typeSymbol.Name}}Factory(
                                        IEvDbStorageAdapter storageAdapter, 
                                        TimeProvider timeProvider,
                                        ILogger logger)
                          {
                            _storageAdapter = storageAdapter;
                            _timeProvider = timeProvider;
                            _logger = logger;
                          } 

                        string IEvDbViewFactory.ViewName { get; } = "{{name}}";

                        IEvDbViewStore IEvDbViewFactory.CreateEmpty(EvDbStreamAddress address, JsonSerializerOptions? options) => 
                                new {{typeSymbol.Name}}(address, _storageAdapter, _timeProvider, _logger, options);

                        IEvDbViewStore IEvDbViewFactory.CreateFromSnapshot(EvDbStreamAddress address,
                            EvDbStoredSnapshot snapshot,
                            JsonSerializerOptions? options) => 
                                new {{typeSymbol.Name}}(address, _storageAdapter, _timeProvider, _logger, snapshot, options);
                    }
                    """);
        context.AddSource(typeSymbol.GenFileName("view", "Factory"), builder.ToString());

        #endregion // View Factory
    }

    #endregion // OnGenerate
}