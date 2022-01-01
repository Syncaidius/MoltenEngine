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
        /// Transforms a 2D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector2D"/>.</param>
        public static void Transform(ref Vector2D vector, ref QuaternionF rotation, out Vector2D result)
        {
            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double yy = rotation.Y * y;
            double zz = rotation.Z * z;

            result = new Vector2D((vector.X * (1.0f - yy - zz)) + (vector.Y * (xy - wz)), (vector.X * (xy + wz)) + (vector.Y * (1.0D - xx - zz)));
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector2D Transform(Vector2D vector, QuaternionF rotation)
        {
            Vector2D result;
            Transform(ref vector, ref rotation, out result);
            return result;
        }

        

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static Vector4D Transform(ref Vector2D vector, ref Matrix4F transform)
        {
            return new Vector4D(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43,
                (vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44);
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector4F Transform(Vector2D vector, Matrix4D transform)
        {
            return Transform(ref vector, ref transform);
        }

        /// <summary>
        /// Transforms an array of 2D vectors by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector2D[] source, ref Matrix4D transform, Vector4D[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
                destination[i] = Transform(ref source[i], ref transform);
        }

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

