using Microsoft.CodeAnalysis;

namespace EvDb.SourceGenerator.Helpers;

public struct PayloadInfo
{
    public PayloadInfo(INamedTypeSymbol cls)
    {
        ITypeSymbol payloadType = cls.TypeArguments.First();
        AttributeData payloadAtt = payloadType.GetAttributes()
            .First(m => m.AttributeClass?.Name.StartsWith("EvDbDefineEventPayload") ?? false);
        SnapshotStorageName = payloadAtt.ConstructorArguments.First().Value?.ToString() ?? string.Empty;
        FullTypeName = cls.TypeArguments.First().ToDisplayString();
    }

    public string FullTypeName { get; }
    public string SnapshotStorageName { get; }
}
