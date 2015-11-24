using System;
using System.IO;

namespace Encryptor.FileOperator
{
    public class Zip
    {
        public static bool CreateZipArchiv(string[] files, string zipArchiv)
        {
            var zipFile = new Ionic.Zip.ZipFile(zipArchiv);

            foreach (var file in files)
            {
                if (File.GetAttributes(file).HasFlag(FileAttributes.Directory))
                {
                    zipFile.AddDirectory(file,new DirectoryInfo(file).Name);
                }
                else
                {
                    zipFile.AddFile(file,String.Empty);
                }
                zipFile.Save();
            }
            return true;
        }
    }
}