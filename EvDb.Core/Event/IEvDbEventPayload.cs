using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public interface IEvDbEventPayload
{
    string EventType { get; }
}
