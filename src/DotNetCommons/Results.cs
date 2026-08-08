using System.Text.Json.Serialization;

namespace DotNetCommons;

public class Results : List<Result?>
{
    public void Fail(int index, Error error)
    {
        EnsureIndexExists(index);
        this[index] = Result.Fail(error);
    }

    public void Ok(int index)
    {
        EnsureIndexExists(index);
        this[index] = Result.Ok();
    }

    private void EnsureIndexExists(int index)
    {
        while (Count <= index)
            Add(null);
    }

    [JsonIgnore] public bool IsCompleteSuccess => this.NotNulls().All(r => r.IsSuccess);
    [JsonIgnore] public bool IsCompleteFailure => this.NotNulls().All(r => r.IsFailure);
    [JsonIgnore] public bool HasSuccess => this.NotNulls().Any(r => r.IsSuccess);
    [JsonIgnore] public bool HasFailures => this.NotNulls().Any(r => r.IsFailure);

    public static Results FailEverything(Error error)
    {
        return [error];
    }
}

public class Results<T> : List<Result<T>?>
{
    public void Fail(int index, Error error)
    {
        EnsureIndexExists(index);
        this[index] = Result<T>.Fail(error);
    }

    public void Ok(int index, T value)
    {
        EnsureIndexExists(index);
        this[index] = Result<T>.Ok(value);
    }

    public bool IsSet(int index)
    {
        return Count > index && this[index] != null;
    }

    private void EnsureIndexExists(int index)
    {
        while (Count <= index)
            Add(null);
    }

    [JsonIgnore] public bool IsCompleteSuccess => this.NotNulls().All(r => r.IsSuccess);
    [JsonIgnore] public bool IsCompleteFailure => this.NotNulls().All(r => r.IsFailure);
    [JsonIgnore] public bool HasSuccess => this.NotNulls().Any(r => r.IsSuccess);
    [JsonIgnore] public bool HasFailures => this.NotNulls().Any(r => r.IsFailure);

    public static Results<T> FailEverything(Error error)
    {
        return [error];
    }
}