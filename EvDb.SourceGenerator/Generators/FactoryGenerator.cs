#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class FactoryGenerator : BaseGenerator
{
    internal const string EVENT_TARGET = "EvDbStreamFactory";
    internal const string EVENT_TARGET_ATTRIBUTE = "EvDbStreamFactoryAttribute";
    protected override string EventTargetAttribute => EVENT_TARGET_ATTRIBUTE;
  
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

        #region eventType = .., factoryName = ..

        AttributeData att = typeSymbol.GetAttributes()
                                  .First(att => att.AttributeClass?.Name == EventTargetAttribute);

        ImmutableArray<ITypeSymbol> args = att.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol eventTypeSymbol = args[0];
        string eventType = eventTypeSymbol.ToDisplayString();
            
        string factoryName =  $"EvDb{typeSymbol.Name}";

        #endregion // eventType = .., factoryName = ..

        string type = typeSymbol.ToType(syntax, cancellationToken);

        #region string rootName = .., interfaceType = .., stateType = ..

        string rootName = factoryName;
        if (factoryName.EndsWith("Factory"))
            rootName = factoryName.Substring(0, factoryName.Length - 7);
        else
            factoryName = $"{factoryName}Factory";

        AssemblyName asm = GetType().Assembly.GetName();

        if (rootName == typeSymbol.Name)
            rootName = $"{rootName}_";
 

        string interfaceType = $"I{rootName}";
        string factoryInterfaceType = $"{interfaceType}Factory";

        #endregion // string rootName = .., interfaceType = .., stateType = ..

        #region Aggregate Interface

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public partial interface {{interfaceType}}: IEvDbStream, {{eventType}}
                    { 
                    }
                    """);
        context.AddSource($"{interfaceType}.generated.cs", builder.ToString());

        #endregion // Aggregate Interface

        builder.Clear();

        #region Factory Interface

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")]
                    public interface {{factoryInterfaceType}}: IEvDbFactory<{{interfaceType}}>
                    { 
                    }
                    """);
        context.AddSource($"{factoryInterfaceType}.generated.cs", builder.ToString());

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
                        EvDbFactoryBase<{{interfaceType}}>
                    {                
                        #region Ctor

                        public {{factoryName}}Base(IEvDbStorageAdapter storageAdapter): base(storageAdapter)
                        {
                        }

                        #endregion // Ctor

                        #region CreateViews

                        private IImmutableList<IEvDbView> CreateViews()
                        {
                            var options = JsonSerializerOptions; 
                            var views = ViewFactories.Select(f => f(options));

                            var immutable = ImmutableList.CreateRange<IEvDbView>(views);
                            return immutable;
                        }

                        #endregion // CreateViews

                        #region Create

                        public override {{interfaceType}} Create(
                            string streamId, 
                            long lastStoredOffset = -1)
                        {
                            var views = CreateViews();
                            {{rootName}} collection =
                                new(
                                    this,
                                    views,
                                    _repository,
                                    streamId,
                                    lastStoredOffset);

                            return collection;
                        }

                        // public override {{interfaceType}} Create(EvDbStoredSnapshot? snapshot)
                        // {
                        //     EvDbStreamAddress stream = snapshot.Cursor;
                        //     {{rootName}} collection =
                        //         new(
                        //             _repository,
                        //             Kind,
                        //             stream,
                        //             _views,
                        //             MinEventsBetweenSnapshots,
                        //             snapshot.Cursor.Offset,
                        //             JsonSerializerOptions);
                        // 
                        //     return collection;
                        // }

                        #endregion // Create 

                        protected abstract Func<JsonSerializerOptions?,IEvDbView>[] ViewFactories { get; }
                    }
                    """);
        context.AddSource($"{factoryName}Base.generated.cs", builder.ToString());

        #endregion // FactoryBase

        builder.Clear();

        #region Factory

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{typeSymbol.Name}}: {{factoryName}}Base,
                            {{factoryInterfaceType}}
                    { 
                    }
                    """);
        context.AddSource($"{typeSymbol.Name}.factory.generated.cs", builder.ToString());

        #endregion // Factory

        builder.Clear();

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

        #region Collection

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
                    public partial class {{rootName}}: 
                            EvDbCollection,
                            {{eventType}},
                            {{interfaceType}}
                    { 
                        #region Ctor

                        public {{rootName}}(
                            IEvDbStreamConfig factory,
                            IImmutableList<IEvDbView> views,
                            IEvDbRepositoryV1 repository,
                            string streamId,
                            long lastStoredOffset) : 
                                base(factory, views, repository, streamId, lastStoredOffset)
                        {
                        }

                        #endregion // Ctor
                    
                        #region Add

                    {{string.Join("", adds)}}
                        #endregion // Add
                    }
                    """);
        context.AddSource($"{rootName}.generated.cs", builder.ToString());

        #endregion // Collection
    }

    #endregion // OnGenerate
}