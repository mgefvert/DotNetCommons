using System.Text.Json.Serialization;

namespace DotNetCommons;

public abstract record ResultBase
{
    [JsonPropertyName("error")]
    public Error? Error { get; init; }

    [JsonPropertyName("value")]
    public object? Value { get; init; }

    [JsonIgnore]
    public bool IsFailure => Error != null;

    [JsonPropertyName("success")]
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

public class CompoundResult : List<Result>
{
    public bool IsCompleteSuccess => this.All(r => r.IsSuccess);
    public bool IsCompleteFailure => this.All(r => r.IsFailure);
    public bool HasSuccess => this.Any(r => r.IsSuccess);
    public bool HasFailures => this.Any(r => r.IsFailure);
}

public class CompoundResult<T> : List<Result<T>>
{
    public bool IsCompleteSuccess => this.All(r => r.IsSuccess);
    public bool IsCompleteFailure => this.All(r => r.IsFailure);
    public bool HasSuccess => this.Any(r => r.IsSuccess);
    public bool HasFailures => this.Any(r => r.IsFailure);
}