using System.Globalization;
using System.Runtime.CompilerServices;

namespace DotNetCommons.Text.Csv;

public class CsvConverter<T>
    where T : class, new()
{
    public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

/*    public string Convert(T item)
    {
    }

    public T Parse(string data)
    {
    }

    public async IAsyncEnumerable<T> Read(StreamReader reader, [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (!reader.EndOfStream)
        {
            var item = await ReadOne(reader, ct);
            if (item != null)
                yield return item;
            else
                yield break;
        }
    }

    public async Task<T?> ReadOne(StreamReader reader, CancellationToken ct = default)
    {
        var line = await reader.ReadLineAsync(ct);
        return line == null ? null : Parse(line);
    }

    public async Task Write(StreamWriter writer, IEnumerable<T> items, CancellationToken ct = default)
    {
        foreach (var item in items)
        {
            if (ct.IsCancellationRequested)
                break;

            await WriteOne(writer, item, ct);
        }
    }

    public async Task WriteOne(StreamWriter writer, T item, CancellationToken ct = default)
    {
        var line = Convert(item);
        await writer.WriteLineAsync(line);
    }*/
}