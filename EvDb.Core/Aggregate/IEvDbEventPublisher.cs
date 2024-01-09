namespace EvDb.Core;

public interface IEvDbEventPublisher
{
    void AddEvent(IEvDbEvent e);
}