﻿#pragma warning disable HAA0301 // Closure Allocation Source
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
public partial class FoldingGenerator : BaseGenerator
{
    protected override string EventTargetAttribute { get; } = "EvDbFoldingAttribute";

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
        string foldingName = typeSymbol.Name;
        string rootName = foldingName;
        if (foldingName.EndsWith("Folding"))
            rootName = foldingName.Substring(0, foldingName.Length - 7);
        else if (foldingName.EndsWith("Fold"))
            rootName = foldingName.Substring(0, foldingName.Length - 4);
        else
            foldingName = $"{foldingName}Folding";

        AssemblyName asm = GetType().Assembly.GetName();

        string aggregateInterfaceType = $"I{rootName}";

        AttributeData att = typeSymbol.GetAttributes()
                                  .Where(att => att.AttributeClass?.Name == EventTargetAttribute)
                                  .First();
        ImmutableArray<ITypeSymbol> args = att.AttributeClass?.TypeArguments ?? ImmutableArray<ITypeSymbol>.Empty;
        string stateType = args[0].ToDisplayString();
        ITypeSymbol eventTypeSymbol = args[1];
        string eventType = eventTypeSymbol.ToDisplayString();

        #endregion // string rootName = .., aggregateInterfaceType = .., stateType = .., eventType = ..

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

        #region FactoryBase

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
                    public abstract class {{foldingName}}Base:
                        IEvDbFoldingUnit<{{stateType}}>
                    {        
                        protected abstract {{stateType}} DefaultState { get; }
                        private readonly JsonSerializerOptions? _jsonSerializerOptions;
                        private {{stateType}} _state;
                                         
                        protected {{foldingName}}Base(
                            JsonSerializerOptions? jsonSerializerOptions)
                        {
                            _state = DefaultState;
                            _jsonSerializerOptions = jsonSerializerOptions;
                        }

                        {{stateType}} IEvDbFoldingUnit<{{stateType}}>.State => _state;

                        #region FoldEvent

                        void IEvDbFoldingUnit.FoldEvent(IEvDbEvent e)
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
        context.AddSource($"{foldingName}Base.generated.cs", builder.ToString());

        #endregion // FactoryBase

        builder.Clear();

        #region Folding

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{foldingName}}: {{foldingName}}Base
                    { 
                        public static IEvDbFoldingUnit<{{stateType}}> Create(JsonSerializerOptions? jsonSerializerOptions) => new {{foldingName}}(jsonSerializerOptions);

                        private {{foldingName}}(
                            JsonSerializerOptions? jsonSerializerOptions):base (jsonSerializerOptions)
                        {
                        }
                    }
                    """);
        context.AddSource($"{foldingName}.generated.cs", builder.ToString());

        #endregion // Folding
    }

    #endregion // OnGenerate
}