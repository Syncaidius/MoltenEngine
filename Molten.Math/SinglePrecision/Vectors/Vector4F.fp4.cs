namespace Molten
{
	///<summary>A <see cref = "float"/> vector comprised of 4 components.</summary>
	public partial struct Vector4F
	{
#region Instance methods
        
#endregion

#region Static Methods
        /// <summary>
        /// Transforms a 4D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static Vector4F Transform(ref Vector4F vector, ref QuaternionF rotation)
        {
            float x = rotation.X + rotation.X;
            float y = rotation.Y + rotation.Y;
            float z = rotation.Z + rotation.Z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;
            float xx = rotation.X * x;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yy = rotation.Y * y;
            float yz = rotation.Y * z;
            float zz = rotation.Z * z;

            return new Vector4F(
                ((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))) + (vector.Z * (xz + wy)),
                ((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz))) + (vector.Z * (yz - wx)),
                ((vector.X * (xz - wy)) + (vector.Y * (yz + wx))) + (vector.Z * ((1.0f - xx) - yy)),
                vector.W);
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector4F Transform(Vector4F vector, QuaternionF rotation)
        {
            return Transform(ref vector, ref rotation);
        }

        /// <summary>
        /// Transforms an array of vectors by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector4F[] source, ref QuaternionF rotation, Vector4F[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            float x = rotation.X + rotation.X;
            float y = rotation.Y + rotation.Y;
            float z = rotation.Z + rotation.Z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;
            float xx = rotation.X * x;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yy = rotation.Y * y;
            float yz = rotation.Y * z;
            float zz = rotation.Z * z;

            float num1 = ((1.0f - yy) - zz);
            float num2 = (xy - wz);
            float num3 = (xz + wy);
            float num4 = (xy + wz);
            float num5 = ((1.0f - xx) - zz);
            float num6 = (yz - wx);
            float num7 = (xz - wy);
            float num8 = (yz + wx);
            float num9 = ((1.0f - xx) - yy);

            for (int i = 0; i < source.Length; ++i)
            {
                destination[i] = new Vector4F(
                    ((source[i].X * num1) + (source[i].Y * num2)) + (source[i].Z * num3),
                    ((source[i].X * num4) + (source[i].Y * num5)) + (source[i].Z * num6),
                    ((source[i].X * num7) + (source[i].Y * num8)) + (source[i].Z * num9),
                    source[i].W);
            }
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static void Transform(ref Vector4F vector, ref Matrix4F transform, out Vector4F result)
        {
            result = new Vector4F(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + (vector.W * transform.M41),
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + (vector.W * transform.M42),
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + (vector.W * transform.M43),
                (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + (vector.W * transform.M44));
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector4F Transform(Vector4F vector, Matrix4F transform)
        {
            Vector4F result;
            Transform(ref vector, ref transform, out result);
            return result;
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrix5x4"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix5x4"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static void Transform(ref Vector4F vector, ref Matrix5x4 transform, out Vector4F result)
        {
            result = new Vector4F(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + (vector.W * transform.M41) + transform.M51,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + (vector.W * transform.M42) + transform.M52,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + (vector.W * transform.M43) + transform.M53,
                (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + (vector.W * transform.M44) + transform.M54);
        }

        /// <summary>
        /// Transforms a 4D vector by the given <see cref="Matrix5x4"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix5x4"/>.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector4F Transform(Vector4F vector, Matrix5x4 transform)
        {
            Vector4F result;
            Transform(ref vector, ref transform, out result);
            return result;
        }

        /// <summary>
        /// Transforms an array of 4D vectors by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector4F[] source, ref Matrix4F transform, Vector4F[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Transform(ref source[i], ref transform, out destination[i]);
            }
        }
#endregion
	}
}

