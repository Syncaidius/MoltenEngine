using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "nuint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3NU
	{
           /// <summary>
        /// A unit <see cref="Vector3NU"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3NU Up = new Vector3NU(0, 1U, 0);

        /// <summary>
        /// A unit <see cref="Vector3NU"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3NU Down = new Vector3NU(0, -1U, 0);

        /// <summary>
        /// A unit <see cref="Vector3NU"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3NU Left = new Vector3NU(-1U, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3NU"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3NU Right = new Vector3NU(1U, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3NU"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3NU ForwardRH = new Vector3NU(0, 0, -1U);

        /// <summary>
        /// A unit <see cref="Vector3NU"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3NU ForwardLH = new Vector3NU(0, 0, 1U);

        /// <summary>
        /// A unit <see cref="Vector3NU"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3NU BackwardRH = new Vector3NU(0, 0, 1U);

        /// <summary>
        /// A unit <see cref="Vector3NU"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3NU BackwardLH = new Vector3NU(0, 0, -1U);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3NU"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3NU"/>.</param>
        /// <param name="right">Second source <see cref="Vector3NU"/>.</param>

        public static Vector3NU Cross(ref Vector3NU left, ref Vector3NU right)
        {
            return new Vector3NU(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3NU"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3NU"/>.</param>
        /// <param name="right">Second source <see cref="Vector3NU"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3NU"/>.</returns>
        public static Vector3NU Cross(Vector3NU left, Vector3NU right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

