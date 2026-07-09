using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security;

public class MySqlCnfReader
{
    public class Parameters
    {
        public string? Host { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
    }

    public Dictionary<string, Parameters> Entries { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Load()
    {
        return Load(Environment.ExpandEnvironmentVariables(@"%APPDATA%\MySQL\.mylogin.cnf"));
    }

    public bool Load(string fileName)
    {
        if (!File.Exists(fileName))
            return false;

        using var fs = new FileStream(fileName, FileMode.Open);

        Parameters? p = null;
        foreach (var line in ReadEncryptedLines(fs))
        {
            if (line.StartsWith('['))
            {
                var section = line.Mid(1, -2);
                p = new Parameters();
                Entries[section] = p;
            }
            else if (line.Contains('=') && p != null)
            {
                var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 2)
                {
                    var key = parts[0].ToLowerInvariant();
                    var value = parts[1].Trim('"');

                    switch (key)
                    {
                        case "host":
                            p.Host = value;
                            break;
                        case "user":
                            p.User = value;
                            break;
                        case "password":
                            p.Password = value;
                            break;
                    }
                }
            }
        }

        return true;
    }

    public string? GetConnectionString(string loginPath, string? databaseName = null)
    {
        var p = Entries.GetValueOrDefault(loginPath);
        if (p == null)
            return null;

        var result = $"Server={p.Host}; User={p.User}; Password={p.Password}";
        if (databaseName != null)
            result +=  $"; Database={databaseName}";

        return result;
    }

    private static IEnumerable<string> ReadEncryptedLines(FileStream fs)
    {
        using var reader = new BinaryReader(fs);

        _ = reader.ReadInt32();
        var loginKey = reader.ReadBytes(20);
        var key      = new byte[16];
        for (var i=0; i<loginKey.Length; i++)
            key[i % 16] ^= loginKey[i];

        var aes = Aes.Create();
        aes.Key = key;

        while (fs.Position < fs.Length - 4)
        {
            var recordLen = reader.ReadInt32();
            var encrypted = reader.ReadBytes(recordLen);

            var plain = aes.DecryptEcb(encrypted, PaddingMode.PKCS7);
            var line  = Encoding.Default.GetString(plain).Trim();

            if (line.IsSet())
                yield return line;
        }
    }
}