namespace EvDb.Core.NamedTypes.Internals;

public sealed class ValueObjectOrError<T>
{
    private readonly bool _isSuccess;

    private readonly T? _valueObject;

    private readonly Validation? _error;

    public bool IsSuccess => _isSuccess;

    public Validation Error
    {
        get
        {
            if (!_isSuccess)
            {
                return _error!;
            }

            return Validation.Ok;
        }
    }

    public T ValueObject
    {
        get
        {
            if (!_isSuccess)
            {
                throw new InvalidOperationException("Cannot access the value object as it is not valid: " + _error?.ErrorMessage);
            }

            return _valueObject!;
        }
    }

    public ValueObjectOrError(T valueObject)
    {
        _isSuccess = true;
        _valueObject = valueObject;
        _error = Validation.Ok;
    }

    public ValueObjectOrError(Validation error)
    {
        _isSuccess = false;
        _valueObject = default(T);
        _error = error;
    }
}

