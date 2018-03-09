using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Math.P2T
{
    public struct TriPoint
    {
        public double X;

        public double Y;

        public List<Edge> EdgeList;

        public TriPoint(double x, double y)
        {
            X = x;
            Y = y;
            EdgeList = new List<Edge>();
        }

        public void set_zero()
        {
            X = 0;
            Y = 0;
        }

        public void Set(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Negate this point and return a new instance of it.
        /// </summary>
        /// <returns></returns>
        public TriPoint GetNegated()
        {
            return new TriPoint(-X, -Y);
        }

        /// <summary>
        /// Add a point to this point.
        /// </summary>
        /// <param name="v"></param>
        public void Add(TriPoint v)
        {
            X += v.X;
            Y += v.Y;
        }

        /// <summary>
        /// Subtract a point from this point.
        /// </summary>
        /// <param name="v"></param>
        public void Subtract(TriPoint v)
        {
            X -= v.X;
            Y -= v.Y;
        }

        /// <summary>
        /// Multiply this point by a scalar.
        /// </summary>
        /// <param name="a"></param>
        public void Multiply(double a)
        {
            X *= a;
            Y *= a;
        }

        /// <summary>
        /// Get the length of this point (the norm).
        /// </summary>
        /// <returns></returns>
        public double Length()
        {
            return System.Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// Convert this point into a unit point (normalizes it). Returns the Length.
        /// </summary>
        /// <returns></returns>
        public double Normalize()
        {
            double len = Length();
            X /= len;
            Y /= len;
            return len;
        }

        public static TriPoint operator +(TriPoint a, TriPoint b)
        {
            return new TriPoint(a.X + b.X, a.Y + b.Y);
        }

        public static TriPoint operator -(TriPoint a, TriPoint b)
        {
            return new TriPoint(a.X - b.X, a.Y - b.Y);
        }

        public static TriPoint operator -(TriPoint a, double s)
        {
            return new TriPoint(a.X * s, a.Y * s);
        }

        public static bool operator ==(TriPoint a, TriPoint b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(TriPoint a, TriPoint b)
        {
            // Original: return !(a.x == b.x) && !(a.y == b.y); ??????
            return a.X != b.X || a.Y != b.Y;
        }

        public static double Dot(TriPoint a, TriPoint b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static double Cross(TriPoint a, TriPoint b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static TriPoint Cross(TriPoint a, double s)
        {
            return new TriPoint(s * a.Y, -s * a.X);
        }

        public static TriPoint Cross(double s, TriPoint a)
        {
            return new TriPoint(-s * a.Y, s * a.X);
        }

        public class Comparer : IComparer<TriPoint>
        {
            public int Compare(TriPoint a, TriPoint b)
            {
                if (a.Y < b.Y)
                {
                    return 1;
                }
                else if (a.Y == b.Y)
                {
                    if (a.X < b.X)
                        return 1;
                }

                return 0;
            }
        }
    }
}
