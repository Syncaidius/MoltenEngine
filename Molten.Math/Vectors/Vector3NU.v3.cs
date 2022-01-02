using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3NU
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3NU"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3NU"/>.</param>
        /// <param name="right">Second source <see cref="Vector3NU"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3NU"/>.</param>
        public static Vector3NU Cross(ref Vector3NU left, ref Vector3NU right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3NU"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3NU"/>.</param>
        /// <param name="right">Second source <see cref="Vector3NU"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3NU"/>.</returns>
        public static Vector3NU Cross(Vector3NU left, Vector3NU right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

