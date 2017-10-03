using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace DotNetCommons.IO
{
    public class ProtectedStorage
    {
        private SecureString Key { get; }

        public ProtectedStorage(string seed)
        {
            Key = new SecureString();

            foreach(var c in GetType().Name)
                Key.AppendChar(c);

            for (int i = 0; i < seed.Length; i++)
            {
                Key.AppendChar(seed[i]);
                Key.AppendChar((char)((byte)seed[i] ^ 0x8B));
                Key.AppendChar((char)i);
            }

            Key.MakeReadOnly();
        }

        private byte[] DeriveKey(byte[] iv)
        {
            var ptr = IntPtr.Zero;
            try
            {
                unsafe
                {
                    ptr = Marshal.SecureStringToGlobalAllocAnsi(Key);

                    var bptr = (byte*)ptr;
                    var c = 0;

                    for (var i = 0; i < Key.Length; i++)
                    {
                        *bptr = (byte) (*bptr ^ iv[c]);
                        if (++c >= iv.Length)
                            c = 0;
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    using (var stream = new UnmanagedMemoryStream(bptr, Key.Length))
                        return new SHA256Managed().ComputeHash(stream);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeGlobalAllocAnsi(ptr);
            }
        }

        public string Protect(string data)
        {
            using (var aes = new AesManaged())
            {
                aes.GenerateIV();
                aes.Key = DeriveKey(aes.IV);

                using (var crypt = aes.CreateEncryptor())
                using (var result = new MemoryStream())
                {
                    using (var encryptStream = new CryptoStream(result, crypt, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(encryptStream))
                    {
                        writer.Write(data);
                    }

                    return Convert.ToBase64String(result.ToArray()) + "$" + Convert.ToBase64String(aes.IV);
                }
            }
        }

        public string Unprotect(string data)
        {
            using (var aes = new AesManaged())
            {
                var dataArray = data.Split('$');
                aes.IV = Convert.FromBase64String(dataArray[1]);
                var buffer = Convert.FromBase64String(dataArray[0]);

                aes.Key = DeriveKey(aes.IV);

                using (var decrypt = aes.CreateDecryptor())
                using (var source = new MemoryStream(buffer))
                using (var decryptStream = new CryptoStream(source, decrypt, CryptoStreamMode.Read))
                using (var reader = new StreamReader(decryptStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
