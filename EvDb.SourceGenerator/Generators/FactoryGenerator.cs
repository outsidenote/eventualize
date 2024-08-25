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
        context.ThrowIfNotPartial(typeSymbol, syntax);
        AssemblyName asm = GetType().Assembly.GetName();

        StringBuilder builder = new StringBuilder();

        #region eventType = .., factoryName = ..

        AttributeData attOfFactory = typeSymbol.GetAttributes()
                                  .First(att => att.AttributeClass?.Name == EventTargetAttribute);

        ImmutableArray<ITypeSymbol> args = attOfFactory.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        ITypeSymbol eventTypeSymbol = args[0];
        string eventType = eventTypeSymbol.ToDisplayString();

        string factoryName = $"EvDb{typeSymbol.Name}";

        #region string domain = ..., string partition = ...

        if (!attOfFactory.TryGetValue("domain", out string domain))
        {
            // TODO: Bnaya 2024-08-12 report an error
            throw new ArgumentException("domain");
        }

        if (!attOfFactory.TryGetValue("partition", out string partition))
        {
            // TODO: Bnaya 2024-08-12 report an error
            throw new ArgumentException("partition");
        }

        #endregion //  string domain = ..., string partition = ...

        #endregion // eventType = .., factoryName = ..

        string type = typeSymbol.ToType(syntax, cancellationToken);

        #region string rootName = .., interfaceType = .., stateType = ..

        string rootName = factoryName;
        if (factoryName.EndsWith("Factory"))
            rootName = factoryName.Substring(0, factoryName.Length - 7);
        else
            factoryName = $"{factoryName}Factory";


        if (rootName == typeSymbol.Name)
            rootName = $"{rootName}_";


        string interfaceType = $"I{rootName}";
        string factoryInterfaceType = $"{interfaceType}Factory";

        #endregion // string rootName = .., interfaceType = .., stateType = ..

        builder.Clear();

        #region Factory

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{typeSymbol.Name}}: {{factoryName}}Base,
                            {{factoryInterfaceType}}
                    { 
                        #region Partition

                        public override EvDbPartitionAddress PartitionAddress { get; } = 
                            new EvDbPartitionAddress("{{domain}}", "{{partition}}");

                        #endregion // PartitionAddress
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(), builder.ToString());

        #endregion // Factory

        builder.Clear();

        #region var eventsPayloads = from a in eventTypeSymbol.GetAttributes() ...

        var eventsPayloads = from a in eventTypeSymbol.GetAttributes()
                             let cls = a.AttributeClass!
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

        #region Stream

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        var adds = eventsPayloads.Select(ep =>
                    $$"""
                        async ValueTask<IEvDbEventMeta> {{eventType}}.AddAsync(
                                {{ep.Type}} payload, 
                                string? capturedBy)
                        {
                            IEvDbEventMeta meta = await AddEventAsync(payload, capturedBy);
                            return meta;
                        }

                    """);
        builder.AppendLine($$"""
                    public partial class {{rootName}}: 
                            EvDbStream,
                            {{eventType}},
                            {{interfaceType}}
                    { 
                        #region Ctor

                        public {{rootName}}(
                            IEvDbStreamConfig stramConfiguration,
                            IImmutableList<IEvDbViewStore> views,
                            IEvDbStorageStreamAdapter storageAdapter,
                            string streamId,
                            long lastStoredOffset) : 
                                base(stramConfiguration, views, storageAdapter, streamId, lastStoredOffset)
                        {
                            Views = new {{rootName}}Views(views);
                        }

                        #endregion // Ctor
                    
                        public {{rootName}}Views Views { get; }
                    
                        #region Add

                    {{string.Join("", adds)}}
                        #endregion // Add
                    }
                    """);
        context.AddSource(typeSymbol.StandardPath(rootName), builder.ToString());

        #endregion // Stream

        builder.Clear();

        #region  // DI

        builder.AppendHeader(syntax, typeSymbol, "Microsoft.Extensions.DependencyInjection");
        builder.AppendLine();

        builder.AppendLine($$"""
                    using {{typeSymbol.ContainingNamespace.ToDisplayString()}};
                    
                    [Obsolete("Deprecated")]
                    public static class {{factoryName}}Registration
                    {
                        public static IServiceCollection Add{{factoryName}}(this IServiceCollection services)
                        {
                            services.AddScoped<{{factoryInterfaceType}},{{typeSymbol.Name}}>();
                            return services;
                        }
                    }
                    """);
        // context.AddSource(typeSymbol.StandardPath("DI", $"{factoryName}Registration"), builder.ToString());

        #endregion // DI
    }

    #endregion // OnGenerate
}