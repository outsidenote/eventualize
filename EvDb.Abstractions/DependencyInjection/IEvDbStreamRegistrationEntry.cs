namespace EvDb.Core.Internals;

public interface IEvDbStreamRegistrationEntry: IEvDbRegistrationEntry
{
    EvDbCloudEventContext? CloudEventContext { get; }
}
