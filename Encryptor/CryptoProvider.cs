using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Encryptor.Engine
{
    public class CryptoProvider
    {
        public static byte[] EncryptData(byte[] data, byte[] iv, string password)
        {
            var key = CreateKey(password, SaltType.AesEncryption);

            var algorithm = new RijndaelManaged
            {
                KeySize = 256,
                Key = key,
                IV = iv,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC
            };


            Console.Out.WriteLine("EnC, Key: " + BitConverter.ToString(key) + ", IV: " + BitConverter.ToString(iv));

            using (var stream = new MemoryStream())
            using (var encryptor = algorithm.CreateEncryptor())
            using (var encrypt = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(data, 0, data.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        public static byte[] DecryptData(byte[] data, byte[] iv, string password)
        {
            var key = CreateKey(password, SaltType.AesEncryption);

            var algorithm = new RijndaelManaged
            {
                KeySize = 256,
                Key = key,
                IV = iv,
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC
            };

            Console.Out.WriteLine("DeC, Key: " + BitConverter.ToString(key) + ", IV: " + BitConverter.ToString(iv));

            using (var stream = new MemoryStream())
            using (var decryptor = algorithm.CreateDecryptor())
            using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(data, 0, data.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        private static byte[] CreateKey(string password, SaltType saltType = SaltType.Undef)
        {
            string saltTest = null;
            switch (saltType)
            {
                case SaltType.Undef:
                    saltTest =
                        "They who can give up essential liberty to obtain a little temporary safety deserve neither liberty nor safety.";
                    break;
                case SaltType.AesEncryption:
                    saltTest =
                        "These aren't the droids you're looking for.";
                    break;
                case SaltType.Hmac:
                    saltTest =
                        "To deny our own impulses is to deny the very thing that makes us human.";
                    break;
                default:
                    throw new Exception($"SaltType {saltType} is missing.");
            }

            var salt = Encoding.UTF8.GetBytes(saltTest);

            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, Config.Iterations);

            return key.GetBytes(32);
        }

        enum SaltType
        {
            Undef = 0,
            AesEncryption = 1,
            Hmac = 2,
        }

        public static byte[] CreateIV()
        {
            var provider = new RNGCryptoServiceProvider();
            var iv = new byte[16];

            provider.GetBytes(iv);

            return iv;
        }

        public static byte[] CreateHmac(byte[] iv, byte[] encryptedBytes, string password)
        {
            var data = encryptedBytes.Concat(iv).ToArray();

            byte[] result;
            using (var hmacsha512 = new HMACSHA512(data))
            {
                result = hmacsha512.ComputeHash(CreateKey(password, SaltType.Hmac));
            }

            return result;
        }
    }
}