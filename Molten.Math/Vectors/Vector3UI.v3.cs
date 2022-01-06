using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "uint"/> vector comprised of 3 components.</summary>
	public partial struct Vector3UI
	{

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UI"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UI"/>.</param>

        public static Vector3UI Cross(ref Vector3UI left, ref Vector3UI right)
        {
            return new Vector3UI(
                ((left.Y * right.Z) - (left.Z * right.Y)),
                ((left.Z * right.X) - (left.X * right.Z)),
                ((left.X * right.Y) - (left.Y * right.X)));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3UI"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3UI"/>.</param>
        /// <param name="right">Second source <see cref="Vector3UI"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3UI"/>.</returns>
        public static Vector3UI Cross(Vector3UI left, Vector3UI right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

