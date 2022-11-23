// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using System.Numerics;
using Molten.DoublePrecision;

namespace Molten
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class RandomNumber
    {
        //
        // Private Constants 
        //
        private const long MBIG = Int64.MaxValue;
        private const long MSEED = 161803398;
        private const long MZ = 0;


        //
        // Member Variables
        //
        private long inext;
        private long inextp;
        private long[] SeedArray = new long[56];

        //
        // Constructors
        //

        public RandomNumber()
          : this(Environment.TickCount)
        {
        }

        public RandomNumber(long seed)
        {
            long ii;
            long mj, mk;

            //Initialize our Seed array.
            //This algorithm comes from Numerical Recipes in C (2nd Ed.)
            long subtraction = (seed == Int64.MinValue) ? Int64.MaxValue : Math.Abs(seed);
            mj = MSEED - subtraction;
            SeedArray[55] = mj;
            mk = 1;
            for (int i = 1; i < 55; i++)
            {  //Apparently the range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                ii = (21 * i) % 55;
                SeedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += MBIG;
                mj = SeedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    SeedArray[i] -= SeedArray[1 + (i + 30) % 55];
                    if (SeedArray[i] < 0) SeedArray[i] += MBIG;
                }
            }
            inext = 0;
            inextp = 21;
            seed = 1;
        }

        //
        // Package Private Methods
        //

        /*====================================Sample====================================
        **Action: Return a new random number [0..1) and reSeed the Seed array.
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        protected virtual decimal Sample()
        {
            //Including this division at the end gives us significantly improved
            //random number distribution.
            return (InternalSample() * (1.0m / MBIG));
        }

        private long InternalSample()
        {
            long retVal;
            long locINext = inext;
            long locINextp = inextp;

            if (++locINext >= 56) locINext = 1;
            if (++locINextp >= 56) locINextp = 1;

            retVal = SeedArray[locINext] - SeedArray[locINextp];

            if (retVal == MBIG) retVal--;
            if (retVal < 0) retVal += MBIG;

            SeedArray[locINext] = retVal;

            inext = locINext;
            inextp = locINextp;

            return retVal;
        }

        //
        // Public Instance Methods
        // 


        /// <summary>Returns a random number between 0 and Int64.MaxValue</summary>
        /// <returns>A long in the range of 0..Int64.MaxValue.</returns>
        public virtual long Next()
        {
            return InternalSample();
        }

        private double GetSampleForLargeRange()
        {
            // The distribution of double value returned by Sample 
            // is not distributed well enough for a large range.
            // If we use Sample for a range [Int32.MinValue..Int32.MaxValue)
            // We will end up getting even numbers only.

            long result = InternalSample();
            // Note we can't use addition here. The distribution will be bad if we do that.
            bool negative = (InternalSample() % 2 == 0) ? true : false;  // decide the sign based on second sample
            if (negative)
            {
                result = -result;
            }
            double d = result;
            d += (Int64.MaxValue - 1); // get a number in range [0 .. 2 * Int32MaxValue - 1)
            d /= 2 * (ulong)Int64.MaxValue - 1;
            return d;
        }


        /*=====================================Next=====================================
        **Returns: An long [minvalue..maxvalue)
        **Arguments: minValue -- the least legal value for the Random number.
        **           maxValue -- One greater than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual long Next(long minValue, long maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "Min cannot be greater than max.");
            }
            Contract.EndContractBlock();

            BigInteger bRange = (BigInteger)maxValue - minValue;

            if (bRange <= Int64.MaxValue)
            {
                long range = (long)bRange;
                return ((long)(Sample() * range) + minValue);
            }
            else
            {
                ulong range = (ulong)bRange;
                return (long)((BigInteger)(GetSampleForLargeRange() * range) + minValue);
            }
        }

        /*=====================================Next=====================================
        **Returns: An int [minvalue..maxvalue)
        **Arguments: minValue -- the least legal value for the Random number.
        **           maxValue -- One greater than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "Min cannot be greater than max.");
            }
            Contract.EndContractBlock();

            long range = (long)maxValue - minValue;
            if (range <= int.MaxValue)
            {
                return ((int)(Sample() * range) + minValue);
            }
            else
            {
                return (int)((long)(GetSampleForLargeRange() * range) + minValue);
            }
        }

        public virtual short Next(short minValue, short maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "Min cannot be greater than max.");
            }
            Contract.EndContractBlock();

            long range = (long)maxValue - minValue;
            if (range <= short.MaxValue)
            {
                return (short)((int)(Sample() * range) + minValue);
            }
            else
            {
                return (short)((long)(GetSampleForLargeRange() * range) + minValue);
            }
        }

        public virtual byte NextByte(byte minValue, byte maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", "Min cannot be greater than max.");
            }
            Contract.EndContractBlock();

            return (byte)Math.Ceiling(255 * NextDouble());
        }

        /*=====================================Next=====================================
        **Returns: An long [0..maxValue)
        **Arguments: maxValue -- One more than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual long Next(long maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "max cannot be less than 0");
            }
            Contract.EndContractBlock();
            return (long)(Sample() * maxValue);
        }


        /*=====================================Next=====================================
        **Returns: An int [0..maxValue)
        **Arguments: maxValue -- One more than the greatest legal return value.
        **Exceptions: None.
        ==============================================================================*/
        public virtual int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException("maxValue", "max cannot be less than 0");
            }
            Contract.EndContractBlock();
            return (int)(Sample() * maxValue);
        }


        /*=====================================Next=====================================
        **Returns: A double [0..1)
        **Arguments: None
        **Exceptions: None
        ==============================================================================*/
        public virtual decimal NextDecimal()
        {
            return Sample();
        }

        public double NextDouble()
        {
            return (double)Sample();
        }


        /*==================================NextBytes===================================
        **Action:  Fills the byte array with random bytes [0..0x7f].  The entire array is filled.
        **Returns:Void
        **Arugments:  buffer -- the array to be filled.
        **Exceptions: None
        ==============================================================================*/
        public virtual void NextBytes(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            Contract.EndContractBlock();
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(InternalSample() % (Byte.MaxValue + 1));
            }
        }

        /// <summary>
        /// Gets random <c>float</c> number within range.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>Random <c>float</c> number.</returns>
        public float NextFloat(float min, float max)
        {
            return MathHelper.Lerp(min, max, (float)NextDecimal());
        }

        /// <summary>
        /// Gets random <c>double</c> number within range.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>Random <c>double</c> number.</returns>
        public double NextDouble(double min, double max)
        {
            return MathHelper.Lerp(min, max, (double)NextDecimal());
        }

        /// <summary>
        /// Gets random <see cref="Vector2F"/> within range.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>Random <see cref="Vector2F"/>.</returns>
        public Vector2F NextVector2(Vector2F min, Vector2F max)
        {
            return new Vector2F(NextFloat(min.X, max.X), NextFloat(min.Y, max.Y));
        }

        /// <summary>
        /// Gets random <see cref="Vector3F"/> within range.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>Random <see cref="Vector3F"/>.</returns>
        public Vector3F NextVector3(Vector3F min, Vector3F max)
        {
            return new Vector3F(NextFloat(min.X, max.X), NextFloat(min.Y, max.Y), NextFloat(min.Z, max.Z));
        }

        /// <summary>
        /// Gets random <see cref="Vector4F"/> within range.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>Random <see cref="Vector4F"/>.</returns>
        public Vector4F NextVector4(Vector4F min, Vector4F max)
        {
            return new Vector4F(NextFloat(min.X, max.X), NextFloat(min.Y, max.Y), NextFloat(min.Z, max.Z), NextFloat(min.W, max.W));
        }

        /// <summary>
        /// Gets random opaque <see cref="Color"/>.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <returns>Random <see cref="Color"/>.</returns>
        public Color NextColor()
        {
            return new Color(NextFloat(0.0f, 1.0f), NextFloat(0.0f, 1.0f), NextFloat(0.0f, 1.0f), 1.0f);
        }

        /// <summary>
        /// Gets random opaque <see cref="Color"/>.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="minBrightness">Minimum brightness.</param>
        /// <param name="maxBrightness">Maximum brightness</param>
        /// <returns>Random <see cref="Color"/>.</returns>
        public Color NextColor(float minBrightness, float maxBrightness)
        {
            return new Color(NextFloat(minBrightness, maxBrightness), NextFloat(minBrightness, maxBrightness), NextFloat(minBrightness, maxBrightness), 1.0f);
        }

        /// <summary>
        /// Gets random <see cref="Color"/>.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>   
        /// <param name="minBrightness">Minimum brightness.</param>
        /// <param name="maxBrightness">Maximum brightness</param>
        /// <param name="alpha">Alpha value.</param>
        /// <returns>Random <see cref="Color"/>.</returns>
        public Color NextColor(float minBrightness, float maxBrightness, float alpha)
        {
            return new Color(NextFloat(minBrightness, maxBrightness), NextFloat(minBrightness, maxBrightness), NextFloat(minBrightness, maxBrightness), alpha);
        }

        /// <summary>
        /// Gets random <see cref="Color"/>.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="minBrightness">Minimum brightness.</param>
        /// <param name="maxBrightness">Maximum brightness</param>
        /// <param name="minAlpha">Minimum alpha.</param>
        /// <param name="maxAlpha">Maximum alpha.</param>
        /// <returns>Random <see cref="Color"/>.</returns>
        public Color NextColor(float minBrightness, float maxBrightness, float minAlpha, float maxAlpha)
        {
            return new Color(NextFloat(minBrightness, maxBrightness), NextFloat(minBrightness, maxBrightness), NextFloat(minBrightness, maxBrightness), NextFloat(minAlpha, maxAlpha));
        }

        /// <summary>
        /// Gets random <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>Random <see cref="Vector2I"/>.</returns>
        public Vector2I NextPoint(Vector2I min, Vector2I max)
        {
            return new Vector2I(Next(min.X, max.X), Next(min.Y, max.Y));
        }

        /// <summary>
        /// Gets random <see cref="System.TimeSpan"/>.
        /// </summary>
        /// <param name="random">Current <see cref="System.Random"/>.</param>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>Random <see cref="System.TimeSpan"/>.</returns>
        public TimeSpan NextTime( TimeSpan min, TimeSpan max)
        {
            return TimeSpan.FromTicks(Next(min.Ticks, max.Ticks));
        }
    }
}
