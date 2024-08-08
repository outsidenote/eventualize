using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Internals;

/// <summary>
/// The an anchor object for extension method of the storage registration 
/// </summary>
public readonly record struct EvDbRegistrationEntry (IServiceCollection Services);


