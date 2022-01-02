using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
	public partial struct Half3U
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Half3U"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3U"/>.</param>
        /// <param name="right">Second source <see cref="Half3U"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Half3U"/>.</param>
        public static Half3U Cross(ref Half3U left, ref Half3U right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Half3U"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3U"/>.</param>
        /// <param name="right">Second source <see cref="Half3U"/>.</param>
        /// <returns>The cross product of the two <see cref="Half3U"/>.</returns>
        public static Half3U Cross(Half3U left, Half3U right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

