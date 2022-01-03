using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "short"/> vector comprised of 3 components.</summary>
	public partial struct Half3
	{
           /// <summary>
        /// A unit <see cref="Half3"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Half3 Up = new Half3(0, (short)1, 0);

        /// <summary>
        /// A unit <see cref="Half3"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Half3 Down = new Half3(0, -(short)1, 0);

        /// <summary>
        /// A unit <see cref="Half3"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Half3 Left = new Half3(-(short)1, 0, 0);

        /// <summary>
        /// A unit <see cref="Half3"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Half3 Right = new Half3((short)1, 0, 0);

        /// <summary>
        /// A unit <see cref="Half3"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Half3 ForwardRH = new Half3(0, 0, -(short)1);

        /// <summary>
        /// A unit <see cref="Half3"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Half3 ForwardLH = new Half3(0, 0, (short)1);

        /// <summary>
        /// A unit <see cref="Half3"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Half3 BackwardRH = new Half3(0, 0, (short)1);

        /// <summary>
        /// A unit <see cref="Half3"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Half3 BackwardLH = new Half3(0, 0, -(short)1);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Half3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3"/>.</param>
        /// <param name="right">Second source <see cref="Half3"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Half3"/>.</param>
        public static Half3 Cross(ref Half3 left, ref Half3 right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Half3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3"/>.</param>
        /// <param name="right">Second source <see cref="Half3"/>.</param>
        /// <returns>The cross product of the two <see cref="Half3"/>.</returns>
        public static Half3 Cross(Half3 left, Half3 right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

