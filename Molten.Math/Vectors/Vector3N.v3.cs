using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "nint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3N
	{
           /// <summary>
        /// A unit <see cref="Vector3N"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3N Up = new Vector3N(0, 1, 0);

        /// <summary>
        /// A unit <see cref="Vector3N"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3N Down = new Vector3N(0, -1, 0);

        /// <summary>
        /// A unit <see cref="Vector3N"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3N Left = new Vector3N(-1, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3N"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3N Right = new Vector3N(1, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3N"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3N ForwardRH = new Vector3N(0, 0, -1);

        /// <summary>
        /// A unit <see cref="Vector3N"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3N ForwardLH = new Vector3N(0, 0, 1);

        /// <summary>
        /// A unit <see cref="Vector3N"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3N BackwardRH = new Vector3N(0, 0, 1);

        /// <summary>
        /// A unit <see cref="Vector3N"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3N BackwardLH = new Vector3N(0, 0, -1);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3N"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3N"/>.</param>
        /// <param name="right">Second source <see cref="Vector3N"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3N"/>.</param>
        public static Vector3N Cross(ref Vector3N left, ref Vector3N right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3N"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3N"/>.</param>
        /// <param name="right">Second source <see cref="Vector3N"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3N"/>.</returns>
        public static Vector3N Cross(Vector3N left, Vector3N right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

