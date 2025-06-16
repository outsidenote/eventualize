using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Sinks;

/// <summary>
/// Use for fluent registration of the sinks
/// </summary>
public interface IEvDbSinkRegistration
{
    IServiceCollection Services { get; init; }
    string Id { get; init; }
    // EvDbSinkTarget Target { get; init; }
}

