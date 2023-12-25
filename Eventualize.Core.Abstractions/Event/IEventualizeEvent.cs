namespace Eventualize.Core;

// TODO: Json data -> JsonElement if needed at all

public interface IEventualizeEvent
{ 
    string EventType { get; }
    DateTime CapturedAt { get; }
    string CapturedBy { get; }

    [Obsolete("Temp solution, might be solve via entity class")]
    string GetData();
}
