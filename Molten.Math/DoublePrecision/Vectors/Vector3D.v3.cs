namespace Molten.DoublePrecision
{
    ///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
    public partial struct Vector3D
	{
        /// <summary>
        /// A unit <see cref="Vector3D"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3D Up = new Vector3D(0D, 1D, 0D);

        /// <summary>
        /// A unit <see cref="Vector3D"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3D Down = new Vector3D(0D, -1D, 0D);

        /// <summary>
        /// A unit <see cref="Vector3D"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3D Left = new Vector3D(-1D, 0D, 0D);

        /// <summary>
        /// A unit <see cref="Vector3D"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3D Right = new Vector3D(1D, 0D, 0D);

        /// <summary>
        /// A unit <see cref="Vector3D"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3D ForwardRH = new Vector3D(0D, 0D, -1D);

        /// <summary>
        /// A unit <see cref="Vector3D"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3D ForwardLH = new Vector3D(0D, 0D, 1D);

        /// <summary>
        /// A unit <see cref="Vector3D"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3D BackwardRH = new Vector3D(0D, 0D, 1D);

        /// <summary>
        /// A unit <see cref="Vector3D"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3D BackwardLH = new Vector3D(0D, 0D, -1D);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3D"/>.</param>
        /// <param name="right">Second source <see cref="Vector3D"/>.</param>

        public static void Cross(ref Vector3D left, ref Vector3D right, out Vector3D result)
        {
                result.X = ((left.Y * right.Z) - (left.Z * right.Y));
                result.Y = ((left.Z * right.X) - (left.X * right.Z));
                result.Z = ((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3D"/>.</param>
        /// <param name="right">Second source <see cref="Vector3D"/>.</param>

        public static Vector3D Cross(ref Vector3D left, ref Vector3D right)
        {
            return new Vector3D(
                ((left.Y * right.Z) - (left.Z * right.Y)),
                ((left.Z * right.X) - (left.X * right.Z)),
                ((left.X * right.Y) - (left.Y * right.X)));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3D"/>.</param>
        /// <param name="right">Second source <see cref="Vector3D"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3D"/>.</returns>
        public static Vector3D Cross(Vector3D left, Vector3D right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

