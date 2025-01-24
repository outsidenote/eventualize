using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;

namespace EvDb.Core
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EvDbCoreCodeFixProvider)), Shared]
    public class EvDbCoreCodeFixProvider : CodeFixProvider
    {
        private static readonly SymbolRenameOptions RENAMER = new SymbolRenameOptions(true);

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(EvDbCoreAnalyzer.PartialDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                             .ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root?.FindToken(diagnosticSpan.Start)
                                   .Parent?
                                   .AncestorsAndSelf()
                                   .OfType<TypeDeclarationSyntax>()
                                   .First();

            // Register a code action that will invoke the fix.
            if (declaration != null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Fix EvDb partial type",
                        createChangedSolution: c =>
                        FixPartial(context.Document, declaration, c),
                        equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                    diagnostic);
            }
        }

        private async Task<Solution> FixPartial(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            var identifierToken = typeDecl.Identifier;
            var newName = identifierToken.Text.ToUpperInvariant();

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution,
                                                              typeSymbol!,
                                                              RENAMER,
                                                              newName,
                                                              cancellationToken);

            // Return the new solution with the now-uppercase type name.
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (syntaxRoot == null)
                return document.Project.Solution;

            //// Check if the type already has the 'partial' modifier
            //if (typeDecl.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
            //{
            //    // The type is already partial, no fix needed
            //    return document.Project.Solution;
            //}

            // Add the 'partial' modifier to the type declaration
            var updatedTypeDecl = typeDecl.WithModifiers(
                typeDecl.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            );

            // Replace the old node with the updated one
            var updatedSyntaxRoot = syntaxRoot.ReplaceNode(typeDecl, updatedTypeDecl);

            // Return the updated solution
            var updatedDocument = document.WithSyntaxRoot(updatedSyntaxRoot);
            return updatedDocument.Project.Solution;
        }
    }
}
