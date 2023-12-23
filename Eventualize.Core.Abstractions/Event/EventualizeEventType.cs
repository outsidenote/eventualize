using NJsonSchema;
using NJsonSchema.Validation;
using System.Text.Json;


namespace Eventualize.Core;

public class EventualizeEventType
{
    public string EventTypeName { get; private set; }
    private string? DataSchemaString;
    public Type? DataSchemaType { get; private set; }
    public JsonSchema? DataSchema { get; private set; }

    public EventualizeEventType(string eventTypeName, string dataSchemaString)
    {
        EventTypeName = eventTypeName;
        DataSchemaString = dataSchemaString;
    }
    public EventualizeEventType(string eventTypeName, Type dataSchemaType)
    {
        EventTypeName = eventTypeName;
        DataSchemaType = dataSchemaType;
    }

    private async Task CreateShcmea()
    {
        if (DataSchemaType != null)
            DataSchema = JsonSchema.FromType(DataSchemaType);
        else if (DataSchemaString != null)
            DataSchema = await JsonSchema.FromJsonAsync(DataSchemaString);
        else
            throw new ArgumentNullException(nameof(DataSchemaType));
    }

    // TODO: [bnaya 2023-12-13] Remove the schema!
    public EventualizeEvent CreateEvent(
                                object dataObj,
                                string capturedBy)
    {
        if (DataSchema == null)
        {
            CreateShcmea().Wait();
            if (DataSchema == null) throw new ArgumentNullException(nameof(DataSchema));
        }
        string dataString = JsonSerializer.Serialize(dataObj, dataObj.GetType());
        var errors = DataSchema.Validate(dataString);
        if (errors.Count > 0)
        {
            Dictionary<string, ValidationErrorKind> validationErrors = new Dictionary<string, ValidationErrorKind>();
            foreach (var error in errors)
            {
                if (string.IsNullOrEmpty(error.Path)) continue;
                validationErrors.Add(error.Path, error.Kind);
            }
            throw new ArgumentException($"Event Data Validation Error: {JsonSerializer.Serialize(validationErrors)}");
        }
        return new EventualizeEvent(EventTypeName, DateTime.Now, capturedBy, dataString);
    }

    public dynamic ParseData(EventualizeEvent eventObj)
    {
        if (DataSchemaType == null)
            throw new ArgumentNullException(nameof(DataSchemaType));
        var obj = JsonSerializer.Deserialize(eventObj.JsonData, DataSchemaType);
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        return obj;
    }
}