using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of 3 components.</summary>
	public partial struct Vector3L
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3L"/>.</param>
        /// <param name="right">Second source <see cref="Vector3L"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3L"/>.</param>
        public static Vector3L Cross(ref Vector3L left, ref Vector3L right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3L"/>.</param>
        /// <param name="right">Second source <see cref="Vector3L"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3L"/>.</returns>
        public static Vector3L Cross(Vector3L left, Vector3L right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

