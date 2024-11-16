using Microsoft.CodeAnalysis;

namespace EvDb.SourceGenerator;

public struct ShardInfo
{
    public ShardInfo(IFieldSymbol field)
    {
        Name = field.Name;
        Value = field.ConstantValue?.ToString()!;
    }
    public string Name { get; }
    public string Value { get; }
}
