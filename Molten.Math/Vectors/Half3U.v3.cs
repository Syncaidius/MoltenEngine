using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
	public partial struct Half3U
	{
           /// <summary>
        /// A unit <see cref="Half3U"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Half3U Up = new Half3U(0, (ushort)1, 0);

        /// <summary>
        /// A unit <see cref="Half3U"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Half3U Down = new Half3U(0, -(ushort)1, 0);

        /// <summary>
        /// A unit <see cref="Half3U"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Half3U Left = new Half3U(-(ushort)1, 0, 0);

        /// <summary>
        /// A unit <see cref="Half3U"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Half3U Right = new Half3U((ushort)1, 0, 0);

        /// <summary>
        /// A unit <see cref="Half3U"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Half3U ForwardRH = new Half3U(0, 0, -(ushort)1);

        /// <summary>
        /// A unit <see cref="Half3U"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Half3U ForwardLH = new Half3U(0, 0, (ushort)1);

        /// <summary>
        /// A unit <see cref="Half3U"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Half3U BackwardRH = new Half3U(0, 0, (ushort)1);

        /// <summary>
        /// A unit <see cref="Half3U"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Half3U BackwardLH = new Half3U(0, 0, -(ushort)1);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Half3U"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3U"/>.</param>
        /// <param name="right">Second source <see cref="Half3U"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Half3U"/>.</param>
        public static Half3U Cross(ref Half3U left, ref Half3U right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Half3U"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3U"/>.</param>
        /// <param name="right">Second source <see cref="Half3U"/>.</param>
        /// <returns>The cross product of the two <see cref="Half3U"/>.</returns>
        public static Half3U Cross(Half3U left, Half3U right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

