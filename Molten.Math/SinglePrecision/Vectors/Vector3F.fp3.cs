namespace Molten
{
	///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
	public partial struct Vector3F
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
        public static void GetVelocityOfPoint(ref Vector3F point, ref Vector3F center, ref Vector3F linearVelocity, ref Vector3F angularVelocity, out Vector3F velocity)
        {
            Subtract(ref point, ref center, out Vector3F offset);
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
        public static Vector3F GetVelocityOfPoint(Vector3F point, Vector3F center, Vector3F linearVelocity, Vector3F angularVelocity)
        {
            GetVelocityOfPoint(ref point, ref center, ref linearVelocity, ref angularVelocity, out Vector3F toReturn);
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
        public static void Project(ref Vector3F vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix4F worldViewProjection, out Vector3F result)
        {
            Vector3F v = new Vector3F();
            TransformCoordinate(ref vector, ref worldViewProjection, out v);

            result = new Vector3F(((1F + v.X) * 0.5F * width) + x, ((1.0f - v.Y) * 0.5F * height) + y, (v.Z * (maxZ - minZ)) + minZ);
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
        public static Vector3F Project(Vector3F vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix4F worldViewProjection)
        {
            Project(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out Vector3F result);
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
        public static void Unproject(ref Vector3F vector, float x, float y, float width, float height, float minZ, float maxZ, ref Matrix4F worldViewProjection, out Vector3F result)
        {
            Vector3F v = new Vector3F();
            Matrix4F matrix = new Matrix4F();
            Matrix4F.Invert(ref worldViewProjection, out matrix);

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
        public static Vector3F Unproject(Vector3F vector, float x, float y, float width, float height, float minZ, float maxZ, Matrix4F worldViewProjection)
        {
            Unproject(ref vector, x, y, width, height, minZ, maxZ, ref worldViewProjection, out Vector3F result);
            return result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static void Transform(ref Vector3F vector, ref QuaternionF rotation, out Vector3F result)
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

            
            result.X = ((vector.X * ((1.0f - yy) - zz)) + (vector.Y * (xy - wz))) + (vector.Z * (xz + wy));
            result.Y = ((vector.X * (xy + wz)) + (vector.Y * ((1.0f - xx) - zz))) + (vector.Z * (yz - wx));
            result.Z = ((vector.X * (xz - wy)) + (vector.Y * (yz + wx))) + (vector.Z * ((1.0f - xx) - yy));
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static Vector3F Transform(ref Vector3F vector, ref QuaternionF rotation)
        {
            Transform(ref vector, ref rotation, out Vector3F result);
            return result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="QuaternionF"/> rotation.
        /// </summary>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="rotation">The <see cref="QuaternionF"/> rotation to apply.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector3F Transform(Vector3F vector, QuaternionF rotation)
        {
            Transform(ref vector, ref rotation, out Vector3F result);
            return result;
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
        public static void Transform(Vector3F[] source, ref QuaternionF rotation, Vector3F[] destination)
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
                destination[i] = new Vector3F(
                    ((source[i].X * num1) + (source[i].Y * num2)) + (source[i].Z * num3),
                    ((source[i].X * num4) + (source[i].Y * num5)) + (source[i].Z * num6),
                    ((source[i].X * num7) + (source[i].Y * num8)) + (source[i].Z * num9));
            }
        }


        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix3F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector3F"/>.</param>
        public static void Transform(ref Vector3F vector, ref Matrix3F transform, out Vector3F result)
        {
            result = new Vector3F()
            {
                X = (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31),
                Y = (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32),
                Z = (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33)
            };
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix3F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix3F"/>.</param>
        /// <returns>The transformed <see cref="Vector3F"/>.</returns>
        public static Vector3F Transform(Vector3F vector, Matrix3F transform)
        {
            Transform(ref vector, ref transform, out Vector3F result);
            return result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector3F"/>.</param>
        public static void Transform(ref Vector3F vector, ref Matrix4F transform, out Vector3F result)
        {
            Transform(ref vector, ref transform, out Vector4F v4Result);
            result = (Vector3F)v4Result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector3F"/>.</param>
        public static Vector3F Transform(ref Vector3F vector, ref Matrix4F transform)
        {
            Transform(ref vector, ref transform, out Vector4F result);
            return (Vector3F)result;
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed <see cref="Vector4F"/>.</param>
        public static void Transform(ref Vector3F vector, ref Matrix4F transform, out Vector4F result)
        {
            result = new Vector4F(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + transform.M43,
                (vector.X * transform.M14) + (vector.Y * transform.M24) + (vector.Z * transform.M34) + transform.M44);
        }

        /// <summary>
        /// Transforms a 3D vector by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <returns>The transformed <see cref="Vector4F"/>.</returns>
        public static Vector4F Transform(Vector3F vector, Matrix4F transform)
        {
            Transform(ref vector, ref transform, out Vector4F result);
            return result;
        }

        /// <summary>
        /// Transforms an array of 3D vectors by the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="source">The array of vectors to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="destination">The array for which the transformed vectors are stored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Transform(Vector3F[] source, ref Matrix4F transform, Vector4F[] destination)
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
        public static void TransformX(float x, ref QuaternionF rotation, out Vector3F result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;
            float xy2 = rotation.X * y2;
            float xz2 = rotation.X * z2;
            float yy2 = rotation.Y * y2;
            float zz2 = rotation.Z * z2;
            float wy2 = rotation.W * y2;
            float wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            float transformedX = x * (1f - yy2 - zz2);
            float transformedY = x * (xy2 + wz2);
            float transformedZ = x * (xz2 - wy2);
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
        public static void TransformY(float y, ref QuaternionF rotation, out Vector3F result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;
            float xx2 = rotation.X * x2;
            float xy2 = rotation.X * y2;
            float yz2 = rotation.Y * z2;
            float zz2 = rotation.Z * z2;
            float wx2 = rotation.W * x2;
            float wz2 = rotation.W * z2;
            //Defer the component setting since they're used in computation.
            float transformedX = y * (xy2 - wz2);
            float transformedY = y * (1f - xx2 - zz2);
            float transformedZ = y * (yz2 + wx2);
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
        public static void TransformZ(float z, ref QuaternionF rotation, out Vector3F result)
        {
            //This operation is an optimized-down version of v' = q * v * q^-1.
            //The expanded form would be to treat v as an 'axis only' quaternion
            //and perform standard quaternion multiplication.  Assuming q is normalized,
            //q^-1 can be replaced by a conjugation.
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;
            float xx2 = rotation.X * x2;
            float xz2 = rotation.X * z2;
            float yy2 = rotation.Y * y2;
            float yz2 = rotation.Y * z2;
            float wx2 = rotation.W * x2;
            float wy2 = rotation.W * y2;
            //Defer the component setting since they're used in computation.
            float transformedX = z * (xz2 + wy2);
            float transformedY = z * (yz2 - wx2);
            float transformedZ = z * (1f - xx2 - yy2);
            result.X = transformedX;
            result.Y = transformedY;
            result.Z = transformedZ;

        }

        /// <summary>
        /// Performs a coordinate transformation using the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <param name="result">When the method completes, contains the transformed coordinates.</param>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static void TransformCoordinate(ref Vector3F coordinate, ref Matrix4F transform, out Vector3F result)
        {
            Vector4F vector = new Vector4F();
            vector.X = (coordinate.X * transform.M11) + (coordinate.Y * transform.M21) + (coordinate.Z * transform.M31) + transform.M41;
            vector.Y = (coordinate.X * transform.M12) + (coordinate.Y * transform.M22) + (coordinate.Z * transform.M32) + transform.M42;
            vector.Z = (coordinate.X * transform.M13) + (coordinate.Y * transform.M23) + (coordinate.Z * transform.M33) + transform.M43;
            vector.W = 1f / ((coordinate.X * transform.M14) + (coordinate.Y * transform.M24) + (coordinate.Z * transform.M34) + transform.M44);

            result = new Vector3F(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W);
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
        public static Vector3F TransformCoordinate(ref Vector3F coordinate, ref Matrix4F transform)
        {
            TransformCoordinate(ref coordinate, ref transform, out Vector3F result);
            return result;
        }

        /// <summary>
        /// Performs a coordinate transformation using the given <see cref="Matrix4F"/>.
        /// </summary>
        /// <param name="coordinate">The coordinate vector to transform.</param>
        /// <param name="transform">The transformation <see cref="Matrix4F"/>.</param>
        /// <returns>The transformed coordinates.</returns>
        /// <remarks>
        /// A coordinate transform performs the transformation with the assumption that the w component
        /// is one. The four dimensional vector obtained from the transformation operation has each
        /// component in the vector divided by the w component. This forces the w component to be one and
        /// therefore makes the vector homogeneous. The homogeneous vector is often preferred when working
        /// with coordinates as the w component can safely be ignored.
        /// </remarks>
        public static Vector3F TransformCoordinate(Vector3F coordinate, Matrix4F transform)
        {
             TransformCoordinate(ref coordinate, ref transform, out Vector3F result);
             return result;
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
        public static void TransformCoordinate(Vector3F[] source, ref Matrix4F transform, Vector3F[] destination)
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
        public static void TransformNormal(ref Vector3F normal, ref Matrix4F transform, out Vector3F result)
        {
            result.X = (normal.X * transform.M11) + (normal.Y * transform.M21) + (normal.Z * transform.M31);
            result.Y = (normal.X * transform.M12) + (normal.Y * transform.M22) + (normal.Z * transform.M32);
            result.Z = (normal.X * transform.M13) + (normal.Y * transform.M23) + (normal.Z * transform.M33);
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
        public static Vector3F TransformNormal(ref Vector3F normal, ref Matrix4F transform)
        {
            TransformNormal(ref normal, ref transform, out Vector3F result);
            return result;
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
        public static Vector3F TransformNormal(Vector3F normal, Matrix4F transform)
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
        public static void TransformNormal(Vector3F[] source, ref Matrix4F transform, Vector3F[] destination)
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
        public static void BarycentricCoordinates(ref Vector3F p, ref Vector3F a, ref Vector3F b, ref Vector3F c,
            out float aWeight, out float bWeight, out float cWeight)
        {
            Vector3F ab, ac;
            Subtract(ref b, ref a, out ab);
            Subtract(ref c, ref a, out ac);
            Vector3F triangleNormal;
            Cross(ref ab, ref ac, out triangleNormal);
            float x = triangleNormal.X < 0 ? -triangleNormal.X : triangleNormal.X;
            float y = triangleNormal.Y < 0 ? -triangleNormal.Y : triangleNormal.Y;
            float z = triangleNormal.Z < 0 ? -triangleNormal.Z : triangleNormal.Z;

            float numeratorU, numeratorV, denominator;
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

            if (denominator < -1e-9F || denominator > 1e-9F)
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

                float distance1 = DistanceSquared(ref p, ref a);
                float distance2 = DistanceSquared(ref p, ref b);
                float distance3 = DistanceSquared(ref p, ref c);
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

