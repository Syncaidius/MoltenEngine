namespace Molten
{
    /// <summary>A helper class for converting between different computational storage measurements (e.g. bits, bytes, kilobytes, etc).</summary>
    public static class ByteMath
    {
        public const ulong BITS_IN_BYTE = 8;

#if X64 || X86 || WIN32 || WINDOWS || WIN || WIN64 || WINDOWS
        public const ulong B_BASE = 1024;
#else
        public const ulong B_BASE = 1000;
#endif

        public const ulong BYTES_IN_KB = B_BASE;
        public const ulong BYTES_IN_MB = BYTES_IN_KB * B_BASE;
        public const ulong BYTES_IN_GB = BYTES_IN_MB * B_BASE;
        public const ulong BYTES_IN_TB = BYTES_IN_GB * B_BASE;
        public const ulong BYTES_IN_PB = BYTES_IN_TB * B_BASE;

        public const ulong IB_BASE = 1024;
        public const ulong BYTES_IN_KIB = IB_BASE;
        public const ulong BYTES_IN_MIB = BYTES_IN_KB * IB_BASE;
        public const ulong BYTES_IN_GIB = BYTES_IN_MB * IB_BASE;
        public const ulong BYTES_IN_TIB = BYTES_IN_GB * IB_BASE;
        public const ulong BYTES_IN_PIB = BYTES_IN_TB * IB_BASE;

        /// <summary>Converts bytes into kilobytes (KB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToKilobytes(long b)
        {
            return (double)b / BYTES_IN_KB;
        }

        /// <summary>Converts bytes into megabytes (MB).</summary>
        /// <param name="b">The number of bytes.</param>
        /// <returns></returns>
        public static double ToMegabytes(ulong b)
        {
            return (double)b / BYTES_IN_MB;
        }

        /// <summary>Converts bytes into megabytes (MB).</summary>
        /// <param name="b">The number of bytes.</param>
        /// <returns></returns>
        public static double ToMegabytes(long b)
        {
            return (double)b / BYTES_IN_MB;
        }

        /// <summary>Converts bytes into gigbytes (GB).</summary>
        /// <param name="b">The number of bytes.</param>
        /// <returns></returns>
        public static double ToGigabytes(ulong b)
        {
            return (double)b / BYTES_IN_GB;
        }

        /// <summary>Converts bytes into gigbytes (GB).</summary>
        /// <param name="b">The number of bytes.</param>
        /// <returns></returns>
        public static double ToGigabytes(long b)
        {
            return (double)b / BYTES_IN_GB;
        }

        /// <summary>Converts bytes into terabytes (TB).</summary>
        /// <param name="b">The number of bytes.</param>
        /// <returns></returns>
        public static double ToTerabytes(long b)
        {
            return (double)b / BYTES_IN_TB;
        }

        /// <summary>Converts bytes into petabytes (PB).</summary>
        /// <param name="b">The number of bytes.</param>
        /// <returns></returns>
        public static double ToPetabytes(long b)
        {
            return (double)b / BYTES_IN_PB;
        }

        /// <summary>Converts kilobytes into bytes. </summary>
        /// <param name="kb">The number of kilobytes.</param>
        /// <returns></returns>
        public static long FromKilobytes(double kb)
        {
            return (long)(kb * BYTES_IN_KB);
        }

        /// <summary>Converts megabytes into bytes. </summary>
        /// <param name="mb">The number of megabytes.</param>
        /// <returns></returns>
        public static long FromMegabytes(double mb)
        {
            return (long)(mb * BYTES_IN_MB);
        }

        /// <summary>Converts gigabytes into bytes.</summary>
        /// <param name="gb">The number of gigabytes.</param>
        /// <returns></returns>
        public static long FromGigabytes(double gb)
        {
            return (long)(gb * BYTES_IN_GB);
        }

        /// <summary>Converts terabytes into bytes. </summary>
        /// <param name="tb">The number of terabytes.</param>
        /// <returns></returns>
        public static long FromTerabytes(double tb)
        {
            return (long)(tb * BYTES_IN_TB);
        }

        /// <summary>Converts petabytes into bytes. </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        public static long FromPetabytes(double pb)
        {
            return (long)(pb * BYTES_IN_PB);
        }

        /// <summary>Converts bytes into bits.</summary>
        /// <param name="b">The number of bytes</param>
        /// <returns></returns>
        public static ulong ToBits(ulong b)
        {
            return b * BITS_IN_BYTE;
        }

        /// <summary>Converts bytes into bits.</summary>
        /// <param name="b">The number of bytes</param>
        /// <returns></returns>
        public static ulong ToBits(long b)
        {
            return (ulong)b * BITS_IN_BYTE;
        }

        /// <summary>Converts bits into bytes.</summary>
        /// <param name="bits">The number of bits.</param>
        /// <returns></returns>
        public static long FromBits(long bits)
        {
            return bits / (long)BITS_IN_BYTE;
        }

        /// <summary>Converts bits into bytes.</summary>
        /// <param name="bits">The number of bits.</param>
        /// <returns></returns>
        public static ulong FromBits(ulong bits)
        {
            return bits / BITS_IN_BYTE;
        }
    }
}
