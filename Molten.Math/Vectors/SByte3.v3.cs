




using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of 3 components.</summary>
	public partial struct SByte3
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="SByte3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="SByte3"/>.</param>
        /// <param name="right">Second source <see cref="SByte3"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="SByte3"/>.</param>
        public static SByte3 Cross(ref SByte3 left, ref SByte3 right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
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

