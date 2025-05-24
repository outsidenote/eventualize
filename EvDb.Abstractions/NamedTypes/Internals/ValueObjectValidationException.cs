namespace EvDb.Core.NamedTypes.Internals;

[Serializable]
public class ValueObjectValidationException : Exception
{
    public ValueObjectValidationException()
    {
    }

    public ValueObjectValidationException(string message) : base(message)
    {
    }

    public ValueObjectValidationException(string message, Exception inner) : base(message, inner)
    {
    }
}

