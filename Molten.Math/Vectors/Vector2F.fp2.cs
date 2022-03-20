namespace Molten
{
    ///<summary>A <see cref = "float"/> vector comprised of 2 components.</summary>
    public partial struct Vector2F
	{
#region Instance methods
        
#endregion

#region Static Methods
        /// <summary>
        /// Transforms a 2D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector2F"/>.</param>
        public static void Transform(ref Vector2F vector, ref QuaternionF rotation, out Vector2F result)
        {
            float x = rotation.X + rotation.X;
            float y = rotation.Y + rotation.Y;
            float z = rotation.Z + rotation.Z;
            float wz = rotation.W * z;
            float xx = rotation.X * x;
            float xy = rotation.X * y;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;

            result = new Vector2F((vector.X * (1F - yy - zz)) + (vector.Y * (xy - wz)), (vector.X * (xy + wz)) + (vector.Y * (1F - xx - zz)));
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector2F Transform(Vector2F vector, QuaternionF rotation)
        {
            Vector2F result;
            Transform(ref vector, ref rotation, out result);
            return result;
        }  
        
        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        public static void Transform(ref Vector2F vector, ref Matrix4F transform, out Vector4F result)
        {
            result.X = (vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41;
            result.Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42;
            result.Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43;
            result.W = (vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44;
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        public static Vector4F Transform(ref Vector2F vector, ref Matrix4F transform)
        {
            return new Vector4F(
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
        public static Vector4F Transform(Vector2F vector, Matrix4F transform)
        {
            Transform(ref vector, ref transform, out Vector4F result);
            return result;
        }

        /// <summary>
        /// Transforms an array of 2D vectors by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector2F[] source, ref Matrix4F transform, Vector4F[] destination)
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
        /// Transforms an array of vectors by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector2F[] source, ref QuaternionF rotation, Vector2F[] destination)
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
            float wz = rotation.W * z;
            float xx = rotation.X * x;
            float xy = rotation.X * y;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;

            float num1 = (1F - yy - zz);
            float num2 = (xy - wz);
            float num3 = (xy + wz);
            float num4 = (1F - xx - zz);

            for (int i = 0; i < source.Length; ++i)
            {
                destination[i] = new Vector2F(
                    (source[i].X * num1) + (source[i].Y * num2),
                    (source[i].X * num3) + (source[i].Y * num4));
            }
        }

        /// <summary>
        /// Performs a coordinate transformation using the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static Vector2F TransformCoordinate(ref Vector2F coordinate, ref Matrix4F transform)
        {
            Vector4F vector = new Vector4F();
            vector.X = (coordinate.X * transform.M11) + (coordinate.Y * transform.M21) + transform.M41;
            vector.Y = (coordinate.X * transform.M12) + (coordinate.Y * transform.M22) + transform.M42;
            vector.Z = (coordinate.X * transform.M13) + (coordinate.Y * transform.M23) + transform.M43;
            vector.W = 1F / ((coordinate.X * transform.M14) + (coordinate.Y * transform.M24) + transform.M44);

            return new Vector2F(vector.X * vector.W, vector.Y * vector.W);
        }

        /// <summary>
        /// Performs a coordinate transformation on an array of vectors using the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="source">The array of coordinate vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static void TransformCoordinate(Vector2F[] source, ref Matrix4F transform, Vector2F[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
                destination[i] = TransformCoordinate(ref source[i], ref transform);
        }

        /// <summary>
        /// Performs a normal transformation using the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed normal.</param>
        /// <remarks>
        /// A normal transform performs the transformation with the assumption that the w component
        /// is zero. This causes the fourth row and fourth column of the matrix to be unused. The
        /// end result is a vector that is not translated, but all other transformation properties
        /// apply. This is often preferred for normal vectors as normals purely represent direction
        /// rather than location because normal vectors should not be translated.
        /// </remarks>
        public static Vector2F TransformNormal(ref Vector2F normal, ref Matrix4F transform)
        {
            return new Vector2F(
                (normal.X * transform.M11) + (normal.Y * transform.M21),
                (normal.X * transform.M12) + (normal.Y * transform.M22));
        }

        /// <summary>
        /// Performs a normal transformation using the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <returns>The transformed normal.</returns>
        /// <remarks>
        /// A normal transform performs the transformation with the assumption that the w component
        /// is zero. This causes the fourth row and fourth column of the matrix to be unused. The
        /// end result is a vector that is not translated, but all other transformation properties
        /// apply. This is often preferred for normal vectors as normals purely represent direction
        /// rather than location because normal vectors should not be translated.
        /// </remarks>
        public static Vector2F TransformNormal(Vector2F normal, Matrix4F transform)
        {
            return TransformNormal(ref normal, ref transform);
        }


        /// <summary>
        /// Performs a normal transformation using the given <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix2F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed normal.</param>
        public static Vector2F TransformNormal(ref Vector2F normal, ref Matrix2F transform)
        {
            return new Vector2F(
                (normal.X * transform.M11) + (normal.Y * transform.M21),
                (normal.X * transform.M12) + (normal.Y * transform.M22));
        }

        /// <summary>
        /// Performs a normal transformation using the given <see cref="Matrix2F"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix2F"/>.</param>
        /// <returns>The transformed normal.</returns>
        public static Vector2F TransformNormal(Vector2F normal, Matrix2F transform)
        {
            return TransformNormal(ref normal, ref transform);
        }

        /// <summary>
        /// Performs a normal transformation on an array of vectors using the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="source">The array of normal vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.
        /// This array may be the same array as <paramref name="source"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        /// <remarks>
        /// A normal transform performs the transformation with the assumption that the w component
        /// is zero. This causes the fourth row and fourth column of the matrix to be unused. The
        /// end result is a vector that is not translated, but all other transformation properties
        /// apply. This is often preferred for normal vectors as normals purely represent direction
        /// rather than location because normal vectors should not be translated.
        /// </remarks>
        public static void TransformNormal(Vector2F[] source, ref Matrix4F transform, Vector2F[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
                destination[i] = TransformNormal(ref source[i], ref transform);
        }
#endregion
	}
}

