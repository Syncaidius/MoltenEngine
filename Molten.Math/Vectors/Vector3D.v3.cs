using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
	public partial struct Vector3D
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3D"/>.</param>
        /// <param name="right">Second source <see cref="Vector3D"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3D"/>.</param>
        public static Vector3D Cross(ref Vector3D left, ref Vector3D right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3D"/>.</param>
        /// <param name="right">Second source <see cref="Vector3D"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3D"/>.</returns>
        public static Vector3D Cross(Vector3D left, Vector3D right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

