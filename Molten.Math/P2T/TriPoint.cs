using Molten.DoublePrecision;

namespace Molten;

internal class TriPoint : IEquatable<TriPoint>
{
    /// <summary>
    /// An empty <see cref="TriPoint"/> with <see cref="EdgeList"/> uninitialized (null).
    /// </summary>
    public static readonly TriPoint Empty = new TriPoint();

    public double X;

    public double Y;

    public List<TriEdge> EdgeList;

    public TriPoint() { }

    public TriPoint(double x, double y)
    {
        X = x;
        Y = y;
        EdgeList = null;
    }

    public TriPoint(Vector2F p)
    {
        X = p.X;
        Y = p.Y;
        EdgeList = null;
    }

    public TriPoint(Vector2D p)
    {
        X = p.X;
        Y = p.Y;
        EdgeList = null;
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
        return Math.Sqrt(X * X + Y * Y);
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

    public static explicit operator Vector2D(TriPoint p)
    {
        return new Vector2D(p.X, p.Y);
    }

    public static explicit operator Vector2F(TriPoint p)
    {
        return new Vector2F((float)p.X, (float)p.Y);
    }

    public class Comparer : IComparer<TriPoint>
    {
        public int Compare(TriPoint a, TriPoint b)
        {
            if (a.Y < b.Y)
            {
                return -1;
            }
            else if (a.Y > b.Y)
            {
                return 1;
            }
            else
            {
                if (a.X < b.X)
                    return -1;
                else if (a.X > b.X)
                    return 1;
                else
                    return 0;
            }
        }
    }

    public override string ToString()
    {
        return $"{X},{Y}";
    }

    public bool Equals(double x, double y)
    {
        return X == x && Y == y;
    }

    public override bool Equals(object obj)
    {
        if (obj is TriPoint op)
            return X == op.X && Y == op.Y;
        else
            return false;
    }

    public bool Equals(TriPoint other)
    {
        return X == other.X && Y == other.Y;
    }

    public static TriPoint operator *(TriPoint a, double scale)
    {
        return new TriPoint(a.X * scale, a.Y * scale)
        {
            EdgeList = a.EdgeList,
        };
    }

    public static TriPoint operator *(TriPoint a, Vector2D scale)
    {
        return new TriPoint(a.X * scale.X, a.Y * scale.Y)
        {
            EdgeList = a.EdgeList,
        };
    }

    public static TriPoint operator +(TriPoint a, Vector2D delta)
    {
        return new TriPoint(a.X + delta.X, a.Y + delta.Y)
        {
            EdgeList = a.EdgeList,
        };
    }
}
