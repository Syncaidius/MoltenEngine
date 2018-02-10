//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)
//MIT, 2018, James Yarwood (Adapted for Molten Engine)

using System;

namespace Msdfgen
{
    public struct Vector2
    {
        public readonly double X;
        public readonly double Y;

        public Vector2(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static bool IsEqual(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public bool IsZero()
        {
            return X == 0 && Y == 0;
        }

        public Vector2 GetOrthoNormal(bool polarity = true, bool allowZero = false)
        {
            double len = Length();
            if (len == 0)
            {
                return polarity ? new Vector2(0, (!allowZero ? 1 : 0)) : new Vector2(0, -(!allowZero ? 1 : 0));
            }
            return polarity ? new Vector2(-Y / len, X / len) : new Vector2(Y / len, -X / len);
        }

        public Vector2 GetOrthogonal(bool polarity = true)
        {
            return polarity ? new Vector2(-Y, X) : new Vector2(Y, -X);
        }

        public static double Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static double Cross(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.X - b.X,
                a.Y - b.Y);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.X + b.X,
                a.Y + b.Y);
        }

        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.X * b.X,
                a.Y * b.Y);
        }

        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.X / b.X,
                a.Y / b.Y);
        }

        public static Vector2 operator *(Vector2 a, double n)
        {
            return new Vector2(
                a.X * n,
                a.Y * n);
        }

        public static Vector2 operator /(Vector2 a, double n)
        {
            return new Vector2(
                a.X / n,
                a.Y / n);
        }

        public static Vector2 operator *(double n, Vector2 a)
        {
            return new Vector2(
                a.X * n,
                a.Y * n);
        }

        public static Vector2 operator /(double n, Vector2 a)
        {
            return new Vector2(
                a.X / n,
                a.Y / n);
        }

        public Vector2 Normalize(bool allowZero = false)
        {
            double len = Length();
            if (len == 0)
            {
                return new Vector2(0, !allowZero ? 1 : 0);
            }
            return new Vector2(X / len, Y / len);
        }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public static double Shoelace(Vector2 a, Vector2 b)
        {
            return (b.X - a.X) * (a.Y + b.Y);
        }

        public override string ToString()
        {
            return X + "," + Y;
        }

        public static void PointBounds(Vector2 p, ref double l, ref double b, ref double r, ref double t)
        {
            if (p.X < l) l = p.X;
            if (p.Y < b) b = p.Y;
            if (p.X > r) r = p.X;
            if (p.Y > t) t = p.Y;
        }

    }
}
