using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace commons;

public static class Helper
{
    private static readonly HttpClient Client;

    static Helper()
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");
    }

    public static CsvReader GetCsvReader<T>(TextReader reader, CultureInfo culture, bool hasHeader, string delimiter)
        where T : ClassMap
    {
        var csv = new CsvReader(reader, new CsvConfiguration(culture)
        {
            HasHeaderRecord = hasHeader,
            Delimiter = delimiter
        });
        csv.Context.RegisterClassMap<T>();
        return csv;
    }

    public static byte[] DownloadOrCache(string cacheId, Uri url)
    {
        Client.DefaultRequestHeaders.Referrer = new Uri(url.GetLeftPart(UriPartial.Authority));
        
        var cacheFile = cacheId + ".cache";
        var fileInfo  = new FileInfo(cacheFile);

        if (fileInfo.Exists && fileInfo.LastWriteTime.Date == DateTime.Today)
        {
            var result = File.ReadAllBytes(fileInfo.FullName);
            Console.WriteLine($"{cacheId}: Loaded {result.Length} bytes from cache");
            return result;
        }

        var data = Client.GetByteArrayAsync(url).Result;

        File.WriteAllBytes(fileInfo.FullName, data);
        Console.WriteLine($"{cacheId}: Downloaded {data.Length} bytes");
        return data;
    }

    public static string DownloadOrCacheString(string cacheId, Uri url)
    {
        var data = DownloadOrCache(cacheId, url);
        return Encoding.UTF8.GetString(data);
    }
}