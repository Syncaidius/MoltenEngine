using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 3 components.</summary>
	public partial struct Byte3
	{
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

