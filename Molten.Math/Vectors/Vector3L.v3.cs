namespace Molten
{
    ///<summary>A <see cref = "long"/> vector comprised of 3 components.</summary>
    public partial struct Vector3L
	{
           /// <summary>
        /// A unit <see cref="Vector3L"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3L Up = new Vector3L(0, 1L, 0);

        /// <summary>
        /// A unit <see cref="Vector3L"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3L Down = new Vector3L(0, -1L, 0);

        /// <summary>
        /// A unit <see cref="Vector3L"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3L Left = new Vector3L(-1L, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3L"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3L Right = new Vector3L(1L, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3L"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3L ForwardRH = new Vector3L(0, 0, -1L);

        /// <summary>
        /// A unit <see cref="Vector3L"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3L ForwardLH = new Vector3L(0, 0, 1L);

        /// <summary>
        /// A unit <see cref="Vector3L"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3L BackwardRH = new Vector3L(0, 0, 1L);

        /// <summary>
        /// A unit <see cref="Vector3L"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3L BackwardLH = new Vector3L(0, 0, -1L);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3L"/>.</param>
        /// <param name="right">Second source <see cref="Vector3L"/>.</param>

        public static void Cross(ref Vector3L left, ref Vector3L right, out Vector3L result)
        {
                result.X = ((left.Y * right.Z) - (left.Z * right.Y));
                result.Y = ((left.Z * right.X) - (left.X * right.Z));
                result.Z = ((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3L"/>.</param>
        /// <param name="right">Second source <see cref="Vector3L"/>.</param>

        public static Vector3L Cross(ref Vector3L left, ref Vector3L right)
        {
            return new Vector3L(
                ((left.Y * right.Z) - (left.Z * right.Y)),
                ((left.Z * right.X) - (left.X * right.Z)),
                ((left.X * right.Y) - (left.Y * right.X)));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3L"/>.</param>
        /// <param name="right">Second source <see cref="Vector3L"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3L"/>.</returns>
        public static Vector3L Cross(Vector3L left, Vector3L right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

