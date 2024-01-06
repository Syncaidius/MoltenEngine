namespace Molten.HalfPrecision;

///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
public partial struct Vector3US
{

#region Static Methods
    /// <summary>
    /// Calculates the cross product of two <see cref="Vector3US"/>.
    /// </summary>
    /// <param name="left">First source <see cref="Vector3US"/>.</param>
    /// <param name="right">Second source <see cref="Vector3US"/>.</param>

    public static void Cross(ref Vector3US left, ref Vector3US right, out Vector3US result)
    {
            result.X = (ushort)((left.Y * right.Z) - (left.Z * right.Y));
            result.Y = (ushort)((left.Z * right.X) - (left.X * right.Z));
            result.Z = (ushort)((left.X * right.Y) - (left.Y * right.X));
    }

    /// <summary>
    /// Calculates the cross product of two <see cref="Vector3US"/>.
    /// </summary>
    /// <param name="left">First source <see cref="Vector3US"/>.</param>
    /// <param name="right">Second source <see cref="Vector3US"/>.</param>

    public static Vector3US Cross(ref Vector3US left, ref Vector3US right)
    {
        return new Vector3US(
            (ushort)((left.Y * right.Z) - (left.Z * right.Y)),
            (ushort)((left.Z * right.X) - (left.X * right.Z)),
            (ushort)((left.X * right.Y) - (left.Y * right.X)));
    }

    /// <summary>
    /// Calculates the cross product of two <see cref="Vector3US"/>.
    /// </summary>
    /// <param name="left">First source <see cref="Vector3US"/>.</param>
    /// <param name="right">Second source <see cref="Vector3US"/>.</param>
    /// <returns>The cross product of the two <see cref="Vector3US"/>.</returns>
    public static Vector3US Cross(Vector3US left, Vector3US right)
    {
        return Cross(ref left, ref right);
    }
#endregion
}

