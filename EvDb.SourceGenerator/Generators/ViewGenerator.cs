﻿#pragma warning disable HAA0301 // Closure Allocation Source
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

        #endregion // string baseName = .., aggregateInterfaceType = .., stateType = .., eventType = ..

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
        builder.AppendLine();

        builder.AppendLine($$"""
                    [System.CodeDom.Compiler.GeneratedCode("{{asm.Name}}","{{asm.Version}}")] 
                    public abstract class {{viewClassName}}Base: 
                        EvDbView<{{stateType}}>
                    {                                
                        public const string ViewName = "{{name}}";
                    
                        #region Ctor

                        protected {{viewClassName}}Base(
                            EvDbStreamAddress address, 
                            JsonSerializerOptions? options):
                                base(new EvDbViewAddress(address, ViewName), 
                                EvDbStoredSnapshot.Empty,
                                options)
                        {
                        }

                        protected {{viewClassName}}Base(
                            EvDbStreamAddress address,
                            EvDbStoredSnapshot snapshot, 
                            JsonSerializerOptions? options):
                                base(
                                    new EvDbViewAddress(address, ViewName), 
                                    snapshot,
                                    options)
                        {
                        }

                        #endregion // Ctor

                        #region OnFoldEvent

                        protected override void OnFoldEvent(EvDbEvent e)
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
        context.AddSource($"{viewClassName}Base.generated.cs", builder.ToString());

        #endregion // ViewBase

        builder.Clear();

        #region View

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    partial {{type}} {{typeSymbol.Name}}: {{viewClassName}}Base
                    { 
                        internal {{typeSymbol.Name}}(
                            EvDbStreamAddress address, 
                            JsonSerializerOptions? options):
                                    base (address,options)
                        {
                        }

                        internal {{typeSymbol.Name}}(
                            EvDbStreamAddress address,
                            EvDbStoredSnapshot snapshot, 
                            JsonSerializerOptions? options):
                                base (
                                    address,
                                    snapshot,
                                    options)
                        {
                        }
                    }
                    """);
        context.AddSource($"{typeSymbol.Name}.generated.cs", builder.ToString());

        #endregion // View

        builder.Clear();

        #region View Factory

        builder.AppendHeader(syntax, typeSymbol);
        builder.AppendLine();

        builder.AppendLine($$"""
                    internal class {{typeSymbol.Name}}Factory: IEvDbViewFactory
                    { 
                        public static readonly IEvDbViewFactory Default = new {{typeSymbol.Name}}Factory();

                        string IEvDbViewFactory.ViewName { get; } = "{{name}}";

                        IEvDbViewStore IEvDbViewFactory.CreateEmpty(EvDbStreamAddress address, JsonSerializerOptions? options) => 
                                new {{typeSymbol.Name}}(address, options);

                        IEvDbViewStore IEvDbViewFactory.CreateFromSnapshot(EvDbStreamAddress address,
                            EvDbStoredSnapshot snapshot,
                            JsonSerializerOptions? options) => 
                                new {{typeSymbol.Name}}(address, snapshot, options);
                    }
                    """);
        context.AddSource($"{typeSymbol.Name}Factory.generated.cs", builder.ToString());

        #endregion // View Factory
    }

    #endregion // OnGenerate
}