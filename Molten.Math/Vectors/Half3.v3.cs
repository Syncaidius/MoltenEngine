using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 3 components.</summary>
	public partial struct Half3
	{
#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Half3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3"/>.</param>
        /// <param name="right">Second source <see cref="Half3"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Half3"/>.</param>
        public static Half3 Cross(ref Half3 left, ref Half3 right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Half3"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Half3"/>.</param>
        /// <param name="right">Second source <see cref="Half3"/>.</param>
        /// <returns>The cross product of the two <see cref="Half3"/>.</returns>
        public static Half3 Cross(Half3 left, Half3 right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

