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
public partial class FactoryGenerator : BaseGenerator
{
    protected override string EventTargetAttribute { get; } = "EvDbStreamFactoryAttribute";

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
        string factoryName = typeSymbol.Name;
        string rootName = factoryName;
        if (factoryName.EndsWith("Factory"))
            rootName = factoryName.Substring(0, factoryName.Length - 7);
        else
            factoryName = $"{factoryName}Factory";
        rootName = $"{rootName}";
        factoryName = $"{factoryName}";

        AssemblyName asm = GetType().Assembly.GetName();

        string aggregateInterfaceType = $"I{rootName}";

        AttributeData att = typeSymbol.GetAttributes()
                                  .Where(att => att.AttributeClass?.Name == EventTargetAttribute)
                                  .First();
        ImmutableArray<ITypeSymbol> args = att.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol eventTypeSymbol = args[0];
        string eventType = eventTypeSymbol.ToDisplayString();

        #endregion // string rootName = .., aggregateInterfaceType = .., stateType = .., eventType = ..

        #region Aggregate Interface

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public interface {{aggregateInterfaceType}}: IEvDb, {{eventType}}
                    { 
                    }
                    """);
        context.AddSource($"{aggregateInterfaceType}.generated.cs", builder.ToString());

        #endregion // Aggregate Interface

        builder.Clear();

        #region Factory Interface

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public interface {{aggregateInterfaceType}}Factory: IEvDbFactory<{{aggregateInterfaceType}}>
                    { 
                    }
                    """);
        context.AddSource($"{aggregateInterfaceType}Factory.generated.cs", builder.ToString());

        #endregion // Factory Interface

        builder.Clear();

        #region FactoryBase

        #region var eventsPayloads = ...

        #endregion // var eventsPayloads = ...


        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public abstract class {{factoryName}}Base:
                        EvDbFactoryBase<{{aggregateInterfaceType}}>
                    {                
                        private readonly IImmutableList<IEvDbFoldingUnit> _foldings;
                        #region Ctor

                        public {{factoryName}}Base(IEvDbStorageAdapter storageAdapter): base(storageAdapter)
                        {
                            var options = JsonSerializerOptions; 
                            var foldings = FoldingsFactories.Select(f => f(options));
                            _foldings = ImmutableList.CreateRange<IEvDbFoldingUnit>(foldings
                            );
                        }

                        #endregion // Ctor

                        #region Create

                        public override {{aggregateInterfaceType}} Create(
                            string streamId, 
                            long lastStoredOffset = -1)
                        {
                            {{rootName}}__Aggregate agg =
                                new(
                                    this,
                                    _foldings,
                                    _repository,
                                    streamId,
                                    lastStoredOffset);

                            return agg;
                        }

                        // public override {{aggregateInterfaceType}} Create(EvDbStoredSnapshot? snapshot)
                        // {
                        //     EvDbStreamAddress stream = snapshot.Cursor;
                        //     {{rootName}}__Aggregate agg =
                        //         new(
                        //             _repository,
                        //             Kind,
                        //             stream,
                        //             _foldings,
                        //             MinEventsBetweenSnapshots,
                        //             snapshot.Cursor.Offset,
                        //             JsonSerializerOptions);
                        // 
                        //     return agg;
                        // }

                        #endregion // Create 

                        protected abstract Func<JsonSerializerOptions?,IEvDbFoldingUnit>[] FoldingsFactories { get; }
                    }
                    """);
        context.AddSource($"{factoryName}Base.generated.cs", builder.ToString());

        #endregion // FactoryBase

        builder.Clear();

        #region Factory

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{factoryName}}: {{factoryName}}Base,
                            {{aggregateInterfaceType}}Factory
                    { 
                    }
                    """);
        context.AddSource($"{factoryName}.generated.cs", builder.ToString());

        #endregion // Factory

        builder.Clear();

        #region var eventsPayloads = from a in eventTypeSymbol.GetAttributes() ...

        var eventsPayloads = from a in eventTypeSymbol.GetAttributes()
                             let cls = (INamedTypeSymbol)(a.AttributeClass!)
                             where cls != null
                             let text = cls.Name
                             where text == EventTypesGenerator.EventTarget
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

        #region Aggregate

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        var adds = eventsPayloads.Select(ep =>
                    $$"""
                        void {{eventType}}.Add(
                                {{ep.Type}} payload, 
                                string? capturedBy = null)
                        {
                            AddEvent(payload, capturedBy);
                        }

                    """);
        builder.AppendLine($$"""
                    public class {{rootName}}__Aggregate: 
                            EvDbClient,
                            {{eventType}},
                            I{{rootName}}
                    { 
                        #region Ctor

                        public {{rootName}}__Aggregate(
                            IEvDbFactory factory,
                            IEnumerable<IEvDbFoldingUnit> foldings,
                            IEvDbRepositoryV1 repository,
                            string streamId,
                            long lastStoredOffset) : 
                                base(factory, foldings, repository, streamId, lastStoredOffset)
                        {
                        }

                        #endregion // Ctor
                    
                        #region Add

                    {{string.Join("", adds)}}
                        #endregion // Add
                    }
                    """);
        context.AddSource($"{rootName}.generated.cs", builder.ToString());

        #endregion // Aggregate
    }

    #endregion // OnGenerate
}