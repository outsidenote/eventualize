namespace EvDb.Core;

public interface IEvDbTopicTransformer
{
    byte[] Transform(byte[] payload);
}
