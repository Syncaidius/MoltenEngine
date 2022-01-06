




using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 3 components.</summary>
	public partial struct SByte3
	{
           /// <summary>
        /// A unit <see cref="SByte3"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly SByte3 Up = new SByte3(0, 1, 0);

        /// <summary>
        /// A unit <see cref="SByte3"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly SByte3 Down = new SByte3(0, -1, 0);

        /// <summary>
        /// A unit <see cref="SByte3"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly SByte3 Left = new SByte3(-1, 0, 0);

        /// <summary>
        /// A unit <see cref="SByte3"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly SByte3 Right = new SByte3(1, 0, 0);

        /// <summary>
        /// A unit <see cref="SByte3"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly SByte3 ForwardRH = new SByte3(0, 0, -1);

        /// <summary>
        /// A unit <see cref="SByte3"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly SByte3 ForwardLH = new SByte3(0, 0, 1);

        /// <summary>
        /// A unit <see cref="SByte3"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly SByte3 BackwardRH = new SByte3(0, 0, 1);

        /// <summary>
        /// A unit <see cref="SByte3"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly SByte3 BackwardLH = new SByte3(0, 0, -1);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="SByte3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="SByte3"/>.</param>
        /// <param name="right">Second source <see cref="SByte3"/>.</param>

        public static void Cross(ref SByte3 left, ref SByte3 right, out SByte3 result)
        {
                result.X = (sbyte)((left.Y * right.Z) - (left.Z * right.Y));
                result.Y = (sbyte)((left.Z * right.X) - (left.X * right.Z));
                result.Z = (sbyte)((left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="SByte3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="SByte3"/>.</param>
        /// <param name="right">Second source <see cref="SByte3"/>.</param>

        public static SByte3 Cross(ref SByte3 left, ref SByte3 right)
        {
            return new SByte3(
                (sbyte)((left.Y * right.Z) - (left.Z * right.Y)),
                (sbyte)((left.Z * right.X) - (left.X * right.Z)),
                (sbyte)((left.X * right.Y) - (left.Y * right.X)));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="SByte3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="SByte3"/>.</param>
        /// <param name="right">Second source <see cref="SByte3"/>.</param>
        /// <returns>The cross product of the two <see cref="SByte3"/>.</returns>
        public static SByte3 Cross(SByte3 left, SByte3 right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

