using Generator.Equals;
using System.Text.Json;

namespace Eventualize.Core;

[Equatable]
public partial record EventualizeEvent<T>(string EventType,
                                       [property: IgnoreEquality] DateTime CapturedAt,
                                       string CapturedBy,
                                       T Data) : IEventualizeEvent
{
    string IEventualizeEvent.GetData()
    {
        var json = JsonSerializer.Serialize(Data);
        return json;
    }
}
