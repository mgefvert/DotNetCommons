using System.Text.Json.Serialization;

namespace DotNetCommons;

public abstract record ResultBase
{
    [JsonPropertyName("error"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Error? Error { get; init; }

    [JsonPropertyName("value"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Value { get; init; }

    [JsonIgnore] public bool IsFailure => Error != null;
    public bool IsSuccess => Error == null;

    public void ThrowOnFailure()
    {
        if (Error != null)
            throw Error.ToException();
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
    public new T? Value
    {
        get => (T?)base.Value;
        init => base.Value = value;
    }

    public static Result<T> Ok(T? value) => new() { Value = value };
    public static Result<T> Fail(Error error) => new() { Error = error ?? throw new ArgumentNullException(nameof(error)) };

    public static implicit operator Result<T>(T error) => Ok(error);
    public static implicit operator Result<T>(Error error) => Fail(error);
}
