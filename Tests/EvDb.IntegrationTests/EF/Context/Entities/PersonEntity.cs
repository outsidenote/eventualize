namespace EvDb.IntegrationTests.EF;

public record PersonEntity(int Id,
                            string Name,
                            DateOnly Birthday,
                            Email[] Emails,
                            AddressEntity Address)
{
}
