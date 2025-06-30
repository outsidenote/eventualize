using EvDb.Core.Internals;

namespace Microsoft.Extensions.DependencyInjection;

public static class CoreDependencyInjectionExtensions
{

    /// <summary>
    /// Serve as an anchor for extension method of the storage registration 
    /// </summary>
    /// <param name="services"></param>
    public static EvDbRegistrationEntry AddEvDb(this IServiceCollection services)
    {
        return new EvDbRegistrationEntry(services);
    }
}
