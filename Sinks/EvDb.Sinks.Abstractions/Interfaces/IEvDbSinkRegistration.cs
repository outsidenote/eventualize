using EvDb.Core.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Sinks.Internals;

/// <summary>
/// Use for fluent registration of the sinks
/// </summary>
public interface IEvDbSinkRegistration: IEvDbServiceCollectionWrapper
{
    string Id { get; }
}

