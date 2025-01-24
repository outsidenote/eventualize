namespace EvDb.IntegrationTests.EF;

public record AddressEntity(string? Country, string? City, string? Street)
{
    //public static implicit operator Address(AddressEntity temp) => temp.FromEntity();
    //public static implicit operator AddressEntity(Address temp) => temp.ToEntity();
}
