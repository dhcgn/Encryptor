using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Encryptor.Engine
{
    public class Encryptor
    {
        public const string FileExtension = ".enc";

        private enum Status
        {
            ReadFile,
            EncryptData,
            CreateHmac,
            WriteAllBytes,
            Done,
            DecryptData
        }

        private static readonly Dictionary<Status, Double> EncryptionProgress = new Dictionary<Status, double>()
        {
            {Status.ReadFile, 10},
            {Status.EncryptData, 30},
            {Status.CreateHmac, 50},
            {Status.WriteAllBytes, 70},
            {Status.Done, 100},
        };

        private static readonly Dictionary<Status, Double> DecryptionProgress = new Dictionary<Status, double>()
        {
            {Status.ReadFile, 10},
            {Status.CreateHmac, 30},
            {Status.DecryptData, 50},
            {Status.WriteAllBytes, 70},
            {Status.Done, 100},
        };

        public static string Encrypt(string path, string password, bool replaceFilename = false,
            Action<double, string> statusCallback = null)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            SetStatusEncryption(statusCallback, Status.ReadFile);
            var bytes = File.ReadAllBytes(path);

            var iv = CryptoProvider.CreateIV();

            var plainContent = new PlainContent
            {
                Data = bytes,
                Filename = Path.GetFileName(path)
            };

            SetStatusEncryption(statusCallback, Status.EncryptData);
            var encryptedBytes = CryptoProvider.EncryptData(plainContent.GetBytes(), iv, password);

            SetStatusEncryption(statusCallback, Status.CreateHmac);
            var hmac = CryptoProvider.CreateHmac(iv, encryptedBytes, password);

            var newPath = CreateEnryptedFilename(path, replaceFilename);

            var container = new ContainerV1(iv, encryptedBytes, hmac);

            SetStatusEncryption(statusCallback, Status.WriteAllBytes);
            File.WriteAllBytes(newPath, container.GetBytes());

            SetStatusEncryption(statusCallback, Status.Done);
            return newPath;
        }

        private static void SetStatusEncryption(Action<double, string> statusCallback, Status status)
        {
            statusCallback?.Invoke(EncryptionProgress[status], status.ToString());
        }
        private static void SetStatusDecryption(Action<double, string> statusCallback, Status status)
        {
            statusCallback?.Invoke(DecryptionProgress[status], status.ToString());
        }

        private static string CreateEnryptedFilename(string path, bool replaceFilename)
        {
            if (!replaceFilename)
                return $"{path}{FileExtension}";

            return Path.Combine(Path.GetDirectoryName(path), Guid.NewGuid() + FileExtension);
        }

        private class PlainContent
        {
            public byte[] Data { get; set; }
            public string Filename { get; set; }

            public byte[] GetBytes()
            {
                var filenameBytes = Encoding.UTF8.GetBytes(this.Filename);
                return (BitConverter.GetBytes(filenameBytes.Length)).Concat(filenameBytes).Concat(this.Data).ToArray();
            }

            public static PlainContent FromBytes(byte[] decryptedBytes)
            {
                var filenameLength = BitConverter.ToInt32(decryptedBytes.Take(4).ToArray(), 0);

                var result = new PlainContent
                {
                    Filename = Encoding.UTF8.GetString(decryptedBytes.Skip(4).Take(filenameLength).ToArray()),
                    Data = decryptedBytes.Skip(4 + filenameLength).ToArray()
                };
                return result;
            }
        }

        public static string Decrypt(string path, string password, Action<double, string> statusCallback = null)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            if (!IsEncryptorFile(path))
                throw new Exception("File has wrong format (magic number).");

            SetStatusDecryption(statusCallback, Status.ReadFile);
            var bytes = File.ReadAllBytes(path);

            var container = ContainerV1.FromBytes(bytes);

            SetStatusDecryption(statusCallback, Status.CreateHmac);
            var hmac = CryptoProvider.CreateHmac(container.IV, container.Data, password);
            if (!container.Hmac.SequenceEqual(hmac))
            {
                throw new EncryptorTampertOrWrongPassphraseException();
            }

            SetStatusDecryption(statusCallback, Status.DecryptData);
            var decryptedBytes = CryptoProvider.DecryptData(container.Data, container.IV, password);

            var plainContent = PlainContent.FromBytes(decryptedBytes);

            SetStatusDecryption(statusCallback, Status.WriteAllBytes);
            var fullpath = Path.Combine(Path.GetDirectoryName(path),plainContent.Filename);
            File.WriteAllBytes(fullpath, plainContent.Data);

            SetStatusDecryption(statusCallback, Status.Done);
            return fullpath;
        }

        public static bool IsEncryptorFile(string path)
        {
            byte[] buffer = new byte[8];
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                }

                return buffer.SequenceEqual(Config.MagicBytes);
            }
            catch (Exception exception)
            {
                return false;
            }
        }
    }

    public class EncryptorTampertOrWrongPassphraseException : Exception
    {
    }
}