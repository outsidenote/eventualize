namespace EvDb.IntegrationTests.EF;

using EvDb.IntegrationTests.EF.States;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class PersonMapper
{
    public static partial PersonEntity ToEntity(this Person person);
    public static partial Person FromEntity(this PersonEntity person);
}
