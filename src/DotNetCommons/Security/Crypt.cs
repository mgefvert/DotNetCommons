using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Security
{
    public static class Crypt
    {
        public static byte[] Decrypt(byte[] key, byte[] data)
        {
            using (var aes = new AesManaged())
            {
                aes.Key = PadKey(key, 32);

                using (var mem = new MemoryStream(data))
                {
                    byte[] result;

                    using (var reader = new BinaryReader(mem, Encoding.UTF8, true))
                    {
                        var ivlen = reader.ReadInt32();
                        aes.IV = reader.ReadBytes(ivlen);
                        result = new byte[reader.ReadInt32()];
                    }

                    using (var crypto = new CryptoStream(mem, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        crypto.Read(result, 0, result.Length);
                        return result;
                    }
                }
            }
        }

        public static string Decrypt(byte[] key, string data)
        {
            var bytes = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(Decrypt(key, bytes));
        }

        public static byte[] Encrypt(byte[] key, byte[] data)
        {
            using (var aes = new AesManaged())
            {
                aes.GenerateIV();
                aes.Key = PadKey(key, 32);

                using (var mem = new MemoryStream())
                {
                    mem.Write(BitConverter.GetBytes(aes.IV.Length), 0, 4);
                    mem.Write(aes.IV, 0, aes.IV.Length);
                    mem.Write(BitConverter.GetBytes(data.Length), 0, 4);

                    using (var crypto = new CryptoStream(mem, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        crypto.Write(data, 0, data.Length);
                    }

                    return mem.ToArray();
                }
            }
        }

        public static string Encrypt(byte[] key, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(Encrypt(key, bytes));
        }

        public static byte[] PadKey(byte[] key, int length)
        {
            var result = new byte[length];
            var maxlen = Math.Max(length, key.Length);
            for (var i = 0; i < maxlen; i++)
                result[i % length] ^= key[i % key.Length];

            return result;
        }

        public static void Zero(byte[] key)
        {
            for (var i = 0; i < key.Length; i++)
                key[i] = 0;
        }
    }
}
