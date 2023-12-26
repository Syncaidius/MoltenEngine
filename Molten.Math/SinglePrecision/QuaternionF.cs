using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
	///<summary>Represents a four dimensional mathematical QuaternionF.</summary>
	[StructLayout(LayoutKind.Explicit)]
    [Serializable]
	public partial struct QuaternionF : IFormattable, IEquatable<QuaternionF>
	{
		/// <summary>
        /// The size of the <see cref="QuaternionF"/> type, in bytes.
        /// </summary>
        public static readonly int SizeInBytes = Marshal.SizeOf(typeof(QuaternionF));

		 /// <summary>
        /// A <see cref="QuaternionF"/> with all of its components set to zero.
        /// </summary>
        public static readonly QuaternionF Zero = new QuaternionF();

        /// <summary>
        /// A <see cref="QuaternionF"/> with all of its components set to one.
        /// </summary>
        public static readonly QuaternionF One = new QuaternionF(1F, 1F, 1F, 1F);

        /// <summary>
        /// The identity <see cref="QuaternionF"/> (0, 0, 0, 1).
        /// </summary>
        public static readonly QuaternionF Identity = new QuaternionF(0F, 0F, 0F, 0F);

		/// <summary>The X component.</summary>
		[DataMember]
		[FieldOffset(0)]
		public float X;

		/// <summary>The Y component.</summary>
		[DataMember]
		[FieldOffset(4)]
		public float Y;

		/// <summary>The Z component.</summary>
		[DataMember]
		[FieldOffset(8)]
		public float Z;

		/// <summary>The W component.</summary>
		[DataMember]
		[FieldOffset(12)]
		public float W;

		/// <summary>A fixed array mapped to the same memory space as the individual <see cref="QuaternionF"/> components.</summary>
		[IgnoreDataMember]
		[FieldOffset(0)]
		public unsafe fixed float Values[4];


        /// <summary>
        /// Gets a value indicting whether this  <see cref="QuaternionF"/> is normalized.
        /// </summary>
        public bool IsNormalized => MathHelper.IsOne((X * X) + (Y * Y) + (Z * Z) + (W * W));

        /// <summary>
        /// Gets the angle of the  <see cref="QuaternionF"/>.
        /// </summary>
        /// <value>The quaternion's angle.</value>
        public float Angle
        {
            get
            {
                float length = (X * X) + (Y * Y) + (Z * Z);
                if (MathHelper.IsZero(length))
                    return 0.0F;

                return 2.0F * MathF.Acos(float.Clamp(W, -1F, 1F));
            }
        }

        /// <summary>
        /// Gets the axis components of the QuaternionF.
        /// </summary>
        /// <value>The axis components of the QuaternionF.</value>
        public Vector3F Axis
        {
            get
            {
                float length = (X * X) + (Y * Y) + (Z * Z);
                if (MathHelper.IsZero(length))
                    return Vector3F.UnitX;

                float inv = 1.0F  / MathF.Sqrt(length);
                return new Vector3F(X * inv, Y * inv, Z * inv);
            }
        }

#region Constructors
		/// <summary>
		/// Initializes a new instance of <see cref="QuaternionF"/>.
		/// </summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		/// <param name="w">The W component.</param>
		public QuaternionF(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
		/// <summary>Initializes a new instance of <see cref="QuaternionF"/>.</summary>
		/// <param name="value">The value that will be assigned to all components.</param>
		public QuaternionF(float value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}
		/// <summary>Initializes a new instance of <see cref="QuaternionF"/> from an array.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least 4 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 4 elements.</exception>
		public unsafe QuaternionF(float[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least 4 input values for QuaternionF.");

			fixed (float* src = values)
			{
				fixed (float* dst = Values)
					Unsafe.CopyBlock(src, dst, (sizeof(float) * 4));
			}
		}
		/// <summary>Initializes a new instance of <see cref="QuaternionF"/> from a span.</summary>
		/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least 4 elements.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 4 elements.</exception>
		public QuaternionF(Span<float> values)
		{
			if (values == null)
				throw new ArgumentNullException("values");
			if (values.Length < 4)
				throw new ArgumentOutOfRangeException("values", "There must be at least 4 input values for QuaternionF.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
		}
		/// <summary>Initializes a new instance of <see cref="QuaternionF"/> from a an unsafe pointer.</summary>
		/// <param name="ptrValues">The values to assign to the X, Y, Z, W components of the color.
		/// <para>There must be at least 4 elements available or undefined behaviour will occur.</para></param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 4 elements.</exception>
		public unsafe QuaternionF(float* ptrValues)
		{
			if (ptrValues == null)
				throw new ArgumentNullException("ptrValues");

			fixed (float* dst = Values)
				Unsafe.CopyBlock(ptrValues, dst, (sizeof(float) * 4));
		}
        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionF"/> struct.
        /// </summary>
        /// <param name="value">A QuaternionF containing the values with which to initialize the components.</param>
        public QuaternionF(Vector4F value)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = value.W;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionF"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X, Y, and Z components.</param>
        /// <param name="w">Initial value for the W component of the <see cref="QuaternionF"/>.</param>
        public QuaternionF(Vector3F value, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuaternionF"/> struct.
        /// </summary>
        /// <param name="value">A vector containing the values with which to initialize the X and Y components.</param>
        /// <param name="z">Initial value for the Z component of the <see cref="QuaternionF"/>.</param>
        /// <param name="w">Initial value for the W component of the <see cref="QuaternionF"/>.</param>
        public QuaternionF(Vector2F value, float z, float w)
        {
            X = value.X;
            Y = value.Y;
            Z = z;
            W = w;
        }
#endregion

#region Instance Methods
/// <summary>
        /// Gets a value indicating whether this instance is equivalent to the identity QuaternionF.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity QuaternionF; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity
        {
            get { return this.Equals(Identity); }
        }

        /// <summary>
        /// Conjugates the <see cref="QuaternionF"/> X,Y and Z components. W component is not changed.
        /// </summary>
        public void Conjugate()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
        }

        /// <summary>
        /// Conjugates the <see cref="QuaternionF"/>.
        /// </summary>
        public void Abs()
        {
            X = Math.Abs(X);
            Y = Math.Abs(Y);
            Z = Math.Abs(Z);
            W = Math.Abs(W);
        }

        /// <summary>
        /// Calculates the squared length of the <see cref="QuaternionF"/>.
        /// </summary>
        /// <returns>The squared length of the <see cref="QuaternionF"/>.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="QuaternionF.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public float LengthSquared()
        {
            return (X * X) + (Y * Y) + (Z * Z) + (W * W);
        }

        /// <summary>
        /// Creates an array containing the elements of the <see cref="QuaternionF"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the <see cref="QuaternionF"/>.</returns>
        public float[] ToArray()
        {
            return new float[] { X, Y, Z, W };
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Computes the quaternion rotation between two normalized vectors.
        /// </summary>
        /// <param name="v1">First unit-length vector.</param>
        /// <param name="v2">Second unit-length vector.</param>
        /// <param name="q">Quaternion representing the rotation from v1 to v2.</param>
        public static QuaternionF GetQuaternionBetweenNormalizedVectors(ref Vector3F v1, ref Vector3F v2)
        {
            QuaternionF q;
            float dot = Vector3F.Dot(ref v1, ref v2);
            //For non-normal vectors, the multiplying the axes length squared would be necessary:
            //float w = dot + MathF.Sqrt(v1.LengthSquared() * v2.LengthSquared());
            if (dot < -0.9999F) //parallel, opposing direction
            {
                //If this occurs, the rotation required is ~180 degrees.
                //The problem is that we could choose any perpendicular axis for the rotation. It's not uniquely defined.
                //The solution is to pick an arbitrary perpendicular axis.
                //Project onto the plane which has the lowest component magnitude.
                //On that 2d plane, perform a 90 degree rotation.
                float absX = Math.Abs(v1.X);
                float absY = Math.Abs(v1.Y);
                float absZ = Math.Abs(v1.Z);
                if (absX < absY && absX < absZ)
                    q = new QuaternionF(0, -v1.Z, v1.Y, 0);
                else if (absY < absZ)
                    q = new QuaternionF(-v1.Z, 0, v1.X, 0);
                else
                    q = new QuaternionF(-v1.Y, v1.X, 0, 0);
            }
            else
            {
                Vector3F axis = Vector3F.Cross(ref v1, ref v2);
                q = new QuaternionF(axis.X, axis.Y, axis.Z, dot + 1);
            }
            q.Normalize();

            return q;
        }

        /// <summary>
        /// Converts the <see cref="QuaternionF"/> into a unit quaternion.
        /// </summary>
        public void Normalize()
        {
            float length = Length();
            if (!MathHelper.IsZero(length))
            {
                float inverse = 1.0F / length;
                X *= inverse;
                Y *= inverse;
                Z *= inverse;
                W *= inverse;
            }
        }

        /// <summary>
        /// Calculates the length of the QuaternionF.
        /// </summary>
        /// <returns>The length of the QuaternionF.</returns>
        /// <remarks>
        /// <see cref="QuaternionF.LengthSquared"/> may be preferred when only the relative length is needed and speed is of the essence.
        /// </remarks>
        public float Length()
        {
            return (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

        /// <summary>
        /// Conjugates and renormalizes the QuaternionF.
        /// </summary>
        public void Invert()
        {
            float lengthSq = LengthSquared();
            if (!MathHelper.IsZero(lengthSq))
            {
                lengthSq = 1.0F / lengthSq;

                X = -X * lengthSq;
                Y = -Y * lengthSq;
                Z = -Z * lengthSq;
                W = W * lengthSq;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="QuaternionF"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="QuaternionF"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="QuaternionF"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref QuaternionF other)
        {
            return MathHelper.NearEqual(other.X, X) && MathHelper.NearEqual(other.Y, Y) && MathHelper.NearEqual(other.Z, Z) && MathHelper.NearEqual(other.W, W);
        }

        /// <summary>
        /// Determines whether the specified <see cref="QuaternionF"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="QuaternionF"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="QuaternionF"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(QuaternionF other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is QuaternionF))
                return false;

            var strongValue = (QuaternionF)value;
            return Equals(ref strongValue);
        }
#endregion

#region To-String Methods
/// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, CultureInfo.CurrentCulture),
                Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
#endregion

#region Static Methods
        /// <summary>
        /// Multiplies two <see cref="QuaternionF"/> together in opposite order.
        /// </summary>
        /// <param name="a">First <see cref="QuaternionF"/> to multiply.</param>
        /// <param name="b">Second <see cref="QuaternionF"/> to multiply.</param>
        public static void Concatenate(ref QuaternionF a, ref QuaternionF b, out QuaternionF result)
        {
            float aX = a.X;
            float aY = a.Y;
            float aZ = a.Z;
            float aW = a.W;
            float bX = b.X;
            float bY = b.Y;
            float bZ = b.Z;
            float bW = b.W;

            result.X = aW * bX + aX * bW + aZ * bY - aY * bZ;
            result.Y = aW * bY + aY * bW + aX * bZ - aZ * bX;
            result.Z = aW * bZ + aZ * bW + aY * bX - aX * bY;
            result.W = aW * bW - aX * bX - aY * bY - aZ * bZ;
        }

        /// <summary>
        /// Multiplies two <see cref="QuaternionF"/> together in opposite order.
        /// </summary>
        /// <param name="a">First <see cref="QuaternionF"/> to multiply.</param>
        /// <param name="b">Second <see cref="QuaternionF"/> to multiply.</param>
        public static QuaternionF Concatenate(ref QuaternionF a, ref QuaternionF b)
        {
            Concatenate(ref a, ref b, out QuaternionF result);
            return result;
        }

        /// <summary>
        /// Computes the angle change represented by a normalized quaternion.
        /// </summary>
        /// <param name="q">Quaternion to be converted.</param>
        /// <returns>Angle around the axis represented by the quaternion.</returns>
        public static float GetAngleFromQuaternion(ref QuaternionF q)
        {
            float qw = Math.Abs(q.W);
            if (qw > 1)
                return 0;
            return 2 * (float)Math.Acos(qw);
        }

        /// <summary>
        /// Calculates the natural logarithm of the specified quaternion.
        /// </summary>
        /// <param name="value">The quaternion whose logarithm will be calculated.</param>
        /// <param name="result">When the method completes, contains the natural logarithm of the quaternion.</param>
        public static QuaternionF Logarithm(ref QuaternionF value)
        {
            QuaternionF result;

            if (Math.Abs(value.W) < 1.0)
            {
                float angle = (float)Math.Acos(value.W);
                float sin = (float)Math.Sin(angle);

                if (!MathHelper.IsZero(sin))
                {
                    float coeff = angle / sin;
                    result.X = value.X * coeff;
                    result.Y = value.Y * coeff;
                    result.Z = value.Z * coeff;
                }
                else
                {
                    result = value;
                }
            }
            else
            {
                result = value;
            }

            result.W = 0.0F;
            return result;
        }

        /// <summary>
        /// Calculates the natural logarithm of the specified quaternion.
        /// </summary>
        /// <param name="value">The quaternion whose logarithm will be calculated.</param>
        /// <param name="result">When the method completes, contains the natural logarithm of the quaternion.</param>
        public static QuaternionF Logarithm(QuaternionF value)
        {
            return Logarithm(ref value);
        }

        /// <summary>
        /// Computes the axis angle representation of a normalized <see cref="QuaternionF"/>.
        /// </summary>
        /// <param name="q"><see cref="QuaternionF"/> to be converted.</param>
        /// <param name="axis">Axis represented by the <see cref="QuaternionF"/>.</param>
        /// <param name="angle">Angle around the axis represented by the <see cref="QuaternionF"/>.</param>
        public static void GetAxisAngle(ref QuaternionF q, out Vector3F axis, out float angle)
        {
            float qw = q.W;
            if (qw > 0)
            {
                axis.X = q.X;
                axis.Y = q.Y;
                axis.Z = q.Z;
            }
            else
            {
                axis.X = -q.X;
                axis.Y = -q.Y;
                axis.Z = -q.Z;
                qw = -qw;
            }

            float lengthSquared = axis.LengthSquared();
            if (lengthSquared > 1e-14f)
            {
                axis = axis / MathF.Sqrt(lengthSquared);
                angle = 2F * MathF.Acos(float.Clamp(qw, -1, 1));
            }
            else
            {
                axis = Vector3F.Up;
                angle = 0;
            }
        }

        /// <summary>
        /// Sets up control points for spherical quadrangle interpolation.
        /// </summary>
        /// <param name="value1">First source <see cref="QuaternionF"/>.</param>
        /// <param name="value2">Second source <see cref="QuaternionF"/>.</param>
        /// <param name="value3">Third source <see cref="QuaternionF"/>.</param>
        /// <param name="value4">Fourth source <see cref="QuaternionF"/>.</param>
        /// <returns>An array of three <see cref="QuaternionF"/> that represent control points for spherical quadrangle interpolation.</returns>
        public static QuaternionF[] SquadSetup(ref QuaternionF value1, ref QuaternionF value2, ref QuaternionF value3, ref QuaternionF value4)
        {
            QuaternionF q0 = (value1 + value2).LengthSquared() < (value1 - value2).LengthSquared() ? -value1 : value1;
            QuaternionF q2 = (value2 + value3).LengthSquared() < (value2 - value3).LengthSquared() ? -value3 : value3;
            QuaternionF q3 = (value3 + value4).LengthSquared() < (value3 - value4).LengthSquared() ? -value4 : value4;
            QuaternionF q1 = value2;

            QuaternionF q1Exp = Exponential(ref q1);
            QuaternionF q2Exp = Exponential(ref q2);

            QuaternionF[] results = new QuaternionF[3];
            results[0] = q1 * Exponential(-0.25f * (Logarithm(q1Exp * q2) + Logarithm(q1Exp * q0)));
            results[1] = q2 * Exponential(-0.25f * (Logarithm(q2Exp * q3) + Logarithm(q2Exp * q1)));
            results[2] = q2;

            return results;
        }

        /// <summary>
        /// Interpolates between <see cref="QuaternionF"/>, using spherical quadrangle interpolation.
        /// </summary>
        /// <param name="value1">First source <see cref="QuaternionF"/>.</param>
        /// <param name="value2">Second source <see cref="QuaternionF"/>.</param>
        /// <param name="value3">Third source <see cref="QuaternionF"/>.</param>
        /// <param name="value4">Fourth source <see cref="QuaternionF"/>.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of interpolation.</param>
        /// <param name="result">When the method completes, contains the spherical quadrangle interpolation of the <see cref="QuaternionF"/>.</param>
        public static QuaternionF Squad(ref QuaternionF value1, ref QuaternionF value2, ref QuaternionF value3, ref QuaternionF value4, float amount)
        {
            QuaternionF start = Slerp(ref value1, ref value4, amount);
            QuaternionF end = Slerp(ref value2, ref value3, amount);
            return Slerp(ref start, ref end, 2.0F * amount * (1.0F - amount));
        }

        /// <summary>
        /// Interpolates between two <see cref="QuaternionF"/>, using spherical linear interpolation.
        /// </summary>
        /// <param name="start">Start <see cref="QuaternionF"/>.</param>
        /// <param name="end">End <see cref="QuaternionF"/>.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the spherical linear interpolation of the two <see cref="QuaternionF"/>.</param>
        public static QuaternionF Slerp(ref QuaternionF start, ref QuaternionF end, float amount)
        {
            float opposite;
            float inverse;
            float dot = Dot(start, end);

            if (MathF.Abs(dot) > 1.0F - MathHelper.Constants<float>.ZeroTolerance)
            {
                inverse = 1.0F - amount;
                opposite = amount * MathF.Sign(dot);
            }
            else
            {
                float acos = MathF.Acos(Math.Abs(dot));
                float invSin = 1.0F / MathF.Sin(acos);

                inverse = MathF.Sin((1.0F - amount) * acos) * invSin;
                opposite = MathF.Sin(amount * acos) * invSin * Math.Sign(dot);
            }

            return new QuaternionF()
            {
                X = (inverse * start.X) + (opposite * end.X),
                Y = (inverse * start.Y) + (opposite * end.Y),
                Z = (inverse * start.Z) + (opposite * end.Z),
                W = (inverse * start.W) + (opposite * end.W)
            };
        }

        /// <summary>
        /// Creates a quaternion given a yaw, pitch, and roll value.
        /// </summary>
        /// <param name="yaw">The yaw of rotation.</param>
        /// <param name="pitch">The pitch of rotation.</param>
        /// <param name="roll">The roll of rotation.</param>
        public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out QuaternionF result)
        {
            float halfRoll = roll * 0.5f;
            float halfPitch = pitch * 0.5f;
            float halfYaw = yaw * 0.5f;

            float sinRoll = (float)Math.Sin(halfRoll);
            float cosRoll = (float)Math.Cos(halfRoll);
            float sinPitch = (float)Math.Sin(halfPitch);
            float cosPitch = (float)Math.Cos(halfPitch);
            float sinYaw = (float)Math.Sin(halfYaw);
            float cosYaw = (float)Math.Cos(halfYaw);

            result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);
        }

        /// <summary>
        /// Creates a quaternion given a yaw, pitch, and roll value.
        /// </summary>
        /// <param name="yaw">The yaw of rotation.</param>
        /// <param name="pitch">The pitch of rotation.</param>
        /// <param name="roll">The roll of rotation.</param>
        public static QuaternionF RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            float halfRoll = roll * 0.5f;
            float halfPitch = pitch * 0.5f;
            float halfYaw = yaw * 0.5f;

            float sinRoll = (float)Math.Sin(halfRoll);
            float cosRoll = (float)Math.Cos(halfRoll);
            float sinPitch = (float)Math.Sin(halfPitch);
            float cosPitch = (float)Math.Cos(halfPitch);
            float sinYaw = (float)Math.Sin(halfYaw);
            float cosYaw = (float)Math.Cos(halfYaw);

            return new QuaternionF()
            {
                X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll),
                Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll),
                Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll),
                W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll)
            };
        }

        /// <summary>
        /// Creates a <see cref="QuaternionF"/> given a rotation matrix.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        public static QuaternionF FromRotationMatrix(ref Matrix4F matrix)
        {
            float sqrt;
            float half;
            float scale = matrix.M11 + matrix.M22 + matrix.M33;
            QuaternionF result;

            if (scale > 0.0F)
            {
                sqrt = (float)Math.Sqrt(scale + 1.0F);
                result.W = sqrt * 0.5F;
                sqrt = 0.5F / sqrt;

                result.X = (matrix.M23 - matrix.M32) * sqrt;
                result.Y = (matrix.M31 - matrix.M13) * sqrt;
                result.Z = (matrix.M12 - matrix.M21) * sqrt;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                sqrt = (float)Math.Sqrt(1.0F + matrix.M11 - matrix.M22 - matrix.M33);
                half = 0.5F / sqrt;

                result.X = 0.5F * sqrt;
                result.Y = (matrix.M12 + matrix.M21) * half;
                result.Z = (matrix.M13 + matrix.M31) * half;
                result.W = (matrix.M23 - matrix.M32) * half;
            }
            else if (matrix.M22 > matrix.M33)
            {
                sqrt = (float)Math.Sqrt(1.0F + matrix.M22 - matrix.M11 - matrix.M33);
                half = 0.5f / sqrt;

                result.X = (matrix.M21 + matrix.M12) * half;
                result.Y = 0.5F * sqrt;
                result.Z = (matrix.M32 + matrix.M23) * half;
                result.W = (matrix.M31 - matrix.M13) * half;
            }
            else
            {
                sqrt = (float)Math.Sqrt(1.0F  + matrix.M33 - matrix.M11 - matrix.M22);
                half = 0.5F  / sqrt;

                result.X = (matrix.M31 + matrix.M13) * half;
                result.Y = (matrix.M32 + matrix.M23) * half;
                result.Z = 0.5F * sqrt;
                result.W = (matrix.M12 - matrix.M21) * half;
            }

            return result;
        }

        /// <summary>
        /// Creates a quaternion given a rotation matrix.
        /// </summary>
        /// <param name="matrix">The rotation matrix.</param>
        public static QuaternionF FromRotationMatrix(ref Matrix3F matrix)
        {
            float sqrt;
            float half;
            float scale = matrix.M11 + matrix.M22 + matrix.M33;
            QuaternionF result;

            if (scale > 0.0f)
            {
                sqrt = (float)Math.Sqrt(scale + 1.0f);
                result.W = sqrt * 0.5F;
                sqrt = 0.5F / sqrt;

                result.X = (matrix.M23 - matrix.M32) * sqrt;
                result.Y = (matrix.M31 - matrix.M13) * sqrt;
                result.Z = (matrix.M12 - matrix.M21) * sqrt;
            }
            else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
            {
                sqrt = (float)Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
                half = 0.5F / sqrt;

                result.X = 0.5F * sqrt;
                result.Y = (matrix.M12 + matrix.M21) * half;
                result.Z = (matrix.M13 + matrix.M31) * half;
                result.W = (matrix.M23 - matrix.M32) * half;
            }
            else if (matrix.M22 > matrix.M33)
            {
                sqrt = (float)Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
                half = 0.5F / sqrt;

                result.X = (matrix.M21 + matrix.M12) * half;
                result.Y = 0.5F * sqrt;
                result.Z = (matrix.M32 + matrix.M23) * half;
                result.W = (matrix.M31 - matrix.M13) * half;
            }
            else
            {
                sqrt = (float)Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
                half = 0.5F / sqrt;

                result.X = (matrix.M31 + matrix.M13) * half;
                result.Y = (matrix.M32 + matrix.M23) * half;
                result.Z = 0.5F * sqrt;
                result.W = (matrix.M12 - matrix.M21) * half;
            }

            return result;
        }

        /// <summary>
        /// Calculates the dot product of two quaternions.
        /// </summary>
        /// <param name="left">First source quaternion.</param>
        /// <param name="right">Second source quaternion.</param>
        public static float Dot(ref QuaternionF left, ref QuaternionF right)
        {
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="QuaternionF"/>.
        /// </summary>
        /// <param name="left">First source <see cref="QuaternionF"/>.</param>
        /// <param name="right">Second source <see cref="QuaternionF"/>.</param>
        /// <returns>The dot product of the two <see cref="QuaternionF"/>.</returns>
        public static float Dot(QuaternionF left, QuaternionF right)
        {
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

        /// <summary>
        /// Exponentiates a <see cref="QuaternionF"/>.
        /// </summary>
        /// <param name="value">The <see cref="QuaternionF"/> to exponentiate.</param>
        public static QuaternionF Exponential(ref QuaternionF value)
        {
            float angle = float.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
            float sin = float.Sin(angle);
            QuaternionF result;

            if (!MathHelper.IsZero(sin))
            {
                float coeff = sin / angle;
                result.X = coeff * value.X;
                result.Y = coeff * value.Y;
                result.Z = coeff * value.Z;
            }
            else
            {
                result = value;
            }

            result.W = float.Cos(angle);
            return result;
        }

        /// <summary>
        /// Exponentiates a <see cref="QuaternionF"/>.
        /// </summary>
        /// <param name="value">The <see cref="QuaternionF"/> to exponentiate.</param>
        public static QuaternionF Exponential(QuaternionF value)
        {
            return Exponential(ref value);
        }

        /// <summary>
        /// Returns a <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static QuaternionF Barycentric(ref QuaternionF value1, ref QuaternionF value2, ref QuaternionF value3, float amount1, float amount2)
        {
            QuaternionF start = Slerp(ref value1, ref value2, amount1 + amount2);
            QuaternionF end = Slerp(ref value1, ref value3, amount1 + amount2);
            return Slerp(ref start, ref end, amount2 / (amount1 + amount2));
        }

        /// <summary>
        /// Returns a <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="QuaternionF"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static QuaternionF Barycentric(QuaternionF value1, QuaternionF value2, QuaternionF value3, float amount1, float amount2)
        {
            return Barycentric(ref value1, ref value2, ref value3, amount1, amount2);
        }

        /// <summary>
        /// Computes the conjugate of the <see cref="QuaternionF"/> .
        /// </summary>
        /// <param name="value"><see cref="QuaternionF"/> to conjugate.</param>
        public static QuaternionF Conjugate(ref QuaternionF value)
        {
            return new QuaternionF()
            {
                X = -value.X,
                Y = -value.Y,
                Z = -value.Z,
                W = value.W
            };
        }

        /// <summary>
        /// Computes the conjugate of the <see cref="QuaternionF"/> .
        /// </summary>
        /// <param name="value"><see cref="QuaternionF"/> to conjugate.</param>
        public static void Conjugate(ref QuaternionF value, out QuaternionF result)
        {
            result.X = -value.X;
            result.Y = -value.Y;
            result.Z = -value.Z;
            result.W = value.W;
        }

        /// <summary>
        /// Computes the conjugate of the <see cref="QuaternionF"/> .
        /// </summary>
        /// <param name="value"><see cref="QuaternionF"/> to conjugate.</param>
        public static QuaternionF Conjugate(QuaternionF value)
        {
            return new QuaternionF()
            {
                X = -value.X,
                Y = -value.Y,
                Z = -value.Z,
                W = value.W
            };
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="QuaternionF"/>.
        /// </summary>
        /// <param name="start">Start <see cref="QuaternionF"/>.</param>
        /// <param name="end">End <see cref="QuaternionF"/>.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// This method performs the linear interpolation based on the following formula.
        /// <code>start + (end - start) * amount</code>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static QuaternionF Lerp(ref QuaternionF start, ref QuaternionF end, float amount)
        {
            float inverse = 1.0F - amount;
            QuaternionF result;

            if (Dot(start, end) >= 0.0F)
            {
                result.X = (inverse * start.X) + (amount * end.X);
                result.Y = (inverse * start.Y) + (amount * end.Y);
                result.Z = (inverse * start.Z) + (amount * end.Z);
                result.W = (inverse * start.W) + (amount * end.W);
            }
            else
            {
                result.X = (inverse * start.X) - (amount * end.X);
                result.Y = (inverse * start.Y) - (amount * end.Y);
                result.Z = (inverse * start.Z) - (amount * end.Z);
                result.W = (inverse * start.W) - (amount * end.W);
            }

            result.Normalize();
            return result;
        }

        /// <summary>
        /// Creates a <see cref="QuaternionF"/> given a rotation and an axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation.</param>
        public static QuaternionF FromAxisAngle(Vector3F axis, float angle)
        {
            return FromAxisAngle(ref axis, angle);
        }

        /// <summary>
        /// Creates a <see cref="QuaternionF"/> given a rotation and an axis.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation.</param>
        public static QuaternionF FromAxisAngle(ref Vector3F axis, float angle)
        {
            Vector3F normalized = axis.GetNormalized();

            float half = angle * 0.5F;
            float sin = (float)Math.Sin(half);
            float cos = (float)Math.Cos(half);

            return new QuaternionF()
            {
                X = normalized.X * sin,
                Y = normalized.Y * sin,
                Z = normalized.Z * sin,
                W = cos     
            };
        }

        /// <summary>
        /// Creates a left-handed, look-at <see cref="QuaternionF"/>.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        public static QuaternionF LookAtLH(ref Vector3F eye, ref Vector3F target, ref Vector3F up)
        {
            Matrix3F matrix = Matrix3F.LookAtLH(ref eye, ref target, ref up);
            return FromRotationMatrix(ref matrix);
        }

        /// <summary>
        /// Creates a left-handed, look-at quaternion.
        /// </summary>
        /// <param name="forward">The camera's forward direction.</param>
        /// <param name="up">The camera's up vector.</param>
        /// <returns>The created look-at quaternion.</returns>
        public static QuaternionF RotationLookAtLH(Vector3F forward, Vector3F up)
        {
            return RotationLookAtLH(ref forward, ref up);
        }

        /// <summary>
        /// Creates a left-handed, look-at <see cref="QuaternionF"/>.
        /// </summary>
        /// <param name="forward">The camera's forward direction.</param>
        /// <param name="up">The camera's up vector.</param>
        public static QuaternionF  RotationLookAtLH(ref Vector3F forward, ref Vector3F up)
        {
            Vector3F eye = Vector3F.Zero;
            return LookAtLH(ref eye, ref forward, ref up);
        }

        /// <summary>
        /// Creates a right-handed, look-at quaternion.
        /// </summary>
        /// <param name="eye">The position of the viewer's eye.</param>
        /// <param name="target">The camera look-at target.</param>
        /// <param name="up">The camera's up vector.</param>
        public static QuaternionF LookAtRH(ref Vector3F eye, ref Vector3F target, ref Vector3F up)
        {
            Matrix3F matrix = Matrix3F.LookAtRH(ref eye, ref target, ref up);
            return FromRotationMatrix(ref matrix);
        }

        /// <summary>
        /// Creates a right-handed, look-at quaternion.
        /// </summary>
        /// <param name="forward">The camera's forward direction.</param>
        /// <param name="up">The camera's up vector.</param>
        public static QuaternionF RotationLookAtRH(ref Vector3F forward, ref Vector3F up)
        {
            Vector3F eye = Vector3F.Zero;
            return LookAtRH(ref eye, ref forward, ref up);
        }

        /// <summary>
        /// Creates a left-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        public static QuaternionF BillboardLH(ref Vector3F objectPosition, ref Vector3F cameraPosition, ref Vector3F cameraUpVector, ref Vector3F cameraForwardVector)
        {
            Matrix3F matrix = Matrix3F.BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector);
            return FromRotationMatrix(ref matrix);
        }

        /// <summary>
        /// Creates a right-handed spherical billboard that rotates around a specified object position.
        /// </summary>
        /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
        /// <param name="cameraPosition">The position of the camera.</param>
        /// <param name="cameraUpVector">The up vector of the camera.</param>
        /// <param name="cameraForwardVector">The forward vector of the camera.</param>
        public static QuaternionF BillboardRH(ref Vector3F objectPosition, ref Vector3F cameraPosition, ref Vector3F cameraUpVector, ref Vector3F cameraForwardVector)
        {
            Matrix3F matrix = Matrix3F.BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector);
            return FromRotationMatrix(ref matrix);
        }
#endregion

#region Operators - Multiply
        /// <summary>
        /// Scales a quaternion by the given value.
        /// </summary>
        /// <param name="value">The quaternion to scale.</param>
        /// <param name="scale">The amount by which to scale the quaternion.</param>
        /// <returns>The scaled quaternion.</returns>
        public static QuaternionF operator *(float scale, QuaternionF value)
        {
            return new QuaternionF()
            {
                X = value.X * scale,
                Y = value.Y * scale,
                Z = value.Z * scale,
                W = value.W * scale,
            };
        }

        /// <summary>
        /// Scales a quaternion by the given value.
        /// </summary>
        /// <param name="value">The quaternion to scale.</param>
        /// <param name="scale">The amount by which to scale the quaternion.</param>
        /// <returns>The scaled quaternion.</returns>
        public static QuaternionF operator *(QuaternionF value, float scale)
        {
            return new QuaternionF()
            {
                X = value.X * scale,
                Y = value.Y * scale,
                Z = value.Z * scale,
                W = value.W * scale,
            };
        }

        /// <summary>
        /// Multiplies a QuaternionF by another QuaternionF.
        /// </summary>
        /// <param name="left">A reference to the first QuaternionF to multiply.</param>
        /// <param name="right">A reference to the second QuaternionF to multiply.</param>
        /// <param name="result">An output to store the result.</param>
        /// <returns>The multiplied QuaternionF.</returns>
        public static void Multiply(ref QuaternionF left, ref QuaternionF right, out QuaternionF result)
        {
            float lx = left.X;
            float ly = left.Y;
            float lz = left.Z;
            float lw = left.W;
            float rx = right.X;
            float ry = right.Y;
            float rz = right.Z;
            float rw = right.W;
            float a = (ly * rz - lz * ry);
            float b = (lz * rx - lx * rz);
            float c = (lx * ry - ly * rx);
            float d = (lx * rx + ly * ry + lz * rz);

            result.X = (lx * rw + rx * lw) + a;
            result.Y = (ly * rw + ry * lw) + b;
            result.Z = (lz * rw + rz * lw) + c;
            result.W = lw * rw - d;
        }

        /// <summary>
        /// Multiplies a QuaternionF by another QuaternionF.
        /// </summary>
        /// <param name="left">The first QuaternionF to multiply.</param>
        /// <param name="right">The second QuaternionF to multiply.</param>
        /// <returns>The multiplied QuaternionF.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionF operator *(QuaternionF left, QuaternionF right)
        {
            Multiply(ref left, ref right, out QuaternionF result);
            return result;
        }
#endregion

#region Operators - Subtract
		///<summary>Performs a subtract operation on two <see cref="QuaternionF"/>.</summary>
		///<param name="a">The first <see cref="QuaternionF"/> to subtract.</param>
		///<param name="b">The second <see cref="QuaternionF"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref QuaternionF a, ref QuaternionF b, out QuaternionF result)
		{
			result.X = a.X - b.X;
			result.Y = a.Y - b.Y;
			result.Z = a.Z - b.Z;
			result.W = a.W - b.W;
		}

		///<summary>Performs a subtract operation on two <see cref="QuaternionF"/>.</summary>
		///<param name="a">The first <see cref="QuaternionF"/> to subtract.</param>
		///<param name="b">The second <see cref="QuaternionF"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator -(QuaternionF a, QuaternionF b)
		{
			Subtract(ref a, ref b, out QuaternionF result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="QuaternionF"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="QuaternionF"/> to subtract.</param>
		///<param name="b">The <see cref="float"/> to subtract.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Subtract(ref QuaternionF a, float b, out QuaternionF result)
		{
			result.X = a.X - b;
			result.Y = a.Y - b;
			result.Z = a.Z - b;
			result.W = a.W - b;
		}

		///<summary>Performs a subtract operation on a <see cref="QuaternionF"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="QuaternionF"/> to subtract.</param>
		///<param name="b">The <see cref="float"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator -(QuaternionF a, float b)
		{
			Subtract(ref a, b, out QuaternionF result);
			return result;
		}

		///<summary>Performs a subtract operation on a <see cref="float"/> and a <see cref="QuaternionF"/>.</summary>
		///<param name="a">The <see cref="float"/> to subtract.</param>
		///<param name="b">The <see cref="QuaternionF"/> to subtract.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator -(float a, QuaternionF b)
		{
			Subtract(ref b, a, out QuaternionF result);
			return result;
		}


        /// <summary>
        /// Reverses the direction of a given quaternion.
        /// </summary>
        /// <param name="value">The quaternion to negate.</param>
        /// <returns>A quaternion facing in the opposite direction.</returns>
        public static QuaternionF operator -(QuaternionF value)
        {
            return new QuaternionF()
            {
                X = -value.X,
                Y = -value.Y,
                Z = -value.Z,
                W = -value.W
            };
        }
#endregion

#region Operators - Division
		///<summary>Performs a divide operation on two <see cref="QuaternionF"/>.</summary>
		///<param name="a">The first <see cref="QuaternionF"/> to divide.</param>
		///<param name="b">The second <see cref="QuaternionF"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref QuaternionF a, ref QuaternionF b, out QuaternionF result)
		{
			result.X = a.X / b.X;
			result.Y = a.Y / b.Y;
			result.Z = a.Z / b.Z;
			result.W = a.W / b.W;
		}

		///<summary>Performs a divide operation on two <see cref="QuaternionF"/>.</summary>
		///<param name="a">The first <see cref="QuaternionF"/> to divide.</param>
		///<param name="b">The second <see cref="QuaternionF"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator /(QuaternionF a, QuaternionF b)
		{
			Divide(ref a, ref b, out QuaternionF result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="QuaternionF"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="QuaternionF"/> to divide.</param>
		///<param name="b">The <see cref="float"/> to divide.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Divide(ref QuaternionF a, float b, out QuaternionF result)
		{
			result.X = a.X / b;
			result.Y = a.Y / b;
			result.Z = a.Z / b;
			result.W = a.W / b;
		}

		///<summary>Performs a divide operation on a <see cref="QuaternionF"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="QuaternionF"/> to divide.</param>
		///<param name="b">The <see cref="float"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator /(QuaternionF a, float b)
		{
			Divide(ref a, b, out QuaternionF result);
			return result;
		}

		///<summary>Performs a divide operation on a <see cref="float"/> and a <see cref="QuaternionF"/>.</summary>
		///<param name="a">The <see cref="float"/> to divide.</param>
		///<param name="b">The <see cref="QuaternionF"/> to divide.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator /(float a, QuaternionF b)
		{
			Divide(ref b, a, out QuaternionF result);
			return result;
		}

#endregion

#region Operators - Add
		///<summary>Performs a add operation on two <see cref="QuaternionF"/>.</summary>
		///<param name="a">The first <see cref="QuaternionF"/> to add.</param>
		///<param name="b">The second <see cref="QuaternionF"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref QuaternionF a, ref QuaternionF b, out QuaternionF result)
		{
			result.X = a.X + b.X;
			result.Y = a.Y + b.Y;
			result.Z = a.Z + b.Z;
			result.W = a.W + b.W;
		}

		///<summary>Performs a add operation on two <see cref="QuaternionF"/>.</summary>
		///<param name="a">The first <see cref="QuaternionF"/> to add.</param>
		///<param name="b">The second <see cref="QuaternionF"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator +(QuaternionF a, QuaternionF b)
		{
			Add(ref a, ref b, out QuaternionF result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="QuaternionF"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="QuaternionF"/> to add.</param>
		///<param name="b">The <see cref="float"/> to add.</param>
		///<param name="result">Output for the result of the operation.</param>
		public static void Add(ref QuaternionF a, float b, out QuaternionF result)
		{
			result.X = a.X + b;
			result.Y = a.Y + b;
			result.Z = a.Z + b;
			result.W = a.W + b;
		}

		///<summary>Performs a add operation on a <see cref="QuaternionF"/> and a <see cref="float"/>.</summary>
		///<param name="a">The <see cref="QuaternionF"/> to add.</param>
		///<param name="b">The <see cref="float"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator +(QuaternionF a, float b)
		{
			Add(ref a, b, out QuaternionF result);
			return result;
		}

		///<summary>Performs a add operation on a <see cref="float"/> and a <see cref="QuaternionF"/>.</summary>
		///<param name="a">The <see cref="float"/> to add.</param>
		///<param name="b">The <see cref="QuaternionF"/> to add.</param>
		///<returns>The result of the operation.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static QuaternionF operator +(float a, QuaternionF b)
		{
			Add(ref b, a, out QuaternionF result);
			return result;
		}

#endregion

#region Operators - Equality
/// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(QuaternionF left, QuaternionF right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(QuaternionF left, QuaternionF right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Indexers
		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="QuaternionF"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe float this[int index]
		{
			get
			{
				if(index < 0 || index > 3)
					throw new IndexOutOfRangeException("index for QuaternionF must be between 0 and 3, inclusive.");

				return Values[index];
			}
			set
			{
				if(index < 0 || index > 3)
					throw new IndexOutOfRangeException("index for QuaternionF must be between 0 and 3, inclusive.");

				Values[index] = value;
			}
		}

		/// <summary> Gets or sets the component at the specified index. </summary>
		/// <value>The value of the <see cref="QuaternionF"/> component, depending on the index.</value>
		/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
		/// <returns>The value of the component at the specified index value provided.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
		public unsafe float this[uint index]
		{
			get
			{
				if(index > 3)
					throw new IndexOutOfRangeException("index for QuaternionF must be between 0 and 3, inclusive.");

				return Values[index];
			}
			set
			{
				if(index > 3)
					throw new IndexOutOfRangeException("index for QuaternionF must be between 0 and 3, inclusive.");

				Values[index] = value;
			}
		}

#endregion
	}
}

