using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten
{
    /// <summary>A helper class for converting between different computational storage measurements (e.g. bits, bytes, kilobytes, etc).</summary>
    public static class ByteMath
    {
        /// <summary>Converts bytes into kilobytes (KB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToKilobytes(long bytes)
        {
            double val = (float)bytes;
            val /= 1024;

            return val;
        }

        /// <summary>Converts bytes into megabytes (MB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToMegabytes(ulong bytes)
        {
            decimal val = (decimal)bytes;
            val /= 1024;
            val /= 1024;

            return (double)val;
        }

        /// <summary>Converts bytes into megabytes (MB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToMegabytes(long bytes)
        {
            double val = (float)bytes;
            val /= 1024;
            val /= 1024;

            return val;
        }

        /// <summary>Converts bytes into gigbytes (GB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToGigabytes(ulong bytes)
        {
            decimal val = (decimal)bytes;
            val /= 1024; //to kilobytes
            val /= 1024; //to megabytes
            val /= 1024; //to gigabytes

            return (double)val;
        }

        /// <summary>Converts bytes into gigbytes (GB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToGigabytes(long bytes)
        {
            double val = (float)bytes;
            val /= 1024; //to kilobytes
            val /= 1024; //to megabytes
            val /= 1024; //to gigabytes

            return val;
        }

        /// <summary>Converts bytes into terabytes (TB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToTerabytes(long bytes)
        {
            double val = (float)bytes;
            val /= 1024; //to kilobytes
            val /= 1024; //to megabytes
            val /= 1024; //to gigabytes
            val /= 1024; //to terabytes

            return val;
        }

        /// <summary>Converts bytes into petabytes (PB).</summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static double ToPetabytes(long bytes)
        {
            double val = (float)bytes;
            val /= 1024; //to kilobytes
            val /= 1024; //to megabytes
            val /= 1024; //to gigabytes
            val /= 1024; //to terabytes
            val /= 1024; //to petabytes

            return val;
        }

        /// <summary>Converts kilobytes into bytes. </summary>
        /// <param name="kilobytes"></param>
        /// <returns></returns>
        public static long FromKilobytes(double kilobytes)
        {
            kilobytes *= 1024; //to B
            return (long)kilobytes;
        }

        /// <summary>Converts megabytes into bytes. </summary>
        /// <param name="megabytes"></param>
        /// <returns></returns>
        public static long FromMegabytes(double megabytes)
        {
            megabytes *= 1024; //to KB
            megabytes *= 1024; //to B

            return (long)megabytes;
        }

        /// <summary>Converts gigabytes into bytes. </summary>
        /// <param name="gigabytes"></param>
        /// <returns></returns>
        public static long FromGigabytes(double gigabytes)
        {
            gigabytes *= 1024; //to MB
            gigabytes *= 1024; //to KB
            gigabytes *= 1024; //to B

            return (long)gigabytes;
        }

        /// <summary>Converts terabytes into bytes. </summary>
        /// <param name="petabytes"></param>
        /// <returns></returns>
        public static long FromTerabytes(double terabytes)
        {
            terabytes *= 1024; //to gigabytes
            terabytes *= 1024; //to megabytes
            terabytes *= 1024; //to KB
            terabytes *= 1024; //to B


            return (long)terabytes;
        }

        /// <summary>Converts petabytes into bytes. </summary>
        /// <param name="terabytes"></param>
        /// <returns></returns>
        public static long FromPetabytes(double petabytes)
        {
            petabytes *= 1024; //to terabytes
            petabytes *= 1024; //to gigabytes
            petabytes *= 1024; //to megabytes
            petabytes *= 1024; //to kilobytes
            petabytes *= 1024; //to bytes


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
