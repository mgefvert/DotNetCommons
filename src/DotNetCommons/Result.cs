using System.Diagnostics.CodeAnalysis;

namespace DotNetCommons;

public abstract record ResultBase
{
    public Error? Error { get; protected init; }

    public bool IsFailure => Error != null;
    public bool IsSuccess => Error == null;

    public void ThrowOnFailure()
    {
        if (Error != null)
            throw Error.ToAppException();
    }
}

public record Result : ResultBase
{
    private static readonly Result DefaultSuccess = new();

    public static Result Ok() => DefaultSuccess;
    public static Result Fail(Error error) => new() { Error = error ?? throw new ArgumentNullException(nameof(error)) };

    public static implicit operator Result(Error error) => Fail(error);
}

public record Result<T> : ResultBase
{
    private T? InternalValue { get; init; }

    /// Access the result value. If unsuccessful, throws a <see cref="AppException"/>.
    public T? Value => IsSuccess ? InternalValue : throw Error!.ToAppException();

    public static Result<T> Ok(T? value) => new() { InternalValue = value };
    public static Result<T> Fail(Error error) => new() { Error = error ?? throw new ArgumentNullException(nameof(error)) };

    public static implicit operator Result<T>(T error) => Ok(error);
    public static implicit operator Result<T>(Error error) => Fail(error);
}