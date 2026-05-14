namespace DotNetCommons;

public record Result
{
    public bool IsFailure => !IsSuccess;
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error     = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error ?? throw new ArgumentNullException(nameof(error)));

    /// Implicit conversion that wraps an <see cref="Error"/> into a Result object.
    public static implicit operator Result(Error error) => Failure(error);

    public void ThrowOnFailure()
    {
        if (Error != null)
            throw Error.ToAppException();
    }
}

public record Result<T> : Result
{
    /// Access the result value. If unsuccessful, throws a <see cref="AppException"/>.
    public T? Value => IsSuccess ? field : throw Error!.ToAppException();

    private Result(T value) : base(true, null) => Value = value;
    private Result(Error error) : base(false, error) { }

    /// Implicit conversion that wraps a return value into a Result object.
    public static implicit operator Result<T>(T value) => new(value);

    /// Implicit conversion that wraps an <see cref="Error"/> into a Result object.
    public static implicit operator Result<T>(Error error) => new(error);
}