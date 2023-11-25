// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Text.Json.Nodes;
// using System.Xml.Schema;

// namespace Core.Event
// {
//     public static class EventDeserializer
//     {
//         public static Type GetEventType(string serializedEvent)
//         {
//             JsonNode? dict = JsonNode.Parse(serializedEvent);
//             if (dict == null) throw new ArgumentNullException(nameof(dict));
//             JsonNode datatypeNode = dict["DataTypeName"];
//             if (datatypeNode == null) throw new ArgumentNullException(nameof(datatypeNode));
//             string dataTypeName = datatypeNode.GetValue<string>();

//             JsonObject rootObject = dict as JsonObject;
//             rootObject.Remove("DataTypeName");

//             Type? eventType = Type.GetType(dataTypeName, true);
//             if (eventType == null) throw new ArgumentNullException(nameof(eventType));
//             return eventType;
//         }

//     }
// }