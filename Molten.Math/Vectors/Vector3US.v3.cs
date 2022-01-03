using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
	public partial struct Vector3US
	{
           /// <summary>
        /// A unit <see cref="Vector3US"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3US Up = new Vector3US(0, (ushort)1, 0);

        /// <summary>
        /// A unit <see cref="Vector3US"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3US Down = new Vector3US(0, -(ushort)1, 0);

        /// <summary>
        /// A unit <see cref="Vector3US"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3US Left = new Vector3US(-(ushort)1, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3US"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3US Right = new Vector3US((ushort)1, 0, 0);

        /// <summary>
        /// A unit <see cref="Vector3US"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3US ForwardRH = new Vector3US(0, 0, -(ushort)1);

        /// <summary>
        /// A unit <see cref="Vector3US"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3US ForwardLH = new Vector3US(0, 0, (ushort)1);

        /// <summary>
        /// A unit <see cref="Vector3US"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3US BackwardRH = new Vector3US(0, 0, (ushort)1);

        /// <summary>
        /// A unit <see cref="Vector3US"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3US BackwardLH = new Vector3US(0, 0, -(ushort)1);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3US"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3US"/>.</param>
        /// <param name="right">Second source <see cref="Vector3US"/>.</param>

        public static Vector3US Cross(ref Vector3US left, ref Vector3US right)
        {
            return new Vector3US(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
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
}

