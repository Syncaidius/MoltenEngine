using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public unsafe class MsdfMath
    {
        public const double M_PI = 3.14159265358979323846;
        /// <summary>
        /// Returns the smaller of the arguments.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Min(double a, double b)
        {
            return b < a ? b : a;
        }

        /// <summary>
        /// Returns the smaller of the arguments.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Min(float a, float b)
        {
            return b < a ? b : a;
        }

        /// <summary>
        /// Returns the larger of the arguments.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Max(double a, double b)
        {
            return a < b ? b : a;
        }

        /// <summary>
        /// Returns the larger of the arguments.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Max(float a, float b)
        {
            return a < b ? b : a;
        }

        /// <summary>
        /// Returns the middle out of three values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static double Median(double a, double b, double c)
        {
            return Max(Min(a, b), Min(Max(a, b), c));
        }

        /// <summary>
        /// Returns the middle out of three values
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static float Median(float a, float b, float c)
        {
            return Max(Min(a, b), Min(Max(a, b), c));
        }

        /// Returns the weighted average of a and b. MathHelper.Lerp equivilent
        public static double Mix(double a, double b, double weight)
        {
            return (1.0 - weight) * a + weight * b;
        }

        /// Returns the weighted average of a and b. MathHelper.Lerp equivilent
        public static Vector2D Mix(Vector2D a, Vector2D b, double weight)
        {
            return (1.0 - weight) * a + weight * b;
        }

        /// Returns the weighted average of a and b.
        public static double Mix(double a, double b, float weight)
        {
            return (1.0 - weight) * a + weight * b;
        }

        /// Returns the weighted average of a and b.
        public static float Mix(float a, float b, double weight)
        {
            return (float)((1.0f - weight) * a + weight * b);
        }

        /// Returns the weighted average of a and b.
        public static float Mix(float a, float b, float weight)
        {
            return (float)(1.0 - weight) * a + weight * b;
        }

        /// Clamps the number to the interval from 0 to 1.
        public static float Clamp(float n)
        {
            return n >= 0f && n <= 1.0f ? n : (n > 0 ? 1f : 0f);
        }

        /// Clamps the number to the interval from 0 to 1.
        public static double Clamp(double n)
        {
            return n >= 0 && n <= 1.0 ? n : (n > 0 ? 1 : 0);
        }

        /// Clamps the number to the interval from 0 to b.
        public static float Clamp(float n, float b)
        {
            return n >= 0f && n <= b ? n : (n > 0 ? 1f : 0f) * b;
        }

        /// Clamps the number to the interval from a to b.
        public static double Clamp(double n, double a, double b)
        {
            return n >= a && n <= b ? n : n < a ? a : b;
        }

        /// Returns 1 for positive values, -1 for negative values, and 0 for zero.
        public static int Sign(double n)
        {
            int greaterThanZero = 0.0 < n ? 1 : 0;
            int lessThanZero = n < 0.0 ? 1 : 0;
            return (greaterThanZero - lessThanZero);
        }

        /// Returns 1 for non-negative values and -1 for negative values.
        public static int NonZeroSign(double n)
        {
            int greaterThanZero = (n > 0) ? 1 : 0;
            return 2 * greaterThanZero - 1;
        }
    }
}
