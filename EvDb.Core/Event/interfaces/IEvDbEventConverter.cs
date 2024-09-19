using System.Text.Json;

namespace EvDb.Core;



public interface IEvDbEventConverter
{
    T GetData<T>(JsonSerializerOptions? options = null);
}
