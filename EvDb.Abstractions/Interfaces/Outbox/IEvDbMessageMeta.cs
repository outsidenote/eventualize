namespace EvDb.Core;

public interface IEvDbMessageMeta : IEvDbEventMeta
{
    string MessageType { get; }
    EvDbChannelName Channel { get; }
}
