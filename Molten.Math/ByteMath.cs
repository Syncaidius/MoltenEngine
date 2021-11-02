using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten
{
    /// <summary>A helper class for converting between different computational storage measurements (e.g. bits, bytes, kilobytes, etc).</summary>
    public static class ByteMath
    {
        public const long BYTES_IN_KiB = 1024;
        public const long BYTES_IN_MiB = BYTES_IN_KiB * 1024;
        public const long BYTES_IN_GiB = BYTES_IN_MiB * 1024;
        public const long BYTES_IN_TiB = BYTES_IN_GiB * 1024;
        public const long BYTES_IN_PiB = BYTES_IN_TiB * 1024;

#if WINDOWS
        const long BYTE_BASE = 1024;
#else
        const long BYTE_BASE = 1000;
#endif

        public const long BYTES_IN_KB = BYTE_BASE;
        public const long BYTES_IN_MB = BYTES_IN_KB * BYTE_BASE;
        public const long BYTES_IN_GB = BYTES_IN_MB * BYTE_BASE;
        public const long BYTES_IN_TB = BYTES_IN_GB * BYTE_BASE;
        public const long BYTES_IN_PB = BYTES_IN_TB * BYTE_BASE;

        /// <summary>Converts bytes into kilobytes (KB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToKilobytes(long bytes)
        {
            double val = (float)bytes;
            val /= BYTES_IN_KB;

            return val;
        }

        /// <summary>Converts bytes into megabytes (MB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToMegabytes(ulong bytes)
        {
            decimal val = (decimal)bytes;
            val /= BYTES_IN_MB;

            return (double)val;
        }

        /// <summary>Converts bytes into megabytes (MB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToMegabytes(long bytes)
        {
            double val = bytes;
            val /= BYTES_IN_MB;

            return val;
        }

        /// <summary>Converts bytes into gigbytes (GB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToGigabytes(ulong bytes)
        {
            decimal val = (decimal)bytes;
            val /= BYTES_IN_GB;

            return (double)val;
        }

        /// <summary>Converts bytes into gigbytes (GB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToGigabytes(long bytes)
        {
            double val = (float)bytes;
            val /= BYTES_IN_GB;

            return val;
        }

        /// <summary>Converts bytes into terabytes (TB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToTerabytes(long bytes)
        {
            double val = (float)bytes;
            val /= BYTES_IN_TB;

            return val;
        }

        /// <summary>Converts bytes into petabytes (PB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToPetabytes(long bytes)
        {
            double val = (float)bytes;
            val /= BYTES_IN_PB;

            return val;
        }

        /// <summary>Converts kilobytes into bytes. </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static long FromKilobytes(double kilobytes)
        {
            kilobytes *= BYTES_IN_KB;
            return (long)kilobytes;
        }

        /// <summary>Converts megabytes into bytes. </summary>
        /// <param name="megabytes"></param>
        /// <returns></returns>
        public static long FromMegabytes(double megabytes)
        {
            megabytes *= BYTES_IN_MB;

            return (long)megabytes;
        }

        /// <summary>Converts gigabytes into bytes. </summary>
        /// <param name="gigabytes"></param>
        /// <returns></returns>
        public static long FromGigabytes(double gigabytes)
        {
            gigabytes *= BYTES_IN_GB;

            return (long)gigabytes;
        }

        /// <summary>Converts terabytes into bytes. </summary>
        /// <param name="petabytes"></param>
        /// <returns></returns>
        public static long FromTerabytes(double terabytes)
        {
            terabytes *= BYTES_IN_TB;


            return (long)terabytes;
        }

        /// <summary>Converts petabytes into bytes. </summary>
        /// <param name="terabytes"></param>
        /// <returns></returns>
        public static long FromPetabytes(double petabytes)
        {
            petabytes *= BYTES_IN_PB;

            return (long)petabytes;
        }

        /// <summary>Converts bytes into bits.</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static long ToBits(long bytes)
        {
            bytes *= 8;

            return bytes;
        }

        /// <summary>Converts bits into bytes.</summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static long FromBits(long bits)
        {
            bits /= 8;

            return bits;
        }
    }
}
