using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Encryptor.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Encryptor.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void EncryptDecrypt_Sucess()
        {
            var filepath = WriteTestFileToRandomLocation("Picture.jpg");

            var encryptedFile = Engine.Encryptor.Encrypt(filepath, "qwert");

            Assert.IsTrue(File.Exists(encryptedFile), "Does file exists.");
            Assert.IsTrue(Engine.Encryptor.IsEncryptorFile(encryptedFile), "Is this a EncryptorFile");


            var decrypt = Engine.Encryptor.Decrypt(encryptedFile, "qwert");

            Assert.IsTrue(File.Exists(decrypt), "Does file exists.");

            Assert.IsTrue(File.ReadAllBytes(filepath).SequenceEqual(File.ReadAllBytes(decrypt)));
        }

        [TestMethod]
        public void EncryptDecrypt_HideFilename_Sucess()
        {
            var filepath = WriteTestFileToRandomLocation("Picture.jpg");

            var encryptedFilename = Engine.Encryptor.Encrypt(filepath, "qwert", true);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(encryptedFilename);

            Guid guid;
            Assert.IsTrue(Guid.TryParse(fileNameWithoutExtension, out guid));

            Assert.IsTrue(File.Exists(encryptedFilename), "Does file exists.");
            Assert.IsTrue(Engine.Encryptor.IsEncryptorFile(encryptedFilename), "Is this a EncryptorFile");


            var decryptedFileName = Engine.Encryptor.Decrypt(encryptedFilename, "qwert");

            Assert.IsTrue(File.Exists(decryptedFileName), "Does file exists.");

            Assert.IsTrue(File.ReadAllBytes(filepath).SequenceEqual(File.ReadAllBytes(decryptedFileName)));
        }


        [TestMethod]
        public void Encrypt_Sucess()
        {
            var filepath = WriteTestFileToRandomLocation("Picture.jpg");

            var encryptedFile = Engine.Encryptor.Encrypt(filepath, "qwert");

            Assert.IsTrue(File.Exists(encryptedFile), "Does file exists.");
            Assert.IsTrue(Engine.Encryptor.IsEncryptorFile(encryptedFile), "Is this a EncryptorFile");
        }

        [TestMethod]
        [ExpectedException(typeof (EncryptorTampertOrWrongPassphraseException))]
        public void Decrypt_Tampert_Fail()
        {
            var filepath = WriteTestFileToRandomLocation("Picture.jpg.enc.tampert");

            var decrypt = Engine.Encryptor.Decrypt(filepath, "qwert");

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof (EncryptorTampertOrWrongPassphraseException))]
        public void Decrypt_WrongPassword_Fail()
        {
            var filepath = WriteTestFileToRandomLocation("Picture.jpg.enc");

            var decrypt = Engine.Encryptor.Decrypt(filepath, ".......");

            Assert.Fail();
        }

        [TestMethod]
        public void IsEncryptorFile_Sucess()
        {
            var filepath = WriteTestFileToRandomLocation("Picture.jpg.enc");

            var decrypt = Engine.Encryptor.IsEncryptorFile(filepath);

            Assert.IsTrue(decrypt, "Is this a EncryptorFile");
        }

        [TestMethod]
        public void IsEncryptorFile_Fail()
        {
            var filepath = WriteTestFileToRandomLocation("Picture.jpg");

            var decrypt = Engine.Encryptor.IsEncryptorFile(filepath);

            Assert.IsFalse(decrypt, "Is this a EncryptorFile");
        }

        private static string WriteTestFileToRandomLocation(string name)
        {
            var data = Helper.GetResourceBin(Assembly.GetExecutingAssembly(), name);
            var filepath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            File.WriteAllBytes(filepath, data);
            return filepath;
        }
    }

    public class Helper
    {
        public static byte[] GetResourceBin(Assembly executingAssembly, string fileName)
        {
            var name = executingAssembly.GetManifestResourceNames().Single(x => x.EndsWith(fileName));
            using (Stream stream = executingAssembly.GetManifestResourceStream(name))
            {
                return ReadFully(stream);
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16*1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}