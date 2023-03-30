namespace Molten.DoublePrecision
{
    ///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
    public partial struct Vector3D
	{
#region Instance methods
        
#endregion

#region Static Methods
        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <param name="velocity">Velocity of the point.</param>
        public static void GetVelocityOfPoint(ref Vector3D point, ref Vector3D center, ref Vector3D linearVelocity, ref Vector3D angularVelocity, out Vector3D velocity)
        {
            Subtract(ref point, ref center, out Vector3D offset);
            Cross(ref angularVelocity, ref offset, out velocity);
            Add(ref velocity, ref linearVelocity, out velocity);
        }

        /// <summary>
        /// Computes the velocity of a point as if it were attached to an object with the given center and velocity.
        /// </summary>
        /// <param name="point">Point to compute the velocity of.</param>
        /// <param name="center">Center of the object to which the point is attached.</param>
        /// <param name="linearVelocity">Linear velocity of the object.</param>
        /// <param name="angularVelocity">Angular velocity of the object.</param>
        /// <returns>Velocity of the point.</returns>
        public static Vector3D GetVelocityOfPoint(Vector3D point, Vector3D center, Vector3D linearVelocity, Vector3D angularVelocity)
        {
            GetVelocityOfPoint(ref point, ref center, ref linearVelocity, ref angularVelocity, out Vector3D toReturn);
            return toReturn;
        }

        /// <summary>
        /// Projects a 3D vector from object space into screen space. 
        /// </summary>
        /// <param name="vector">The vector to project.</param>
        /// <param name="x">The X position of the viewport.</param>
        /// <param name="y">The Y position of the viewport.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        /// <param name="minZ">The minimum depth of the viewport.</param>
        /// <param name="maxZ">The maximum depth of the viewport.</param>
        /// <param name="worldViewProjection">The combined world-view-projection matrix.</param>
        /// <param name="result">When the method completes, contains the vector in screen space.</param>
        public static void Project(ref Vector3D vector, double x, double y, double width, double height, double minZ, double maxZ, ref Matrix4D worldViewProjection, out Vector3D result)
        {
            Vector3D v = new Vector3D();
            TransformCoordinate(ref vector, ref worldViewProjection, out v);

            result = new Vector3D(((1D + v.X) * 0.5D * width) + x, ((1.0f - v.Y) * 0.5D * height) + y, (v.Z * (maxZ - minZ)) + minZ);
        }

        /// <summary>
        /// Projects a 3D vector from object space into screen space. 
        /// </summary>
        /// <param name="vector">The vector to project.</param>
        /// <param name="x">The X position of the viewport.</param>
        /// <param name="y">The Y position of the viewport.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        /// <param name="minZ">The minimum depth of the viewport.</param>
        /// <param name="maxZ">The maximum depth of the viewport.</param>
        /// <param name="worldViewProjection">The combined world-view-projection matrix.</param>
        /// <returns>The vector in screen space.</returns>
        public static Vector3D Project(Vector3D vector, double x, double y, double width, double height, double minZ, double maxZ, Matrix4D worldViewProjection)
        {
            Project(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out Vector3D result);
            return result;
        }

        /// <summary>
        /// Projects a 3D vector from screen space into object space. 
        /// </summary>
        /// <param name="vector">The vector to project.</param>
        /// <param name="x">The X position of the viewport.</param>
        /// <param name="y">The Y position of the viewport.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        /// <param name="minZ">The minimum depth of the viewport.</param>
        /// <param name="maxZ">The maximum depth of the viewport.</param>
        /// <param name="worldViewProjection">The combined world-view-projection matrix.</param>
        /// <param name="result">When the method completes, contains the vector in object space.</param>
        public static void Unproject(ref Vector3D vector, double x, double y, double width, double height, double minZ, double maxZ, ref Matrix4D worldViewProjection, out Vector3D result)
        {
            Vector3D v = new Vector3D();
            Matrix4D matrix = new Matrix4D();
            Matrix4D.Invert(ref worldViewProjection, out matrix);

            v.X = (((vector.X - x) / width) * 2.0f) - 1.0f;
            v.Y = -((((vector.Y - y) / height) * 2.0f) - 1.0f);
            v.Z = (vector.Z - minZ) / (maxZ - minZ);

            TransformCoordinate(ref v, ref matrix, out result);
        }

        /// <summary>
        /// Projects a 3D vector from screen space into object space. 
        /// </summary>
        /// <param name="vector">The vector to project.</param>
        /// <param name="x">The X position of the viewport.</param>
        /// <param name="y">The Y position of the viewport.</param>
        /// <param name="width">The width of the viewport.</param>
        /// <param name="height">The height of the viewport.</param>
        /// <param name="minZ">The minimum depth of the viewport.</param>
        /// <param name="maxZ">The maximum depth of the viewport.</param>
        /// <param name="worldViewProjection">The combined world-view-projection matrix.</param>
        /// <returns>The vector in object space.</returns>
        public static Vector3D Unproject(Vector3D vector, double x, double y, double width, double height, double minZ, double maxZ, Matrix4D worldViewProjection)
        {
            Unproject(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out Vector3D result);
            return result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="QuaternionD"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionD"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4D"/>.</param>
        public static void Transform(ref Vector3D vector, ref QuaternionD rotation, out Vector3D result)
        {
            double x = rotation.X + rotation.X;
            double y = rotation.Y + rotation.Y;
            double z = rotation.Z + rotation.Z;
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;

            
            result.X = ((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))) + (vector.Z * (xz + wy));
            result.Y = ((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz))) + (vector.Z * (yz - wx));
            result.Z = ((vector.X * (xz - wy)) + (vector.Y * (yz + wx))) + (vector.Z * ((1.0f - xx) - yy));
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="QuaternionD"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionD"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4D"/>.</param>
        public static Vector3D Transform(ref Vector3D vector, ref QuaternionD rotation)
        {
            Transform(ref vector, ref rotation, out Vector3D result);
            return result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="QuaternionD"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionD"/> rotation to apply.</param>
        /// <returns>The transformed <see cref="Vector4D"/>.</returns>
        public static Vector3D Transform(Vector3D vector, QuaternionD rotation)
        {
            Transform(ref vector, ref rotation, out Vector3D result);
            return result;
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
        public static void Transform(Vector3D[] source, ref QuaternionD rotation, Vector3D[] destination)
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
            double wx = rotation.W * x;
            double wy = rotation.W * y;
            double wz = rotation.W * z;
            double xx = rotation.X * x;
            double xy = rotation.X * y;
            double xz = rotation.X * z;
            double yy = rotation.Y * y;
            double yz = rotation.Y * z;
            double zz = rotation.Z * z;

            double num1 = ((1.0f - yy) - zz);
            double num2 = (xy - wz);
            double num3 = (xz + wy);
            double num4 = (xy + wz);
            double num5 = ((1.0f - xx) - zz);
            double num6 = (yz - wx);
            double num7 = (xz - wy);
            double num8 = (yz + wx);
            double num9 = ((1.0f - xx) - yy);

            for (int i = 0; i < source.Length; ++i)
            {
                destination[i] = new Vector3D(
                    ((source[i].X * num1) + (source[i].Y * num2)) + (source[i].Z * num3),
                    ((source[i].X * num4) + (source[i].Y * num5)) + (source[i].Z * num6),
                    ((source[i].X * num7) + (source[i].Y * num8)) + (source[i].Z * num9));
            }
        }


        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix3D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix3D"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector3D"/>.</param>
        public static void Transform(ref Vector3D vector, ref Matrix3D transform, out Vector3D result)
        {
            result = new Vector3D()
            {
                X = (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31),
                Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32),
                Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33)
            };
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix3D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix3D"/>.</param>
        /// <returns>The transformed <see cref="Vector3D"/>.</returns>
        public static Vector3D Transform(Vector3D vector, Matrix3D transform)
        {
            Transform(ref vector, ref transform, out Vector3D result);
            return result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector3D"/>.</param>
        public static void Transform(ref Vector3D vector, ref Matrix4D transform, out Vector3D result)
        {
            Transform(ref vector, ref transform, out Vector4D v4Result);
            result = (Vector3D)v4Result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector3D"/>.</param>
        public static Vector3D Transform(ref Vector3D vector, ref Matrix4D transform)
        {
            Transform(ref vector, ref transform, out Vector4D result);
            return (Vector3D)result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4D"/>.</param>
        public static void Transform(ref Vector3D vector, ref Matrix4D transform, out Vector4D result)
        {
            result = new Vector4D(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + transform.M43,
                (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + transform.M44);
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <returns>The transformed <see cref="Vector4D"/>.</returns>
        public static Vector4D Transform(Vector3D vector, Matrix4D transform)
        {
            Transform(ref vector, ref transform, out Vector4D result);
            return result;
        }

        /// <summary>
        /// Transforms an array of 3D vectors by the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector3D[] source, ref Matrix4D transform, Vector4D[] destination)
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


        /// <summary>
        /// Transforms a vector using a quaternion. Specialized for x,0,0 vectors.
        /// </summary>
        /// <param name="x">X component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformX(double x, ref QuaternionD rotation, out Vector3D result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;
            double xy2 = rotation.X * y2;
            double xz2 = rotation.X * z2;
            double yy2 = rotation.Y * y2;
            double zz2 = rotation.Z * z2;
            double wy2 = rotation.W * y2;
            double wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            double transformedX = x * (1f - yy2 - zz2);
            double transformedY = x * (xy2 + wz2);
            double transformedZ = x * (xz2 - wy2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        /// <summary>
        /// Transforms a vector using a quaternion. Specialized for 0,y,0 vectors.
        /// </summary>
        /// <param name="y">Y component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformY(double y, ref QuaternionD rotation, out Vector3D result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            double x2 = rotation.X + rotation.X;
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;
            double xx2 = rotation.X * x2;
            double xy2 = rotation.X * y2;
            double yz2 = rotation.Y * z2;
            double zz2 = rotation.Z * z2;
            double wx2 = rotation.W * x2;
            double wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            double transformedX = y * (xy2 - wz2);
            double transformedY = y * (1f - xx2 - zz2);
            double transformedZ = y * (yz2 + wx2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        /// <summary>
        /// Transforms a vector using a quaternion. Specialized for 0,0,z vectors.
        /// </summary>
        /// <param name="z">Z component of the vector to transform.</param>
        /// <param name="rotation">Rotation to apply to the vector.</param>
        /// <param name="result">Transformed vector.</param>
        public static void TransformZ(double z, ref QuaternionD rotation, out Vector3D result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            double x2 = rotation.X + rotation.X;
            double y2 = rotation.Y + rotation.Y;
            double z2 = rotation.Z + rotation.Z;
            double xx2 = rotation.X * x2;
            double xz2 = rotation.X * z2;
            double yy2 = rotation.Y * y2;
            double yz2 = rotation.Y * z2;
            double wx2 = rotation.W * x2;
            double wy2 = rotation.W * y2;
            //Defer the component setting since they're used in computation.
            double transformedX = z * (xz2 + wy2);
            double transformedY = z * (yz2 - wx2);
            double transformedZ = z * (1f - xx2 - yy2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        /// <summary>
        /// Performs a coordinate transformation using the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <param name="result">When the method completes, contains the transformed coordinates.</param>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static void TransformCoordinate(ref Vector3D coordinate, ref Matrix4D transform, out Vector3D result)
        {
            Vector4D vector = new Vector4D();
            vector.X = (coordinate.X * transform.M11) + (coordinate.Y * transform.M21) + (coordinate.Z * transform.M31) + transform.M41;
            vector.Y = (coordinate.X * transform.M12) + (coordinate.Y * transform.M22) + (coordinate.Z * transform.M32) + transform.M42;
            vector.Z = (coordinate.X * transform.M13) + (coordinate.Y * transform.M23) + (coordinate.Z * transform.M33) + transform.M43;
            vector.W = 1f / ((coordinate.X * transform.M14) + (coordinate.Y * transform.M24) + (coordinate.Z * transform.M34) + transform.M44);

            result = new Vector3D(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W);
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
        public static Vector3D TransformCoordinate(ref Vector3D coordinate, ref Matrix4D transform)
        {
            TransformCoordinate(ref coordinate, ref transform, out Vector3D result);
            return result;
        }

        /// <summary>
        /// Performs a coordinate transformation using the given <see cref="Matrix4D"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4D"/>.</param>
        /// <returns>The transformed coordinates.</returns>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static Vector3D TransformCoordinate(Vector3D coordinate, Matrix4D transform)
        {
             TransformCoordinate(ref coordinate, ref transform, out Vector3D result);
             return result;
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
        public static void TransformCoordinate(Vector3D[] source, ref Matrix4D transform, Vector3D[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
                TransformCoordinate(ref source[i], ref transform, out destination[i]);
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
        public static void TransformNormal(ref Vector3D normal, ref Matrix4D transform, out Vector3D result)
        {
            result.X = (normal.X * transform.M11) + (normal.Y * transform.M21) + (normal.Z * transform.M31);
            result.Y = (normal.X * transform.M12) + (normal.Y * transform.M22) + (normal.Z * transform.M32);
            result.Z = (normal.X * transform.M13) + (normal.Y * transform.M23) + (normal.Z * transform.M33);
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
        public static Vector3D TransformNormal(ref Vector3D normal, ref Matrix4D transform)
        {
            TransformNormal(ref normal, ref transform, out Vector3D result);
            return result;
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
        public static Vector3D TransformNormal(Vector3D normal, Matrix4D transform)
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
        public static void TransformNormal(Vector3D[] source, ref Matrix4D transform, Vector3D[] destination)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
                TransformNormal(ref source[i], ref transform, out destination[i]);
        }

        /// <summary>
        /// Gets the barycentric coordinates of the point with respect to a triangle's vertices.
        /// </summary>
        /// <param name="p">Point to compute the barycentric coordinates of.</param>
        /// <param name="a">First vertex in the triangle.</param>
        /// <param name="b">Second vertex in the triangle.</param>
        /// <param name="c">Third vertex in the triangle.</param>
        /// <param name="aWeight">Weight of the first vertex.</param>
        /// <param name="bWeight">Weight of the second vertex.</param>
        /// <param name="cWeight">Weight of the third vertex.</param>
        public static void BarycentricCoordinates(ref Vector3D p, ref Vector3D a, ref Vector3D b, ref Vector3D c,
            out double aWeight, out double bWeight, out double cWeight)
        {
            Vector3D ab, ac;
            Subtract(ref b, ref a, out ab);
            Subtract(ref c, ref a, out ac);
            Vector3D triangleNormal;
            Cross(ref ab, ref ac, out triangleNormal);
            double x = triangleNormal.X < 0 ? -triangleNormal.X : triangleNormal.X;
            double y = triangleNormal.Y < 0 ? -triangleNormal.Y : triangleNormal.Y;
            double z = triangleNormal.Z < 0 ? -triangleNormal.Z : triangleNormal.Z;

            double numeratorU, numeratorV, denominator;
            if (x >= y && x >= z)
            {
                //The projection of the triangle on the YZ plane is the largest.
                numeratorU = (p.Y - b.Y) * (b.Z - c.Z) - (b.Y - c.Y) * (p.Z - b.Z); //PBC
                numeratorV = (p.Y - c.Y) * (c.Z - a.Z) - (c.Y - a.Y) * (p.Z - c.Z); //PCA
                denominator = triangleNormal.X;
            }
            else if (y >= z)
            {
                //The projection of the triangle on the XZ plane is the largest.
                numeratorU = (p.X - b.X) * (b.Z - c.Z) - (b.X - c.X) * (p.Z - b.Z); //PBC
                numeratorV = (p.X - c.X) * (c.Z - a.Z) - (c.X - a.X) * (p.Z - c.Z); //PCA
                denominator = -triangleNormal.Y;
            }
            else
            {
                //The projection of the triangle on the XY plane is the largest.
                numeratorU = (p.X - b.X) * (b.Y - c.Y) - (b.X - c.X) * (p.Y - b.Y); //PBC
                numeratorV = (p.X - c.X) * (c.Y - a.Y) - (c.X - a.X) * (p.Y - c.Y); //PCA
                denominator = triangleNormal.Z;
            }

            if (denominator < -1e-9D || denominator > 1e-9D)
            {
                denominator = 1 / denominator;
                aWeight = numeratorU * denominator;
                bWeight = numeratorV * denominator;
                cWeight = 1 - aWeight - bWeight;
            }
            else
            {
                //It seems to be a degenerate triangle.
                //In that case, pick one of the closest vertices.
                //MOST of the time, this will happen when the vertices
                //are all very close together (all three points form a single point).
                //Sometimes, though, it could be that it's more of a line.
                //If it's a little inefficient, don't worry- this is a corner case anyway.

                double distance1 = DistanceSquared(ref p, ref a);
                double distance2 = DistanceSquared(ref p, ref b);
                double distance3 = DistanceSquared(ref p, ref c);
                if (distance1 < distance2 && distance1 < distance3)
                {
                    aWeight = 1;
                    bWeight = 0;
                    cWeight = 0;
                }
                else if (distance2 < distance3)
                {
                    aWeight = 0;
                    bWeight = 1;
                    cWeight = 0;
                }
                else
                {
                    aWeight = 0;
                    bWeight = 0;
                    cWeight = 1;
                }
            }
        }
#endregion
	}
}

