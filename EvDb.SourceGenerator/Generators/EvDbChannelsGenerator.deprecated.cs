﻿/*// Ignore Spelling: Topic

#pragma warning disable HAA0301 // Closure Allocation Source
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
using EvDb.SourceGenerator.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EvDb.SourceGenerator;

[Generator]
public partial class EvDbChannelsGenerator : BaseGenerator
{
    internal const string ATT = "EvDbOutboxChannelsAttribute";
    protected override string EventTargetAttribute { get; } = ATT;

    #region OnGenerate

    /// <summary>
    /// Called when [generate].
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="compilation">The compilation.</param>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="syntax">The syntax.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    protected override void OnGenerate(
            SourceProductionContext context,
            Compilation compilation,
            INamedTypeSymbol typeSymbol,
            TypeDeclarationSyntax syntax,
            CancellationToken cancellationToken)
    {
        StringBuilder builder = new StringBuilder();
        string clsName = typeSymbol.Name;


        var tableConstants = typeSymbol.GetMembers()
                             .Where(m => m.Kind == SymbolKind.Field)
                             .Select(m => (IFieldSymbol)m)
                             .Select(m => new ChannelInfo(m))
                             .ToArray();

        #region Enum

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();
        builder.DefaultsOnType(typeSymbol, false);
        builder.AppendLine($$"""
                    public enum {{clsName}}Preferences
                    {
                    {{string.Join(",", tableConstants.Select(t =>
                    $$"""

                            {{t.Name}}   
                        """))}}
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{clsName}Preferences"), builder.ToString());

        #endregion // Enum

        #region Extensions

        builder.ClearAndAppendHeader(syntax, typeSymbol);
        builder.AppendLine();
        builder.DefaultsOnType(typeSymbol);
        builder.AppendLine($$"""
                    public static class {{clsName}}ChoisesExtensions
                    { 
                        public static IEnumerable<EvDbChannelName> ToChannelName(this {{clsName}}Preferences[] options) => options.Select(m => m.ToShardName());

                        public static EvDbChannelName ToShardName(this {{clsName}}Preferences option)
                        {
                            string table = option switch
                            {
                    {{string.Join("", tableConstants.Select(t =>
                    $$"""

                                    {{clsName}}Preferences.{{t.Name}} => {{clsName}}.{{t.Name}},
                        """))}}
                                _ => throw new NotSupportedException(),
                            };
                            return table;
                        }
                    }
                    """);
        context.AddSource(typeSymbol.StandardPathIgnoreSymbolName($"{clsName}ChoisesExtensions"), builder.ToString());

        #endregion // Extensions
    }

    #endregion // OnGenerate

    #region ChannelInfo

    public struct ChannelInfo
    {
        public ChannelInfo(IFieldSymbol field)
        {
            Name = field.Name;
            Value = field.ConstantValue?.ToString()!;
        }
        public string Name { get; }
        public string Value { get; }
    }

    #endregion //  ShardInfo
}
*/