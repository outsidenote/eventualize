namespace Core;

public static class EventUtils
{
    public static string ValidateNonEmptyString(string value)
    {
        if (value == "") throw new ArgumentException(nameof(value));
        return value;
    }
}