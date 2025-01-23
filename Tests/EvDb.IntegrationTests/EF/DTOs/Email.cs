namespace EvDb.IntegrationTests.EF;

public readonly partial record struct Email(string Value, string Domain, string Category) 
{
    public static implicit operator Email(string value)
    {
        int len = value.IndexOf('@');
        return new Email(value, value.Substring(len + 1), "Other");
    }
    public static implicit operator string(Email value) => $"{value.Value}@{value.Domain}";
}
