using EvDb.Core;
using EvDb.Core.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
