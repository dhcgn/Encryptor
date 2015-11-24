namespace Encryptor.Engine
{
    public static class Config
    {
        /// <summary>
        /// Hex 4C 85 AB 68 1D 39 B0
        /// </summary>
        public static byte[] MagicBytes => new[]
        {
            (byte) 76, (byte) 133, (byte) 171, (byte) 104,
            (byte) 29, (byte) 57, (byte) 176, (byte) 219,
        };

#if DEBUG
        public const int Iterations = 1000;
#else
        public const int Iterations = 500000;
#endif
    }
}