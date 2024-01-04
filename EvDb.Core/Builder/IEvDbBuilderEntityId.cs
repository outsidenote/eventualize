namespace EvDb.Core.Builder;

public interface IEvDbBuilderEntityId<T>
{
    T AddStreamId(string id);
}
