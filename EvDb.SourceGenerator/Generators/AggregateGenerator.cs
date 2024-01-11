#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using System.Collections.Immutable;
using System;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using EvDb.SourceGenerator.Helpers;
using System.Diagnostics;
using System.Reflection;

namespace EvDb.SourceGenerator;

[Generator]
public partial class AggregateGenerator : IIncrementalGenerator
{
    protected const string EventTargetAttribute = "EvDbAggregateFactoryAttribute";

    private static bool AttributePredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode.MatchAttribute(EventTargetAttribute, cancellationToken);
    }

    #region Initialize

    /// <summary>
    /// Called to initialize the generator and register generation steps via callbacks
    /// on the <paramref name="context" />
    /// </summary>
    /// <param name="context">The <see cref="T:Microsoft.CodeAnalysis.IncrementalGeneratorInitializationContext" /> to register callbacks on</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<SyntaxAndSymbol> classDeclarations =
                context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: AttributePredicate,
                        transform: static (ctx, _) => ToGenerationInput(ctx))
                    .Where(static m => m is not null);

        IncrementalValueProvider<(Compilation, ImmutableArray<SyntaxAndSymbol>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // register a code generator for the triggers
        context.RegisterSourceOutput(compilationAndClasses, Generate);

        static SyntaxAndSymbol ToGenerationInput(GeneratorSyntaxContext context)
        {
            var declarationSyntax = (TypeDeclarationSyntax)context.Node;

            var symbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax);
            if (symbol is not INamedTypeSymbol namedSymbol)
            {
                throw new NullReferenceException($"Code generated symbol of {nameof(declarationSyntax)} is missing");
            }
            return new SyntaxAndSymbol(declarationSyntax, namedSymbol);
        }

        void Generate(
                       SourceProductionContext spc,
                       (Compilation compilation,
                       ImmutableArray<SyntaxAndSymbol> items) source)
        {
            var (compilation, items) = source;
            foreach (SyntaxAndSymbol item in items)
            {
                OnGenerate(spc, compilation, item);
            }
        }
    }

    #endregion // Initialize

    #region OnGenerate

    private void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            SyntaxAndSymbol input)
    {
        INamedTypeSymbol typeSymbol = input.Symbol;
        TypeDeclarationSyntax syntax = input.Syntax;
        var cancellationToken = context.CancellationToken;
        if (cancellationToken.IsCancellationRequested)
            return;

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

        string type = typeSymbol.ToType(syntax, cancellationToken);
        string name = typeSymbol.Name;

        if (!name.EndsWith("Factory"))
        { 
            #region Exception Handling

                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor("EvDb: 014", "Factory muust have a suffix of `Factory`",
                    $"[{typeSymbol.Name}],must have a `Factory` suffix", "EvDb",
                    DiagnosticSeverity.Error, isEnabledByDefault: true),
                    Location.None);
                builder.AppendLine($"[{typeSymbol.Name}],must have a `Factory` suffix");
                context.AddSource($"{typeSymbol.Name}.generated.cs", builder.ToString());
                context.ReportDiagnostic(diagnostic);

            #endregion // Exception Handling
        }

        #region string rootName = .., aggregateInterfaceType = .., stateType = .., eventType = ..

        AssemblyName asm = GetType().Assembly.GetName();

        string rootName = name.Substring(0, name.Length - 7);
        string aggregateInterfaceType = $"I{rootName}";

        AttributeData att = typeSymbol.GetAttributes()
                                  .Where(att => att.AttributeClass?.Name == EventTargetAttribute)
                                  .First();
        ImmutableArray<ITypeSymbol> args = att.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        string stateType = args[0].ToDisplayString();
        ITypeSymbol eventTypeSymbol = args[1];
        string eventType = eventTypeSymbol.ToDisplayString();

        #endregion // string rootName = .., aggregateInterfaceType = .., stateType = .., eventType = ..

        #region Aggregate Interface

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.Diagnostics.DebuggerDisplay("{Kind}: {Partition.Domain}, {Partition.Partition}")]
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public interface {{aggregateInterfaceType}}: IEvDbAggregate<{{stateType}}>, {{eventType}}
                    { 
                    }
                    """);
        context.AddSource($"{aggregateInterfaceType}.generated.cs", builder.ToString());

        #endregion // Aggregate Interface

        builder.Clear();

        #region FactoryBase

        var eventsPayloads = from a in eventTypeSymbol.GetAttributes()
                   let text = a.AttributeClass?.Name
                   where text == EventTypesGenerator.EventTargetAttribute
                   let fullName = a.AttributeClass?.ToString()
                   let genStart = fullName.IndexOf('<') + 1
                   let genLen = fullName.Length - genStart - 1
                   let generic = fullName.Substring(genStart, genLen)
                   select generic;

        var foldAbstracts = eventsPayloads.Select(p =>
                $"""
                    protected virtual {stateType} Fold(
                            {stateType} state,
                            {p} payload,
                            IEvDbEventMeta meta) => state;

                """);

        var foldMap = eventsPayloads.Select(p =>
                $$"""
                        case "??":
                            {
                                var payload = e.GetData<{{p}}>(JsonSerializerOptions);
                                result = Fold(oldState, payload, e);
                                break;
                            }
                        
                """);

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    public abstract class {{name}}Base:
                        AggregateFactoryBase<{{aggregateInterfaceType}}, {{stateType}}>
                    {                    
                        #region Ctor

                        public {{typeSymbol.Name}}Base(IEvDbStorageAdapter storageAdapter): base(storageAdapter)
                        {
                        }

                        #endregion // Ctor

                        #region Create

                        public override {{aggregateInterfaceType}} Create(
                            string streamId, 
                            long lastStoredOffset = -1)
                        {
                            EvDbStreamAddress stream = new(Partition, streamId);
                            {{rootName}} agg =
                                new(
                                    _repository,
                                    Kind,
                                    stream,
                                    this,
                                    MinEventsBetweenSnapshots,
                                    DefaultState,
                                    lastStoredOffset,
                                    JsonSerializerOptions);

                            return agg;
                        }

                        public override {{aggregateInterfaceType}} Create(EvDbStoredSnapshot<{{stateType}}> snapshot)
                        {
                            EvDbStreamAddress stream = snapshot.Cursor;
                            TopStudentAggregate agg =
                                new(
                                    _repository,
                                    Kind,
                                    stream,
                                    this,
                                    MinEventsBetweenSnapshots,
                                    snapshot.State,
                                    snapshot.Cursor.Offset,
                                    JsonSerializerOptions);

                            return agg;
                        }

                        #endregion // Create 
                     
                        #region FoldEvent

                        protected override {{stateType}} FoldEvent(
                            {{stateType}} oldState, 
                            IEvDbEvent e)
                        {
                            {{stateType}} result;
                            switch (e.EventType)
                            {
                        {{string.Join("", foldMap)}}
                                default:
                                    throw new NotSupportedException(e.EventType);
                            }
                            return result;  
                        }

                        #endregion // FoldEvent

                        #region Fold

                    {{string.Join("", foldAbstracts)}}
                        #endregion // Fold
                    }
                    """);
        context.AddSource($"{name}Base.generated.cs", builder.ToString());

        #endregion // FactoryBase

        builder.Clear();

        #region Factory

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{name}}: {{name}}Base
                    { 
                    }
                    """);
        context.AddSource($"{name}.generated.cs", builder.ToString());

        #endregion // Factory
    }

    #endregion // OnGenerate
}