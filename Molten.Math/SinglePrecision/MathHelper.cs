// MIT, 2018 - 2022 - James Yarwood - Molten Engine

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Molten
{
    /// <summary>
    /// Single-precision math helper class.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// The value for which all absolute numbers smaller than are considered equal to zero.
        /// </summary>
        public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

        /// <summary>
        /// A value specifying the approximation of half-π which is 90 degrees.
        /// </summary>
        public const float PiHalf = (float)(Math.PI / 2.0);

        /// <summary>
        /// A value specifying the approximation of π/2 which is 90 degrees.
        /// </summary>
        public const float PiOverTwo = float.Pi / 2f;

        /// <summary>
        /// A value specifying the approximation of π/4 which is 45 degrees.
        /// </summary>
        public const float PiOverFour = float.Pi / 4f;

        /// <summary>
        /// Multiply by this value to convert from degrees to radians.
        /// </summary>
        public const float DegToRad = float.Pi / 180.0f;

        /// <summary>
        /// Multiply by this value to convert from radians to degrees.
        /// </summary>
        public const float RadToDeg = 180.0f / float.Pi;

        /// <summary>
        /// Multiply by this value to convert from gradians to radians.
        /// </summary>
        public const float GradToRad = float.Pi / 200.0f;

        /// <summary>
        /// Multiply by this value to convert from gradians to degrees.
        /// </summary>
        public const float GradToDeg = 9.0f / 10.0f;
        /// <summary>
        /// Multiply by this value to convert from radians to gradians.
        /// </summary>
        public const float RadToGrad = 200.0f / float.Pi;

        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike <see cref="WithinEpsilon"/> method). See Remarks.
        /// See remarks.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <returns><c>true</c> if a almost equal to b, <c>false</c> otherwise</returns>
        /// <remarks>
        /// The code is using the technique described by Bruce Dawson in 
        /// <a href="http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/">Comparing Floating point numbers 2012 edition</a>. 
        /// </remarks>
        public unsafe static bool NearEqual(float a, float b)
        {
            // Check if the numbers are really close -- needed
            // when comparing numbers near zero.
            if (IsZero(a - b))
                return true;

            // Original from Bruce Dawson: http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            // Different signs means they do not match.
            if ((aInt < 0) != (bInt < 0))
                return false;

            // Find the difference in ULPs.
            int ulp = Math.Abs(aInt - bInt);

            // Choose of maxUlp = 4
            // according to http://code.google.com/p/googletest/source/browse/trunk/include/gtest/internal/gtest-internal.h
            const int maxUlp = 4;
            return (ulp <= maxUlp);
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        public static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

        /// <summary>
        /// Determines whether the specified value is close to one (1.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to one (1.0f); otherwise, <c>false</c>.</returns>
        public static bool IsOne(float a)
        {
            return IsZero(a - 1.0f);
        }

        /// <summary>
        /// Checks if a - b are almost equals within a float epsilon.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <param name="epsilon">Epsilon value</param>
        /// <returns><c>true</c> if a almost equal to b within a float epsilon, <c>false</c> otherwise</returns>
        public static bool WithinEpsilon<T>(T a, T b, T epsilon)
            where T : IFloatingPoint<T>
        {
            T num = a - b;
            return ((-epsilon <= num) && (num <= epsilon));
        }

        /// <summary>
        /// Converts revolutions to degrees.
        /// </summary>
        /// <param name="revolution">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RevolutionsToDegrees(float revolution)
        {
            return revolution * 360.0f;
        }

        /// <summary>
        /// Converts revolutions to radians.
        /// </summary>
        /// <param name="revolution">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RevolutionsToRadians(float revolution)
        {
            return revolution * float.Tau;
        }

        /// <summary>
        /// Converts revolutions to gradians.
        /// </summary>
        /// <param name="revolution">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RevolutionsToGradians(float revolution)
        {
            return revolution * 400.0f;
        }

        /// <summary>
        /// Converts degrees to revolutions.
        /// </summary>
        /// <param name="degree">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float DegreesToRevolutions(float degree)
        {
            return degree / 360.0f;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degree">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float DegreesToRadians(float degree)
        {
            return degree * DegToRad;
        }

        /// <summary>
        /// Converts radians to revolutions.
        /// </summary>
        /// <param name="radian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RadiansToRevolutions(float radian)
        {
            return radian / float.Tau;
        }

        /// <summary>
        /// Converts radians to gradians.
        /// </summary>
        /// <param name="radian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RadiansToGradians(float radian)
        {
            return radian * RadToGrad;
        }

        /// <summary>
        /// Converts gradians to revolutions.
        /// </summary>
        /// <param name="gradian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float GradiansToRevolutions(float gradian)
        {
            return gradian / 400.0f;
        }

        /// <summary>
        /// Converts gradians to degrees.
        /// </summary>
        /// <param name="gradian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float GradiansToDegrees(float gradian)
        {
            return gradian * GradToDeg;
        }

        /// <summary>
        /// Converts gradians to radians.
        /// </summary>
        /// <param name="gradian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float GradiansToRadians(float gradian)
        {
            return gradian * GradToRad;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radian">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static float RadiansToDegrees(float radian)
        {
            return radian * RadToDeg;
        }

        /// <summary>
        /// Clamps the specified value between 0 and 1.0f
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of clamping a value between 0 and 1.0f</returns>
        public static T Clamp<T>(T value)
            where T : IFloatingPoint<T>
        {
            return value < T.Zero ? T.Zero : value > T.One ? T.One : value;
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        public static float Lerp(float from, float to, double amount)
        {
            return (float)((1D - amount) * from + amount * to);
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Lerp<T>(T from, T to, T amount)
            where T : struct, IFloatingPoint<T>
        {
            return (T.One - amount) * from + amount * to;
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Lerp(byte from, byte to, float amount)
        {
            return (byte)Lerp(from, (float)to, amount);
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Lerp(int from, int to, float amount)
        {
            return (int)Lerp(from, (float)to, amount);
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Lerp(uint from, uint to, float amount)
        {
            return (uint)Lerp(from, (float)to, amount);
        }

        /// <summary>
        /// Performs smooth (cubic Hermite) interpolation between 0 and 1.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStep(float amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * (3 - (2 * amount));
        }


        /// <summary>
        /// Performs a smooth(er) interpolation between 0 and 1 with 1st and 2nd order derivatives of zero at endpoints.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static float SmootherStep(float amount)
        {
            return (amount <= 0) ? 0
                : (amount >= 1) ? 1
                : amount * amount * amount * (amount * ((amount * 6) - 15) + 10);
        }

        /// <summary>
        /// Calculates the modulo of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="modulo">The modulo.</param>
        /// <returns>The result of the modulo applied to value</returns>
        public static T Mod<T>(T value, T modulo)
            where T : IFloatingPoint<T>
        {
            return modulo == T.Zero ? value : value % modulo;
        }

        /// <summary>
        /// Calculates the modulo 2*PI of the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the modulo applied to value</returns>
        public static T Mod2PI<T>(T value)
            where T : IFloatingPoint<T>
        {
            return Mod(value, T.Tau);
        }

        /// <summary>
        /// Wraps the specified integer value into a range [min, max]
        /// </summary>
        /// <param name="value">The integer value to wrap.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>Result of the wrapping.</returns>
        /// <exception cref="ArgumentException">Is thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
        public static T WrapI<T>(T value, T min, T max)
            where T : IBinaryInteger<T>
        {
            if (min > max)
                throw new ArgumentException(string.Format("min {0} should be less than or equal to max {1}", min, max), "min");

            // Code from http://stackoverflow.com/a/707426/1356325
            T range_size = max - min + T.One;

            if (value < min)
                value += range_size * ((min - value) / range_size + T.One);

            return min + (value - min) % range_size;
        }

        /// <summary>
        /// Wraps the specified value into a range [min, max[
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        /// <returns>Result of the wrapping.</returns>
        /// <exception cref="ArgumentException">Is thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
        public static float Wrap(float value, float min, float max)
        {
            if (NearEqual(min, max))
                return min;

            double mind = min;
            double maxd = max;
            double valued = value;

            if (mind > maxd)
                throw new ArgumentException(string.Format("min {0} should be less than or equal to max {1}", min, max), "min");

            var range_size = maxd - mind;
            return (float)(mind + (valued - mind) - (range_size * Math.Floor((valued - mind) / range_size)));
        }

        /// <summary>
        /// Reduces the angle into a range from -Pi to Pi.
        /// </summary>
        /// <param name="angle">Angle to wrap.</param>
        /// <returns>Wrapped angle.</returns>
        public static T WrapAngle<T>(T angle)
            where T : IFloatingPointIeee754<T>
        {
            angle = T.Ieee754Remainder(angle, T.Tau);
            if (angle < -T.Pi)
                angle += T.Tau;
            else if (angle >= T.Pi)
                angle -= T.Tau;

            return angle;
        }

        /// <summary>
        /// Gauss function.
        /// http://en.wikipedia.org/wiki/Gaussian_function#Two-dimensional_Gaussian_function
        /// </summary>
        /// <param name="amplitude">Curve amplitude.</param>
        /// <param name="x">Position X.</param>
        /// <param name="y">Position Y</param>
        /// <param name="centerX">Center X.</param>
        /// <param name="centerY">Center Y.</param>
        /// <param name="sigmaX">Curve sigma X.</param>
        /// <param name="sigmaY">Curve sigma Y.</param>
        /// <returns>The result of Gaussian function.</returns>
        public static float Gauss(float amplitude, float x, float y, float centerX, float centerY, float sigmaX, float sigmaY)
        {
            float cx = x - centerX;
            float cy = y - centerY;

            float componentX = (cx * cx) / (2 * sigmaX * sigmaX);
            float componentY = (cy * cy) / (2 * sigmaY * sigmaY);

            return (amplitude * MathF.Exp(-(componentX + componentY)));
        }

        /// <summary>Rounds down to the nearest X value.</summary>
        /// <param name="value">The value.</param>
        /// <param name="x">The x (nearest to round to).</param>
        /// <returns></returns>
        public static T RoundToNearest<T>(T value, T x)
            where T : IFloatingPoint<T>
        {
            T diff = value % x;
            return value - diff;
        }

        /// <summary>
        ///  Returns 1 for non-negative values and -1 for negative values.
        /// </summary>
        /// <param name="value">The value</param>
        public static int NonZeroSign<T>(T value)
            where T : ISignedNumber<T>, INumber<T>
        {
            return 2 * ((value > T.Zero) ? 1 : 0) - 1;
        }

        /// <summary>
        /// Returns the largest value out of all of the provided values.
        /// </summary>
        /// <param name="a">The first value to be compared.</param>
        /// <param name="b">The second value to be compared.</param>
        /// <param name="others">The other values to be compared.</param>
        /// <returns></returns>
        public static T Max<T>(T a, T b, params T[] others)
            where T : struct, INumber<T>
        {
            T min = T.Max(a, b);
            for (int i = 0; i < others.Length; i++)
                min = T.Max(min, others[i]);

            return min;
        }

        /// <summary>
        /// Returns the smallest value out of all of the provided values.
        /// </summary>
        /// <param name="a">The first value to be compared.</param>
        /// <param name="b">The second value to be compared.</param>
        /// <param name="others">The other values to be compared.</param>
        /// <returns></returns>
        public static T Min<T>(T a, T b, params T[] others)
            where T : struct, INumber<T>
        {
            T min = T.Min(a,b);
            for (int i = 0; i < others.Length; i++)
                min = T.Min(min, others[i]);

            return min;
        }

        /// <summary>
        /// Returns the middle out of three values
        /// </summary>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <param name="c">Third value.</param>
        /// <returns></returns>
        public static T Median<T>(T a, T b, T c)
            where T : struct, INumber<T>
        {
            return T.Max(T.Min(a, b), T.Min(T.Max(a, b), c));
        }
    }
}
