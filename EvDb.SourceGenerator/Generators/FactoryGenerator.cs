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

        builder.Clear();

        #region  // DI

        builder.ClearAndAppendHeader(syntax, typeSymbol, "Microsoft.Extensions.DependencyInjection");
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