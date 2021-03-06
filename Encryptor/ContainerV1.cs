﻿using System;
using System.IO;
using System.Linq;

namespace Encryptor.Engine
{
    public class ContainerV1
    {
        private const int MagicBytesLength = 8;
        private const int VersionLength = 4;
        private const int HmacLength = 64;
        private const int IVLength = 16;

        public static int ContainerVersion => 1;

        public byte[] Data { get; }

        public byte[] IV { get; }

        public byte[] Hmac { get; }

        public ContainerV1(byte[] iv, byte[] encryptedBytes, byte[] hmac)
        {
            this.IV = iv;
            this.Data = encryptedBytes;
            this.Hmac = hmac;
        }

        /// <summary>
        /// 1. Magic number
        /// 2. Container version
        /// 3. Hmac
        /// 4. IV
        /// 5. Data (see )
        ///     1. Lenth FileName
        ///     2. FileName
        ///     3. File data
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            var length = Config.MagicBytes.Length + BitConverter.GetBytes(ContainerVersion).Length + this.Hmac.Length +
                         this.IV.Length + this.Data.Length;

            var memoryStream = new MemoryStream(length);
            var binaryWriter = new BinaryWriter(memoryStream);

            binaryWriter.Write(Config.MagicBytes);
            binaryWriter.Write(BitConverter.GetBytes(ContainerVersion));
            binaryWriter.Write(this.Hmac);
            binaryWriter.Write(this.IV);
            binaryWriter.Write(this.Data);

            return memoryStream.ToArray();
        }

        public static ContainerV1 FromBytes(byte[] bytes)
        {
            var magicBytes = bytes.Skip(0).Take(MagicBytesLength).ToArray();
            if (!Config.MagicBytes.SequenceEqual(magicBytes))
                throw new Exception("MagicNumber is wrong.");

            var version = BitConverter.ToInt32(bytes.Skip(MagicBytesLength).Take(VersionLength).ToArray(), 0);
            if (version != ContainerVersion)
                throw new Exception("ContainerVersion is wrong.");

            var hmac = bytes.Skip(MagicBytesLength + VersionLength).Take(HmacLength).ToArray();
            var iv = bytes.Skip(MagicBytesLength + VersionLength + HmacLength).Take(IVLength).ToArray();
            var data = bytes.Skip(MagicBytesLength + VersionLength + HmacLength + IVLength).ToArray();
            var result = new ContainerV1(iv, data, hmac);

            return result;
        }
    }
}