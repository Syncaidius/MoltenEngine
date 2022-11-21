using System.Runtime.CompilerServices;

namespace Molten.DoublePrecision
{
	///<summary>A <see cref = "double"/> vector comprised of 2 components.</summary>
	public partial struct Vector2D
	{
#region Instance methods
        public Vector2D GetOrthonormal(bool polarity = true, bool allowZero = false)
        {
            double len = Length();
            double az = allowZero ? 1 : 0;
            double azInv = 1 - az;

            if(len == 0)
                return polarity ? new Vector2D(0, azInv) : new Vector2D(0, -azInv);
            else
                return polarity ? new Vector2D(-Y/len, X/len) : new Vector2D(Y/len, -X/len);
        }

        public Vector2D GetOrthogonal(bool polarity)
        {
            return polarity ? new Vector2D(-Y, X) : new Vector2D(Y, -X);
        }
#endregion

#region Static Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Winding GetWinding(Vector2D pa, Vector2D pb, Vector2D pc)
        {
            return GetWinding(ref pa, ref pb, ref pc);
        }

        /// Forumla to calculate signed area
        /// Positive if CCW
        /// Negative if CW
        /// 0 if collinear
        /// A[1,P2,P3]  =  (x1*y2 - y1*x2) + (x2*y3 - y2*x3) + (x3*y1 - y3*x1)
        ///              =  (x1-x3)*(y2-y3) - (y1-y3)*(x2-x3)
        public static Winding GetWinding(ref Vector2D pa, ref Vector2D pb, ref Vector2D pc)
        {
            double detleft = (pa.X - pc.X) * (pb.Y - pc.Y);
            double detright = (pa.Y - pc.Y) * (pb.X - pc.X);
            double val = detleft - detright;

            if (val > -TriUtil.EPSILON && val < TriUtil.EPSILON)
                return Winding.Collinear;
            else if (val > 0)
                return Winding.CounterClockwise;

            return Winding.Clockwise;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetWindingSign(Vector2D pa, Vector2D pb, Vector2D pc)
        {
            return GetWindingSign(ref pa, ref pb, ref pc);
        }

        public static int GetWindingSign(ref Vector2D pa, ref Vector2D pb, ref Vector2D pc)
        {
            double detleft = (pa.X - pc.X) * (pb.Y - pc.Y);
            double detright = (pa.Y - pc.Y) * (pb.X - pc.X);
            double val = detleft - detright;

            if (val > -TriUtil.EPSILON && val < TriUtil.EPSILON)
                return 0;
            else if (val > 0)
                return -1;

            return 1;
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="QuaternionD"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionD"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector2D"/>.</param>
        public static void Transform(ref Vector2D vector, ref QuaternionD rotation, out Vector2D result)
        {
            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double yy = rotation.Y * y;
            double zz = rotation.Z * z;

            result = new Vector2D((vector.X * (1D - yy - zz)) + (vector.Y * (xy - wz)), (vector.X * (xy + wz)) + (vector.Y * (1D - xx - zz)));
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="QuaternionD"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionD"/> rotation to apply.</param>
        /// <returns>The transformed <see cref="Vector4D"/>.</returns>
        public static Vector2D Transform(Vector2D vector, QuaternionD rotation)
        {
            Vector2D result;
            Transform(ref vector, ref rotation, out result);
            return result;
        }  
        
        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        public static void Transform(ref Vector2D vector, ref Matrix4D transform, out Vector4D result)
        {
            result.X = (vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41;
            result.Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42;
            result.Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43;
            result.W = (vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44;
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        public static Vector4D Transform(ref Vector2D vector, ref Matrix4D transform)
        {
            return new Vector4D(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + transform.M43,
                (vector.X * transform.M14) + (vector.Y * transform.M24) + transform.M44);
        }

        /// <summary>
        /// Transforms a 2D vector by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <returns>The transformed <see cref="Vector4D"/>.</returns>
        public static Vector4D Transform(Vector2D vector, Matrix4D transform)
        {
            Transform(ref vector, ref transform, out Vector4D result);
            return result;
        }

        /// <summary>
        /// Transforms an array of 2D vectors by the given <see cref="Matrix4D"/>.
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

            double num1 = (1D - yy - zz);
            double num2 = (xy - wz);
            double num3 = (xy + wz);
            double num4 = (1D - xx - zz);

            for (int i = 0; i < source.Length; ++i)
            {
                destination[i] = new Vector2D(
                    (source[i].X * num1) + (source[i].Y * num2),
                    (source[i].X * num3) + (source[i].Y * num4));
            }
        }

        /// <summary>
        /// Performs a coordinate transformation using the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static Vector2D TransformCoordinate(ref Vector2D coordinate, ref Matrix4D transform)
        {
            Vector4D vector = new Vector4D();
            vector.X = (coordinate.X * transform.M11) + (coordinate.Y * transform.M21) + transform.M41;
            vector.Y = (coordinate.X * transform.M12) + (coordinate.Y * transform.M22) + transform.M42;
            vector.Z = (coordinate.X * transform.M13) + (coordinate.Y * transform.M23) + transform.M43;
            vector.W = 1D / ((coordinate.X * transform.M14) + (coordinate.Y * transform.M24) + transform.M44);

            return new Vector2D(vector.X * vector.W, vector.Y * vector.W);
        }

        /// <summary>
        /// Performs a coordinate transformation on an array of vectors using the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="source">The array of coordinate vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
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
        public static void TransformCoordinate(Vector2D[] source, ref Matrix4D transform, Vector2D[] destination)
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
        /// Performs a normal transformation using the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <param name="result">When the method completes, contains the transformed normal.</param>
        /// <remarks>
        /// A normal transform performs the transformation with the assumption that the w component
        /// is zero. This causes the fourth row and fourth column of the matrix to be unused. The
        /// end result is a vector that is not translated, but all other transformation properties
        /// apply. This is often preferred for normal vectors as normals purely represent direction
        /// rather than location because normal vectors should not be translated.
        /// </remarks>
        public static Vector2D TransformNormal(ref Vector2D normal, ref Matrix4D transform)
        {
            return new Vector2D(
                (normal.X * transform.M11) + (normal.Y * transform.M21),
                (normal.X * transform.M12) + (normal.Y * transform.M22));
        }

        /// <summary>
        /// Performs a normal transformation using the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <returns>The transformed normal.</returns>
        /// <remarks>
        /// A normal transform performs the transformation with the assumption that the w component
        /// is zero. This causes the fourth row and fourth column of the matrix to be unused. The
        /// end result is a vector that is not translated, but all other transformation properties
        /// apply. This is often preferred for normal vectors as normals purely represent direction
        /// rather than location because normal vectors should not be translated.
        /// </remarks>
        public static Vector2D TransformNormal(Vector2D normal, Matrix4D transform)
        {
            return TransformNormal(ref normal, ref transform);
        }


        /// <summary>
        /// Performs a normal transformation using the given <see cref="Matrix2D"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix2D"/>.</param>
        /// <param name="result">When the method completes, contains the transformed normal.</param>
        public static Vector2D TransformNormal(ref Vector2D normal, ref Matrix2D transform)
        {
            return new Vector2D(
                (normal.X * transform.M11) + (normal.Y * transform.M21),
                (normal.X * transform.M12) + (normal.Y * transform.M22));
        }

        /// <summary>
        /// Performs a normal transformation using the given <see cref="Matrix2D"/>.
        /// </summary>
        /// <param name="normal">The normal vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix2D"/>.</param>
        /// <returns>The transformed normal.</returns>
        public static Vector2D TransformNormal(Vector2D normal, Matrix2D transform)
        {
            return TransformNormal(ref normal, ref transform);
        }

        /// <summary>
        /// Performs a normal transformation on an array of vectors using the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="source">The array of normal vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
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
        public static void TransformNormal(Vector2D[] source, ref Matrix4D transform, Vector2D[] destination)
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

