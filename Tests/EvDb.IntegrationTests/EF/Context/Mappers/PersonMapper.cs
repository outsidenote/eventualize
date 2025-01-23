namespace EvDb.IntegrationTests.EF;

using EvDb.IntegrationTests.EF.States;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class PersonMapper
{
    [MapperIgnoreTarget(nameof(PersonEntity.Emails))]
    [MapperIgnoreSource(nameof(Person.Emails))]
    public static partial PersonEntity ToEntity(this Person source);

    public static partial Person FromEntity(this PersonEntity source);

    public static partial AddressEntity ToEntity(this Address source);

    public static partial Address FromEntity(this AddressEntity source);

    [MapperIgnoreSource(nameof(EmailEntity.PersonId))]
    public static partial Email FromEntity(this EmailEntity source);

    public static partial EmailEntity ToEntity(this Email source, int personId);
}
    