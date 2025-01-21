using EvDb.IntegrationTests.EF.States;

namespace EvDb.IntegrationTests.EF;

public record PersonEntity(int Id,
                            string Name,
                            DateOnly Birthday,
                            Email[] Emails,
                            AddressEntity Address)
{
    public static implicit operator Person(PersonEntity temp)
    {
        return new Person(Id: temp.Id,
                          Name: temp.Name,
                          Birthday: temp.Birthday,
                          Emails: temp.Emails,
                          Address: temp.Address);
    }
    public static implicit operator PersonEntity(Person temp)
    {
        return new Person(Id: temp.Id,
                          Name: temp.Name,
                          Birthday: temp.Birthday,
                          Emails: temp.Emails,
                          Address: temp.Address);
    }
}
