namespace EvDb.IntegrationTests.EF;

public record AddressEntity(string Country, string City, string Street)
{
    public static implicit operator Address(AddressEntity temp)
    {
        return new Address(temp.Country, temp.City, temp.Street);
    }
    public static implicit operator AddressEntity(Address temp)
    {
        return new AddressEntity(temp.Country, temp.City, temp.Street);
    }
}
