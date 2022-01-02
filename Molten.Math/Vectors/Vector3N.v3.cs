using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3N
	{
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

