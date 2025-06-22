using System.Security.Cryptography;
using System.Text;

namespace DotNetCommons.Security;

/// <summary>
/// Class that provides easy encryption and decryption of data using AES-256 encryption and CBC mode with an automatically
/// generated IV, if need be. 
/// </summary>
public class CryptIo : IDisposable
{
    public Aes Aes { get; }
    public int MessageSize { get; }
    public Stream EncryptedStream { get; }
    public Stream? GzipStream { get; }
    public Stream CryptoStream { get; }
    public Stream IoStream { get; }

    protected CryptIo(Aes aes, int messageSize, Stream encryptedStream, Stream? gzipStream, Stream cryptoStream)
    {
        Aes             = aes;
        MessageSize     = messageSize;
        EncryptedStream = encryptedStream;
        GzipStream      = gzipStream;
        CryptoStream    = cryptoStream;
        IoStream        = gzipStream ?? cryptoStream;
    }

    public virtual void Dispose()
    {
        if (GzipStream != null)
        {
            GzipStream.Flush();
            GzipStream.Dispose();
        }
        
        CryptoStream.Flush();
        CryptoStream.Dispose();

        Aes.Dispose();
    }
}

public class CryptIoReader : CryptIo
{
    public BinaryReader Reader { get; }
    
    public CryptIoReader(Aes aes, int messageSize, Stream encryptedStream, Stream? gzipStream, Stream cryptoStream)
        : base(aes, messageSize, encryptedStream, gzipStream, cryptoStream)
    {
        Reader = new BinaryReader(IoStream, Encoding.UTF8, true);
    }

    public override void Dispose()
    {
        Reader.Dispose();
        base.Dispose();
    }
}


public class CryptIoWriter : CryptIo
{
    public BinaryWriter Writer { get; }
    
    public CryptIoWriter(Aes aes, int messageSize, Stream encryptedStream, Stream? gzipStream, Stream cryptoStream)
        : base(aes, messageSize, encryptedStream, gzipStream, cryptoStream)
    {
        Writer = new BinaryWriter(IoStream, Encoding.UTF8, true);
    }

    public override void Dispose()
    {
        Writer.Flush();
        Writer.Dispose();
        base.Dispose();
    }
}
