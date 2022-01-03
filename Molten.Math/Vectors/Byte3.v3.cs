using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "byte"/> vector comprised of 3 components.</summary>
	public partial struct Byte3
	{
           /// <summary>
        /// A unit <see cref="Byte3"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Byte3 Up = new Byte3(0, 1, 0);

        /// <summary>
        /// A unit <see cref="Byte3"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Byte3 Down = new Byte3(0, -1, 0);

        /// <summary>
        /// A unit <see cref="Byte3"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Byte3 Left = new Byte3(-1, 0, 0);

        /// <summary>
        /// A unit <see cref="Byte3"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Byte3 Right = new Byte3(1, 0, 0);

        /// <summary>
        /// A unit <see cref="Byte3"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Byte3 ForwardRH = new Byte3(0, 0, -1);

        /// <summary>
        /// A unit <see cref="Byte3"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Byte3 ForwardLH = new Byte3(0, 0, 1);

        /// <summary>
        /// A unit <see cref="Byte3"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Byte3 BackwardRH = new Byte3(0, 0, 1);

        /// <summary>
        /// A unit <see cref="Byte3"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Byte3 BackwardLH = new Byte3(0, 0, -1);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Byte3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Byte3"/>.</param>
        /// <param name="right">Second source <see cref="Byte3"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Byte3"/>.</param>
        public static Byte3 Cross(ref Byte3 left, ref Byte3 right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Byte3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Byte3"/>.</param>
        /// <param name="right">Second source <see cref="Byte3"/>.</param>
        /// <returns>The cross product of the two <see cref="Byte3"/>.</returns>
        public static Byte3 Cross(Byte3 left, Byte3 right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

