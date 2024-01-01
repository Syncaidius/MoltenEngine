namespace Molten;

///<summary>A <see cref = "int"/> vector comprised of 2 components.</summary>
public partial struct Vector2I
{
#region Static Methods
    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    /// <param name="left">First source vector.</param>
    /// <param name="right">Second source vector.</param>
    public static int Cross(ref Vector2I left, ref Vector2I right)
    {
        return ((left.X * right.Y) - (left.Y * right.X));
    }

    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    /// <param name="left">First source vector.</param>
    /// <param name="right">Second source vector.</param>
    public static int Cross(Vector2I left, Vector2I right)
    {
        return ((left.X * right.Y) - (left.Y * right.X));
    }
#endregion
}

