namespace EvDb.Core.Tests;
using EvDb.UnitTests;

public record StreamAndServiceProvider(
    IEvDbSchoolStream Stream,
    IServiceProvider ServiceProvider);
