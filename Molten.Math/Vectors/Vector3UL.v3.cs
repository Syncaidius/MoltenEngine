using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ulong"/> vector comprised of 3 components.</summary>
	public partial struct Vector3UL
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UL"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UL"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UL"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3UL"/>.</param>
        public static Vector3UL Cross(ref Vector3UL left, ref Vector3UL right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UL"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UL"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UL"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3UL"/>.</returns>
        public static Vector3UL Cross(Vector3UL left, Vector3UL right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

