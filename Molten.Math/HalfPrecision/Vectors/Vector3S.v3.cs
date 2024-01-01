using System.Runtime.InteropServices;

namespace Molten.HalfPrecision;

///<summary>A <see cref = "short"/> vector comprised of 3 components.</summary>
public partial struct Vector3S
{
    /// <summary>
    /// A unit <see cref="Vector3S"/> designating up (0, 1, 0).
    /// </summary>
    public static readonly Vector3S Up = new Vector3S((short)0, (short)1, (short)0);

    /// <summary>
    /// A unit <see cref="Vector3S"/> designating down (0, -1, 0).
    /// </summary>
    public static readonly Vector3S Down = new Vector3S((short)0, -(short)1, (short)0);

    /// <summary>
    /// A unit <see cref="Vector3S"/> designating left (-1, 0, 0).
    /// </summary>
    public static readonly Vector3S Left = new Vector3S(-(short)1, (short)0, (short)0);

    /// <summary>
    /// A unit <see cref="Vector3S"/> designating right (1, 0, 0).
    /// </summary>
    public static readonly Vector3S Right = new Vector3S((short)1, (short)0, (short)0);

    /// <summary>
    /// A unit <see cref="Vector3S"/> designating forward in a right-handed coordinate system (0, 0, -1).
    /// </summary>
    public static readonly Vector3S ForwardRH = new Vector3S((short)0, (short)0, -(short)1);

    /// <summary>
    /// A unit <see cref="Vector3S"/> designating forward in a left-handed coordinate system (0, 0, 1).
    /// </summary>
    public static readonly Vector3S ForwardLH = new Vector3S((short)0, (short)0, (short)1);

    /// <summary>
    /// A unit <see cref="Vector3S"/> designating backward in a right-handed coordinate system (0, 0, 1).
    /// </summary>
    public static readonly Vector3S BackwardRH = new Vector3S((short)0, (short)0, (short)1);

    /// <summary>
    /// A unit <see cref="Vector3S"/> designating backward in a left-handed coordinate system (0, 0, -1).
    /// </summary>
    public static readonly Vector3S BackwardLH = new Vector3S((short)0, (short)0, -(short)1);

#region Static Methods
    /// <summary>
    /// Calculates the cross product of two <see cref="Vector3S"/>.
    /// </summary>
    /// <param name="left">First source <see cref="Vector3S"/>.</param>
    /// <param name="right">Second source <see cref="Vector3S"/>.</param>

    public static void Cross(ref Vector3S left, ref Vector3S right, out Vector3S result)
    {
            result.X = (short)((left.Y * right.Z) - (left.Z * right.Y));
            result.Y = (short)((left.Z * right.X) - (left.X * right.Z));
            result.Z = (short)((left.X * right.Y) - (left.Y * right.X));
    }

    /// <summary>
    /// Calculates the cross product of two <see cref="Vector3S"/>.
    /// </summary>
    /// <param name="left">First source <see cref="Vector3S"/>.</param>
    /// <param name="right">Second source <see cref="Vector3S"/>.</param>

    public static Vector3S Cross(ref Vector3S left, ref Vector3S right)
    {
        return new Vector3S(
            (short)((left.Y * right.Z) - (left.Z * right.Y)),
            (short)((left.Z * right.X) - (left.X * right.Z)),
            (short)((left.X * right.Y) - (left.Y * right.X)));
    }

    /// <summary>
    /// Calculates the cross product of two <see cref="Vector3S"/>.
    /// </summary>
    /// <param name="left">First source <see cref="Vector3S"/>.</param>
    /// <param name="right">Second source <see cref="Vector3S"/>.</param>
    /// <returns>The cross product of the two <see cref="Vector3S"/>.</returns>
    public static Vector3S Cross(Vector3S left, Vector3S right)
    {
        return Cross(ref left, ref right);
    }
#endregion
}

