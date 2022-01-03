using System.Runtime.InteropServices;

namespace Molten
{
	///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
	public partial struct Vector3F
	{
           /// <summary>
        /// A unit <see cref="Vector3F"/> designating up (0, 1, 0).
        /// </summary>
        public static readonly Vector3F Up = new Vector3F(0F, 1F, 0F);

        /// <summary>
        /// A unit <see cref="Vector3F"/> designating down (0, -1, 0).
        /// </summary>
        public static readonly Vector3F Down = new Vector3F(0F, -1F, 0F);

        /// <summary>
        /// A unit <see cref="Vector3F"/> designating left (-1, 0, 0).
        /// </summary>
        public static readonly Vector3F Left = new Vector3F(-1F, 0F, 0F);

        /// <summary>
        /// A unit <see cref="Vector3F"/> designating right (1, 0, 0).
        /// </summary>
        public static readonly Vector3F Right = new Vector3F(1F, 0F, 0F);

        /// <summary>
        /// A unit <see cref="Vector3F"/> designating forward in a right-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3F ForwardRH = new Vector3F(0F, 0F, -1F);

        /// <summary>
        /// A unit <see cref="Vector3F"/> designating forward in a left-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3F ForwardLH = new Vector3F(0F, 0F, 1F);

        /// <summary>
        /// A unit <see cref="Vector3F"/> designating backward in a right-handed coordinate system (0, 0, 1).
        /// </summary>
        public static readonly Vector3F BackwardRH = new Vector3F(0F, 0F, 1F);

        /// <summary>
        /// A unit <see cref="Vector3F"/> designating backward in a left-handed coordinate system (0, 0, -1).
        /// </summary>
        public static readonly Vector3F BackwardLH = new Vector3F(0F, 0F, -1F);

#region Static Methods
        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3F"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3F"/>.</param>
        /// <param name="right">Second source <see cref="Vector3F"/>.</param>
        /// <param name="result">When the method completes, contains he cross product of the two <see cref="Vector3F"/>.</param>
        public static Vector3F Cross(ref Vector3F left, ref Vector3F right)
        {
            return new Vector3F(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));
        }

        /// <summary>
        /// Calculates the cross product of two <see cref="Vector3F"/>.
        /// </summary>
        /// <param name="left">First source <see cref="Vector3F"/>.</param>
        /// <param name="right">Second source <see cref="Vector3F"/>.</param>
        /// <returns>The cross product of the two <see cref="Vector3F"/>.</returns>
        public static Vector3F Cross(Vector3F left, Vector3F right)
        {
            return Cross(ref left, ref right);
        }
#endregion
	}
}

