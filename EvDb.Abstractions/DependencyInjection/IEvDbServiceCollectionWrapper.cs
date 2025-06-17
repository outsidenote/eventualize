using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Internals;

public interface IEvDbServiceCollectionWrapper
{
    IServiceCollection Services { get; }
}