using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 3 components.</summary>
	public partial struct Vector3I
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3I"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3I"/>.</param>
        /// <param name="right">Second source <see cref="Vector3I"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3I"/>.</param>
        public static Vector3I Cross(ref Vector3I left, ref Vector3I right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3I"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3I"/>.</param>
        /// <param name="right">Second source <see cref="Vector3I"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3I"/>.</returns>
        public static Vector3I Cross(Vector3I left, Vector3I right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

