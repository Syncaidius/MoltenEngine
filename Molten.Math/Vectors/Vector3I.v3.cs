using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "int"/> vector comprised of 3 components.</summary>
	public partial struct Vector3I
	{
           /// <summary>
        /// A unit <see cref="Vector3I"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3I Up = new Vector3I(0, 1, 0);

        /// <summary>
        /// A unit <see cref="Vector3I"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3I Down = new Vector3I(0, -1, 0);

        /// <summary>
        /// A unit <see cref="Vector3I"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3I Left = new Vector3I(-1, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3I"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3I Right = new Vector3I(1, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3I"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3I ForwardRH = new Vector3I(0, 0, -1);

        /// <summary>
        /// A unit <see cref="Vector3I"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3I ForwardLH = new Vector3I(0, 0, 1);

        /// <summary>
        /// A unit <see cref="Vector3I"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3I BackwardRH = new Vector3I(0, 0, 1);

        /// <summary>
        /// A unit <see cref="Vector3I"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3I BackwardLH = new Vector3I(0, 0, -1);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3I"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3I"/>.</param>
        /// <param name="right">Second source <see cref="Vector3I"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3I"/>.</param>
        public static Vector3I Cross(ref Vector3I left, ref Vector3I right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3I"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3I"/>.</param>
        /// <param name="right">Second source <see cref="Vector3I"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3I"/>.</returns>
        public static Vector3I Cross(Vector3I left, Vector3I right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

