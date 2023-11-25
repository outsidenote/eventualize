using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using NJsonSchema.Validation;
using System.Net;


namespace Core.Event
{
    public class EventType
    {
        string EventTypeName;
        private string DataSchemaString;
        public Type DataSchemaType { get; private set; }
        public JsonSchema? DataSchema { get; private set; }

        public EventType(string eventTypeName, string dataSchemaString)
        {
            EventTypeName = eventTypeName;
            DataSchemaString = dataSchemaString;
        }
        public EventType(string eventTypeName, Type dataSchemaType)
        {
            EventTypeName = eventTypeName;
            DataSchemaType = dataSchemaType;
        }

        private async Task CreateShcmea()
        {
            if (DataSchemaType != null)
                DataSchema = JsonSchema.FromType(DataSchemaType);
            else
                DataSchema = await JsonSchema.FromJsonAsync(DataSchemaString);
        }

        public async Task<Event> CreateEvent(object dataObj, string capturedBy)
        {
            if (DataSchema == null)
            {
                await CreateShcmea();
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
            return new Event(EventTypeName, DateTime.Now, capturedBy, dataString, null);
        }

        public dynamic ParseData(Event eventObj)
        {
            if (DataSchemaType == null)
                throw new ArgumentNullException(nameof(DataSchemaType));
            var obj = JsonSerializer.Deserialize(eventObj.JsonData, DataSchemaType);
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return obj;
        }
    }
}