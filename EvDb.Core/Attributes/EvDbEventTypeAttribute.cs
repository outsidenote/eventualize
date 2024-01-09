using System.Text.Json.Serialization;

namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbEventTypeAttribute<T> : Attribute
{
}
