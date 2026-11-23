using System.Text;
using DotNetCommons.Collections;

namespace DotNetCommons.IO;

public class FortuneCookieReader
{
    public DrawList<string> DrawList { get; } = new();
    public List<string> Fortunes { get; } = [];

    public void Clear()
    {
        DrawList.Clear();
        Fortunes.Clear();
    }

    public void Load(string filename)
    {
        using var file = new FileStream(filename, FileMode.Open, FileAccess.Read);
        Load(file);
    }

    public void Load(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var sb = new StringBuilder();
        while (reader.ReadLine() is { } line)
        {
            if (line != "%")
                sb.AppendLine(line);
            else
            {
                AddFortune(sb);
                sb.Clear();
            }
        }

        AddFortune(sb);
    }

    private void AddFortune(StringBuilder fortune)
    {
        var value = fortune.ToString().TrimEnd();
        if (!string.IsNullOrWhiteSpace(value))
            Fortunes.Add(value);
    }

    public string? Random()
    {
        if (Fortunes.IsEmpty())
            return null;

        if (DrawList.Count() == 0)
            DrawList.Seed(Fortunes);

        return DrawList.Draw();
    }
}
