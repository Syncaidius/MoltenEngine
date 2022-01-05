using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "ulong"/> vector comprised of 3 components.</summary>
	public partial struct Vector3UL
	{
           /// <summary>
        /// A unit <see cref="Vector3UL"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3UL Up = new Vector3UL(0, 1UL, 0);

        /// <summary>
        /// A unit <see cref="Vector3UL"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3UL Down = new Vector3UL(0, -1UL, 0);

        /// <summary>
        /// A unit <see cref="Vector3UL"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3UL Left = new Vector3UL(-1UL, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3UL"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3UL Right = new Vector3UL(1UL, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3UL"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3UL ForwardRH = new Vector3UL(0, 0, -1UL);

        /// <summary>
        /// A unit <see cref="Vector3UL"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3UL ForwardLH = new Vector3UL(0, 0, 1UL);

        /// <summary>
        /// A unit <see cref="Vector3UL"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3UL BackwardRH = new Vector3UL(0, 0, 1UL);

        /// <summary>
        /// A unit <see cref="Vector3UL"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3UL BackwardLH = new Vector3UL(0, 0, -1UL);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UL"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UL"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UL"/>.</param>

        public static Vector3UL Cross(ref Vector3UL left, ref Vector3UL right)
        {
            return new Vector3UL(
                ((left.Y * right.Z) - (left.Z * right.Y)),
                ((left.Z * right.X) - (left.X * right.Z)),
                ((left.X * right.Y) - (left.Y * right.X)));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UL"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UL"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UL"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3UL"/>.</returns>
        public static Vector3UL Cross(Vector3UL left, Vector3UL right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

