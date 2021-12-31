using System.Runtime.InteropServices;
using System;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 2 components.</summary>
	public partial struct Vector2D
	{
#region Instance methods
        
#endregion

#region Static Methods
		/// <summary>
        /// Transforms an array of vectors by the given <see cref="QuaternionD"/> rotation.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="rotation">The <see cref="QuaternionD"/> rotation to apply.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector2D[] source, ref QuaternionD rotation, Vector2D[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double yy = rotation.Y * y;
            double zz = rotation.Z * z;

            double num1 = (1.0f - yy - zz);
            double num2 = (xy - wz);
            double num3 = (xy + wz);
            double num4 = (1.0f - xx - zz);

            for (int i = 0; i < source.Length; ++i)
            {
                destination[i] = new Vector2D(
                    (source[i].X * num1) + (source[i].Y * num2),
                    (source[i].X * num3) + (source[i].Y * num4));
            }
        }
#endregion
	}
}

