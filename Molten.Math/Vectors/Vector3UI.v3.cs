using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "uint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3UI
	{
           /// <summary>
        /// A unit <see cref="Vector3UI"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3UI Up = new Vector3UI(0, 1U, 0);

        /// <summary>
        /// A unit <see cref="Vector3UI"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3UI Down = new Vector3UI(0, -1U, 0);

        /// <summary>
        /// A unit <see cref="Vector3UI"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3UI Left = new Vector3UI(-1U, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3UI"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3UI Right = new Vector3UI(1U, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3UI"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3UI ForwardRH = new Vector3UI(0, 0, -1U);

        /// <summary>
        /// A unit <see cref="Vector3UI"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3UI ForwardLH = new Vector3UI(0, 0, 1U);

        /// <summary>
        /// A unit <see cref="Vector3UI"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3UI BackwardRH = new Vector3UI(0, 0, 1U);

        /// <summary>
        /// A unit <see cref="Vector3UI"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3UI BackwardLH = new Vector3UI(0, 0, -1U);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UI"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UI"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3UI"/>.</param>
        public static Vector3UI Cross(ref Vector3UI left, ref Vector3UI right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UI"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UI"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3UI"/>.</returns>
        public static Vector3UI Cross(Vector3UI left, Vector3UI right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

