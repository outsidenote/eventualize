using EvDb.Core.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace EvDb.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EvDbCoreAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EvDb.Core.Analyzer";
        private const string Category = "EvDb";
        private static ImmutableHashSet<string> TARGET_ATTRIBUTES =
            ImmutableHashSet.CreateRange([
                "EvDbDefineEventPayloadAttribute",
                "EvDbDefineMessagePayloadAttribute",
                "EvDbEventTypesAttribute",
                "EvDbViewTypeAttribute",
                "EvDbOutboxAttribute",
                "EvDbStreamFactoryAttribute"]);

        private static readonly DiagnosticDescriptor PartialIsMissingRule = new DiagnosticDescriptor(
                                                        DiagnosticId,
                                                        "Partial class is missing",
                                                        "Type must be partial", 
                                                        Category,
                                                        DiagnosticSeverity.Error,
                                                        isEnabledByDefault: true,
                                                        description: "Source code generator needs a partial type");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(PartialIsMissingRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
            var attrs = namedTypeSymbol.GetAttributes();

            // Find just those named type symbols with names containing lowercase letters.
            if (!attrs.Any(a => TARGET_ATTRIBUTES.Contains(a.AttributeClass.Name)))
                return;

            if (!namedTypeSymbol.IsPartial())
                return;

            namedTypeSymbol.CreateDiagnostic(PartialIsMissingRule);            
        }
    }
}
