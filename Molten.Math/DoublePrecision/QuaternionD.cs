using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision;

///<summary>Represents a four dimensional mathematical QuaternionD.</summary>
[StructLayout(LayoutKind.Explicit)]
[Serializable]
public partial struct QuaternionD : IFormattable, IEquatable<QuaternionD>
{
	/// <summary>
    /// The size of the <see cref="QuaternionD"/> type, in bytes.
    /// </summary>
    public static readonly int SizeInBytes = Marshal.SizeOf(typeof(QuaternionD));

		/// <summary>
    /// A <see cref="QuaternionD"/> with all of its components set to zero.
    /// </summary>
    public static readonly QuaternionD Zero = new QuaternionD();

    /// <summary>
    /// A <see cref="QuaternionD"/> with all of its components set to one.
    /// </summary>
    public static readonly QuaternionD One = new QuaternionD(1D, 1D, 1D, 1D);

    /// <summary>
    /// The identity <see cref="QuaternionD"/> (0, 0, 0, 1).
    /// </summary>
    public static readonly QuaternionD Identity = new QuaternionD(0D, 0D, 0D, 0D);

	/// <summary>The X component.</summary>
	[DataMember]
	[FieldOffset(0)]
	public double X;

	/// <summary>The Y component.</summary>
	[DataMember]
	[FieldOffset(8)]
	public double Y;

	/// <summary>The Z component.</summary>
	[DataMember]
	[FieldOffset(16)]
	public double Z;

	/// <summary>The W component.</summary>
	[DataMember]
	[FieldOffset(24)]
	public double W;

	/// <summary>A fixed array mapped to the same memory space as the individual <see cref="QuaternionD"/> components.</summary>
	[IgnoreDataMember]
	[FieldOffset(0)]
	public unsafe fixed double Values[4];


    /// <summary>
    /// Gets a value indicting whether this  <see cref="QuaternionD"/> is normalized.
    /// </summary>
    public bool IsNormalized => MathHelper.IsOne((X * X) + (Y * Y) + (Z * Z) + (W * W));

    /// <summary>
    /// Gets the angle of the  <see cref="QuaternionD"/>.
    /// </summary>
    /// <value>The quaternion's angle.</value>
    public double Angle
    {
        get
        {
            double length = (X * X) + (Y * Y) + (Z * Z);
            if (MathHelper.IsZero(length))
                return 0.0D;

            return 2.0D * Math.Acos(double.Clamp(W, -1D, 1D));
        }
    }

    /// <summary>
    /// Gets the axis components of the QuaternionD.
    /// </summary>
    /// <value>The axis components of the QuaternionD.</value>
    public Vector3D Axis
    {
        get
        {
            double length = (X * X) + (Y * Y) + (Z * Z);
            if (MathHelper.IsZero(length))
                return Vector3D.UnitX;

            double inv = 1.0D  / Math.Sqrt(length);
            return new Vector3D(X * inv, Y * inv, Z * inv);
        }
    }

#region Constructors
	/// <summary>
	/// Initializes a new instance of <see cref="QuaternionD"/>.
	/// </summary>
	/// <param name="x">The X component.</param>
	/// <param name="y">The Y component.</param>
	/// <param name="z">The Z component.</param>
	/// <param name="w">The W component.</param>
	public QuaternionD(double x, double y, double z, double w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}
	/// <summary>Initializes a new instance of <see cref="QuaternionD"/>.</summary>
	/// <param name="value">The value that will be assigned to all components.</param>
	public QuaternionD(double value)
	{
		X = value;
		Y = value;
		Z = value;
		W = value;
	}
	/// <summary>Initializes a new instance of <see cref="QuaternionD"/> from an array.</summary>
	/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least 4 elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 4 elements.</exception>
	public unsafe QuaternionD(double[] values)
	{
		if (values == null)
			throw new ArgumentNullException("values");
		if (values.Length < 4)
			throw new ArgumentOutOfRangeException("values", "There must be at least 4 input values for QuaternionD.");

		fixed (double* src = values)
		{
			fixed (double* dst = Values)
				Unsafe.CopyBlock(src, dst, (sizeof(double) * 4));
		}
	}
	/// <summary>Initializes a new instance of <see cref="QuaternionD"/> from a span.</summary>
	/// <param name="values">The values to assign to the X, Y, Z, W components of the color. This must be an array with at least 4 elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than 4 elements.</exception>
	public QuaternionD(Span<double> values)
	{
		if (values == null)
			throw new ArgumentNullException("values");
		if (values.Length < 4)
			throw new ArgumentOutOfRangeException("values", "There must be at least 4 input values for QuaternionD.");

		X = values[0];
		Y = values[1];
		Z = values[2];
		W = values[3];
	}
	/// <summary>Initializes a new instance of <see cref="QuaternionD"/> from a an unsafe pointer.</summary>
	/// <param name="ptrValues">The values to assign to the X, Y, Z, W components of the color.
	/// <para>There must be at least 4 elements available or undefined behaviour will occur.</para></param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="ptrValues"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ptrValues"/> contains more or less than 4 elements.</exception>
	public unsafe QuaternionD(double* ptrValues)
	{
		if (ptrValues == null)
			throw new ArgumentNullException("ptrValues");

		fixed (double* dst = Values)
			Unsafe.CopyBlock(ptrValues, dst, (sizeof(double) * 4));
	}
    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
    /// </summary>
    /// <param name="value">A QuaternionD containing the values with which to initialize the components.</param>
    public QuaternionD(Vector4D value)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        W = value.W;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
    /// </summary>
    /// <param name="value">A vector containing the values with which to initialize the X, Y, and Z components.</param>
    /// <param name="w">Initial value for the W component of the <see cref="QuaternionD"/>.</param>
    public QuaternionD(Vector3D value, double w)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        W = w;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionD"/> struct.
    /// </summary>
    /// <param name="value">A vector containing the values with which to initialize the X and Y components.</param>
    /// <param name="z">Initial value for the Z component of the <see cref="QuaternionD"/>.</param>
    /// <param name="w">Initial value for the W component of the <see cref="QuaternionD"/>.</param>
    public QuaternionD(Vector2D value, double z, double w)
    {
        X = value.X;
        Y = value.Y;
        Z = z;
        W = w;
    }
#endregion

#region Instance Methods
/// <summary>
    /// Gets a value indicating whether this instance is equivalent to the identity QuaternionD.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is an identity QuaternionD; otherwise, <c>false</c>.
    /// </value>
    public bool IsIdentity
    {
        get { return this.Equals(Identity); }
    }

    /// <summary>
    /// Conjugates the <see cref="QuaternionD"/> X,Y and Z components. W component is not changed.
    /// </summary>
    public void Conjugate()
    {
        X = -X;
        Y = -Y;
        Z = -Z;
    }

    /// <summary>
    /// Conjugates the <see cref="QuaternionD"/>.
    /// </summary>
    public void Abs()
    {
        X = Math.Abs(X);
        Y = Math.Abs(Y);
        Z = Math.Abs(Z);
        W = Math.Abs(W);
    }

    /// <summary>
    /// Calculates the squared length of the <see cref="QuaternionD"/>.
    /// </summary>
    /// <returns>The squared length of the <see cref="QuaternionD"/>.</returns>
    /// <remarks>
    /// This method may be preferred to <see cref="QuaternionD.Length"/> when only a relative length is needed
    /// and speed is of the essence.
    /// </remarks>
    public double LengthSquared()
    {
        return (X * X) + (Y * Y) + (Z * Z) + (W * W);
    }

    /// <summary>
    /// Creates an array containing the elements of the <see cref="QuaternionD"/>.
    /// </summary>
    /// <returns>A four-element array containing the components of the <see cref="QuaternionD"/>.</returns>
    public double[] ToArray()
    {
        return new double[] { X, Y, Z, W };
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
    public static QuaternionD GetQuaternionBetweenNormalizedVectors(ref Vector3D v1, ref Vector3D v2)
    {
        QuaternionD q;
        double dot = Vector3D.Dot(ref v1, ref v2);
        //For non-normal vectors, the multiplying the axes length squared would be necessary:
        //float w = dot + MathF.Sqrt(v1.LengthSquared() * v2.LengthSquared());
        if (dot < -0.9999D) //parallel, opposing direction
        {
            //If this occurs, the rotation required is ~180 degrees.
            //The problem is that we could choose any perpendicular axis for the rotation. It's not uniquely defined.
            //The solution is to pick an arbitrary perpendicular axis.
            //Project onto the plane which has the lowest component magnitude.
            //On that 2d plane, perform a 90 degree rotation.
            double absX = Math.Abs(v1.X);
            double absY = Math.Abs(v1.Y);
            double absZ = Math.Abs(v1.Z);
            if (absX < absY && absX < absZ)
                q = new QuaternionD(0, -v1.Z, v1.Y, 0);
            else if (absY < absZ)
                q = new QuaternionD(-v1.Z, 0, v1.X, 0);
            else
                q = new QuaternionD(-v1.Y, v1.X, 0, 0);
        }
        else
        {
            Vector3D axis = Vector3D.Cross(ref v1, ref v2);
            q = new QuaternionD(axis.X, axis.Y, axis.Z, dot + 1);
        }
        q.Normalize();

        return q;
    }

    /// <summary>
    /// Converts the <see cref="QuaternionD"/> into a unit quaternion.
    /// </summary>
    public void Normalize()
    {
        double length = Length();
        if (!MathHelper.IsZero(length))
        {
            double inverse = 1.0D / length;
            X *= inverse;
            Y *= inverse;
            Z *= inverse;
            W *= inverse;
        }
    }

    /// <summary>
    /// Calculates the length of the QuaternionD.
    /// </summary>
    /// <returns>The length of the QuaternionD.</returns>
    /// <remarks>
    /// <see cref="QuaternionD.LengthSquared"/> may be preferred when only the relative length is needed and speed is of the essence.
    /// </remarks>
    public double Length()
    {
        return Math.Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
    }

    /// <summary>
    /// Conjugates and renormalizes the QuaternionD.
    /// </summary>
    public void Invert()
    {
        double lengthSq = LengthSquared();
        if (!MathHelper.IsZero(lengthSq))
        {
            lengthSq = 1.0D / lengthSq;

            X = -X * lengthSq;
            Y = -Y * lengthSq;
            Z = -Z * lengthSq;
            W = W * lengthSq;
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="QuaternionD"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="QuaternionD"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="QuaternionD"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(ref QuaternionD other)
    {
        return MathHelper.NearEqual(other.X, X) && MathHelper.NearEqual(other.Y, Y) && MathHelper.NearEqual(other.Z, Z) && MathHelper.NearEqual(other.W, W);
    }

    /// <summary>
    /// Determines whether the specified <see cref="QuaternionD"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="QuaternionD"/> to compare with this instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="QuaternionD"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(QuaternionD other)
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
        if (!(value is QuaternionD))
            return false;

        var strongValue = (QuaternionD)value;
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
    /// Multiplies two <see cref="QuaternionD"/> together in opposite order.
    /// </summary>
    /// <param name="a">First <see cref="QuaternionD"/> to multiply.</param>
    /// <param name="b">Second <see cref="QuaternionD"/> to multiply.</param>
    public static void Concatenate(ref QuaternionD a, ref QuaternionD b, out QuaternionD result)
    {
        double aX = a.X;
        double aY = a.Y;
        double aZ = a.Z;
        double aW = a.W;
        double bX = b.X;
        double bY = b.Y;
        double bZ = b.Z;
        double bW = b.W;

        result.X = aW * bX + aX * bW + aZ * bY - aY * bZ;
        result.Y = aW * bY + aY * bW + aX * bZ - aZ * bX;
        result.Z = aW * bZ + aZ * bW + aY * bX - aX * bY;
        result.W = aW * bW - aX * bX - aY * bY - aZ * bZ;
    }

    /// <summary>
    /// Multiplies two <see cref="QuaternionD"/> together in opposite order.
    /// </summary>
    /// <param name="a">First <see cref="QuaternionD"/> to multiply.</param>
    /// <param name="b">Second <see cref="QuaternionD"/> to multiply.</param>
    public static QuaternionD Concatenate(ref QuaternionD a, ref QuaternionD b)
    {
        Concatenate(ref a, ref b, out QuaternionD result);
        return result;
    }

    /// <summary>
    /// Computes the angle change represented by a normalized quaternion.
    /// </summary>
    /// <param name="q">Quaternion to be converted.</param>
    /// <returns>Angle around the axis represented by the quaternion.</returns>
    public static double GetAngleFromQuaternion(ref QuaternionD q)
    {
        double qw = Math.Abs(q.W);
        if (qw > 1)
            return 0;
        return 2 * (double)Math.Acos(qw);
    }

    /// <summary>
    /// Calculates the natural logarithm of the specified quaternion.
    /// </summary>
    /// <param name="value">The quaternion whose logarithm will be calculated.</param>
    /// <param name="result">When the method completes, contains the natural logarithm of the quaternion.</param>
    public static QuaternionD Logarithm(ref QuaternionD value)
    {
        QuaternionD result;

        if (Math.Abs(value.W) < 1.0)
        {
            double angle = Math.Acos(value.W);
            double sin = Math.Sin(angle);

            if (!MathHelper.IsZero(sin))
            {
                double coeff = angle / sin;
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

        result.W = 0.0D;
        return result;
    }

    /// <summary>
    /// Calculates the natural logarithm of the specified quaternion.
    /// </summary>
    /// <param name="value">The quaternion whose logarithm will be calculated.</param>
    /// <param name="result">When the method completes, contains the natural logarithm of the quaternion.</param>
    public static QuaternionD Logarithm(QuaternionD value)
    {
        return Logarithm(ref value);
    }

    /// <summary>
    /// Computes the axis angle representation of a normalized <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="q"><see cref="QuaternionD"/> to be converted.</param>
    /// <param name="axis">Axis represented by the <see cref="QuaternionD"/>.</param>
    /// <param name="angle">Angle around the axis represented by the <see cref="QuaternionD"/>.</param>
    public static void GetAxisAngle(ref QuaternionD q, out Vector3D axis, out double angle)
    {
        double qw = q.W;
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

        double lengthSquared = axis.LengthSquared();
        if (lengthSquared > 1e-14f)
        {
            axis = axis / Math.Sqrt(lengthSquared);
            angle = 2D * Math.Acos(double.Clamp(qw, -1, 1));
        }
        else
        {
            axis = Vector3D.Up;
            angle = 0;
        }
    }

    /// <summary>
    /// Sets up control points for spherical quadrangle interpolation.
    /// </summary>
    /// <param name="value1">First source <see cref="QuaternionD"/>.</param>
    /// <param name="value2">Second source <see cref="QuaternionD"/>.</param>
    /// <param name="value3">Third source <see cref="QuaternionD"/>.</param>
    /// <param name="value4">Fourth source <see cref="QuaternionD"/>.</param>
    /// <returns>An array of three <see cref="QuaternionD"/> that represent control points for spherical quadrangle interpolation.</returns>
    public static QuaternionD[] SquadSetup(ref QuaternionD value1, ref QuaternionD value2, ref QuaternionD value3, ref QuaternionD value4)
    {
        QuaternionD q0 = (value1 + value2).LengthSquared() < (value1 - value2).LengthSquared() ? -value1 : value1;
        QuaternionD q2 = (value2 + value3).LengthSquared() < (value2 - value3).LengthSquared() ? -value3 : value3;
        QuaternionD q3 = (value3 + value4).LengthSquared() < (value3 - value4).LengthSquared() ? -value4 : value4;
        QuaternionD q1 = value2;

        QuaternionD q1Exp = Exponential(ref q1);
        QuaternionD q2Exp = Exponential(ref q2);

        QuaternionD[] results = new QuaternionD[3];
        results[0] = q1 * Exponential(-0.25f * (Logarithm(q1Exp * q2) + Logarithm(q1Exp * q0)));
        results[1] = q2 * Exponential(-0.25f * (Logarithm(q2Exp * q3) + Logarithm(q2Exp * q1)));
        results[2] = q2;

        return results;
    }

    /// <summary>
    /// Interpolates between <see cref="QuaternionD"/>, using spherical quadrangle interpolation.
    /// </summary>
    /// <param name="value1">First source <see cref="QuaternionD"/>.</param>
    /// <param name="value2">Second source <see cref="QuaternionD"/>.</param>
    /// <param name="value3">Third source <see cref="QuaternionD"/>.</param>
    /// <param name="value4">Fourth source <see cref="QuaternionD"/>.</param>
    /// <param name="amount">Value between 0 and 1 indicating the weight of interpolation.</param>
    /// <param name="result">When the method completes, contains the spherical quadrangle interpolation of the <see cref="QuaternionD"/>.</param>
    public static QuaternionD Squad(ref QuaternionD value1, ref QuaternionD value2, ref QuaternionD value3, ref QuaternionD value4, float amount)
    {
        QuaternionD start = Slerp(ref value1, ref value4, amount);
        QuaternionD end = Slerp(ref value2, ref value3, amount);
        return Slerp(ref start, ref end, 2.0D * amount * (1.0D - amount));
    }

    /// <summary>
    /// Interpolates between two <see cref="QuaternionD"/>, using spherical linear interpolation.
    /// </summary>
    /// <param name="start">Start <see cref="QuaternionD"/>.</param>
    /// <param name="end">End <see cref="QuaternionD"/>.</param>
    /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
    /// <param name="result">When the method completes, contains the spherical linear interpolation of the two <see cref="QuaternionD"/>.</param>
    public static QuaternionD Slerp(ref QuaternionD start, ref QuaternionD end, double amount)
    {
        double opposite;
        double inverse;
        double dot = Dot(start, end);

        if (Math.Abs(dot) > 1.0D - MathHelper.Constants<double>.ZeroTolerance)
        {
            inverse = 1.0D - amount;
            opposite = amount * Math.Sign(dot);
        }
        else
        {
            double acos = Math.Acos(Math.Abs(dot));
            double invSin = 1.0D / Math.Sin(acos);

            inverse = Math.Sin((1.0D - amount) * acos) * invSin;
            opposite = Math.Sin(amount * acos) * invSin * Math.Sign(dot);
        }

        return new QuaternionD()
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
    public static void RotationYawPitchRoll(double yaw, double pitch, double roll, out QuaternionD result)
    {
        double halfRoll = roll * 0.5f;
        double halfPitch = pitch * 0.5f;
        double halfYaw = yaw * 0.5f;

        double sinRoll = Math.Sin(halfRoll);
        double cosRoll = Math.Cos(halfRoll);
        double sinPitch = Math.Sin(halfPitch);
        double cosPitch = Math.Cos(halfPitch);
        double sinYaw = Math.Sin(halfYaw);
        double cosYaw = Math.Cos(halfYaw);

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
    public static QuaternionD RotationYawPitchRoll(double yaw, double pitch, double roll)
    {
        double halfRoll = roll * 0.5f;
        double halfPitch = pitch * 0.5f;
        double halfYaw = yaw * 0.5f;

        double sinRoll = Math.Sin(halfRoll);
        double cosRoll = Math.Cos(halfRoll);
        double sinPitch = Math.Sin(halfPitch);
        double cosPitch = Math.Cos(halfPitch);
        double sinYaw = Math.Sin(halfYaw);
        double cosYaw = Math.Cos(halfYaw);

        return new QuaternionD()
        {
            X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll),
            Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll),
            Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll),
            W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll)
        };
    }

    /// <summary>
    /// Creates a <see cref="QuaternionD"/> given a rotation matrix.
    /// </summary>
    /// <param name="matrix">The rotation matrix.</param>
    public static QuaternionD FromRotationMatrix(ref Matrix4D matrix)
    {
        double sqrt;
        double half;
        double scale = matrix.M11 + matrix.M22 + matrix.M33;
        QuaternionD result;

        if (scale > 0.0D)
        {
            sqrt = Math.Sqrt(scale + 1.0D);
            result.W = sqrt * 0.5D;
            sqrt = 0.5D / sqrt;

            result.X = (matrix.M23 - matrix.M32) * sqrt;
            result.Y = (matrix.M31 - matrix.M13) * sqrt;
            result.Z = (matrix.M12 - matrix.M21) * sqrt;
        }
        else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
        {
            sqrt = Math.Sqrt(1.0D + matrix.M11 - matrix.M22 - matrix.M33);
            half = 0.5D / sqrt;

            result.X = 0.5D * sqrt;
            result.Y = (matrix.M12 + matrix.M21) * half;
            result.Z = (matrix.M13 + matrix.M31) * half;
            result.W = (matrix.M23 - matrix.M32) * half;
        }
        else if (matrix.M22 > matrix.M33)
        {
            sqrt = Math.Sqrt(1.0D + matrix.M22 - matrix.M11 - matrix.M33);
            half = 0.5f / sqrt;

            result.X = (matrix.M21 + matrix.M12) * half;
            result.Y = 0.5D * sqrt;
            result.Z = (matrix.M32 + matrix.M23) * half;
            result.W = (matrix.M31 - matrix.M13) * half;
        }
        else
        {
            sqrt = Math.Sqrt(1.0D  + matrix.M33 - matrix.M11 - matrix.M22);
            half = 0.5D  / sqrt;

            result.X = (matrix.M31 + matrix.M13) * half;
            result.Y = (matrix.M32 + matrix.M23) * half;
            result.Z = 0.5D * sqrt;
            result.W = (matrix.M12 - matrix.M21) * half;
        }

        return result;
    }

    /// <summary>
    /// Creates a quaternion given a rotation matrix.
    /// </summary>
    /// <param name="matrix">The rotation matrix.</param>
    public static QuaternionD FromRotationMatrix(ref Matrix3D matrix)
    {
        double sqrt;
        double half;
        double scale = matrix.M11 + matrix.M22 + matrix.M33;
        QuaternionD result;

        if (scale > 0.0f)
        {
            sqrt = Math.Sqrt(scale + 1.0f);
            result.W = sqrt * 0.5D;
            sqrt = 0.5D / sqrt;

            result.X = (matrix.M23 - matrix.M32) * sqrt;
            result.Y = (matrix.M31 - matrix.M13) * sqrt;
            result.Z = (matrix.M12 - matrix.M21) * sqrt;
        }
        else if ((matrix.M11 >= matrix.M22) && (matrix.M11 >= matrix.M33))
        {
            sqrt = Math.Sqrt(1.0f + matrix.M11 - matrix.M22 - matrix.M33);
            half = 0.5D / sqrt;

            result.X = 0.5D * sqrt;
            result.Y = (matrix.M12 + matrix.M21) * half;
            result.Z = (matrix.M13 + matrix.M31) * half;
            result.W = (matrix.M23 - matrix.M32) * half;
        }
        else if (matrix.M22 > matrix.M33)
        {
            sqrt = Math.Sqrt(1.0f + matrix.M22 - matrix.M11 - matrix.M33);
            half = 0.5D / sqrt;

            result.X = (matrix.M21 + matrix.M12) * half;
            result.Y = 0.5D * sqrt;
            result.Z = (matrix.M32 + matrix.M23) * half;
            result.W = (matrix.M31 - matrix.M13) * half;
        }
        else
        {
            sqrt = Math.Sqrt(1.0f + matrix.M33 - matrix.M11 - matrix.M22);
            half = 0.5D / sqrt;

            result.X = (matrix.M31 + matrix.M13) * half;
            result.Y = (matrix.M32 + matrix.M23) * half;
            result.Z = 0.5D * sqrt;
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
    /// Calculates the dot product of two <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="left">First source <see cref="QuaternionD"/>.</param>
    /// <param name="right">Second source <see cref="QuaternionD"/>.</param>
    /// <returns>The dot product of the two <see cref="QuaternionD"/>.</returns>
    public static double Dot(QuaternionD left, QuaternionD right)
    {
        return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
    }

    /// <summary>
    /// Exponentiates a <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="value">The <see cref="QuaternionD"/> to exponentiate.</param>
    public static QuaternionD Exponential(ref QuaternionD value)
    {
        double angle = double.Sqrt((value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z));
        double sin = double.Sin(angle);
        QuaternionD result;

        if (!MathHelper.IsZero(sin))
        {
            double coeff = sin / angle;
            result.X = coeff * value.X;
            result.Y = coeff * value.Y;
            result.Z = coeff * value.Z;
        }
        else
        {
            result = value;
        }

        result.W = double.Cos(angle);
        return result;
    }

    /// <summary>
    /// Exponentiates a <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="value">The <see cref="QuaternionD"/> to exponentiate.</param>
    public static QuaternionD Exponential(QuaternionD value)
    {
        return Exponential(ref value);
    }

    /// <summary>
    /// Returns a <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
    /// </summary>
    /// <param name="value1">A <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
    /// <param name="value2">A <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
    /// <param name="value3">A <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
    /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
    /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
    public static QuaternionD Barycentric(ref QuaternionD value1, ref QuaternionD value2, ref QuaternionD value3, double amount1, double amount2)
    {
        QuaternionD start = Slerp(ref value1, ref value2, amount1 + amount2);
        QuaternionD end = Slerp(ref value1, ref value3, amount1 + amount2);
        return Slerp(ref start, ref end, amount2 / (amount1 + amount2));
    }

    /// <summary>
    /// Returns a <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
    /// </summary>
    /// <param name="value1">A <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
    /// <param name="value2">A <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
    /// <param name="value3">A <see cref="QuaternionD"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
    /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
    /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
    public static QuaternionD Barycentric(QuaternionD value1, QuaternionD value2, QuaternionD value3, double amount1, double amount2)
    {
        return Barycentric(ref value1, ref value2, ref value3, amount1, amount2);
    }

    /// <summary>
    /// Computes the conjugate of the <see cref="QuaternionD"/> .
    /// </summary>
    /// <param name="value"><see cref="QuaternionD"/> to conjugate.</param>
    public static QuaternionD Conjugate(ref QuaternionD value)
    {
        return new QuaternionD()
        {
            X = -value.X,
            Y = -value.Y,
            Z = -value.Z,
            W = value.W
        };
    }

    /// <summary>
    /// Computes the conjugate of the <see cref="QuaternionD"/> .
    /// </summary>
    /// <param name="value"><see cref="QuaternionD"/> to conjugate.</param>
    public static void Conjugate(ref QuaternionD value, out QuaternionD result)
    {
        result.X = -value.X;
        result.Y = -value.Y;
        result.Z = -value.Z;
        result.W = value.W;
    }

    /// <summary>
    /// Computes the conjugate of the <see cref="QuaternionD"/> .
    /// </summary>
    /// <param name="value"><see cref="QuaternionD"/> to conjugate.</param>
    public static QuaternionD Conjugate(QuaternionD value)
    {
        return new QuaternionD()
        {
            X = -value.X,
            Y = -value.Y,
            Z = -value.Z,
            W = value.W
        };
    }

    /// <summary>
    /// Performs a linear interpolation between two <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="start">Start <see cref="QuaternionD"/>.</param>
    /// <param name="end">End <see cref="QuaternionD"/>.</param>
    /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
    /// <remarks>
    /// This method performs the linear interpolation based on the following formula.
    /// <code>start + (end - start) * amount</code>
    /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
    /// </remarks>
    public static QuaternionD Lerp(ref QuaternionD start, ref QuaternionD end, double amount)
    {
        double inverse = 1.0D - amount;
        QuaternionD result;

        if (Dot(start, end) >= 0.0D)
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
    /// Creates a <see cref="QuaternionD"/> given a rotation and an axis.
    /// </summary>
    /// <param name="axis">The axis of rotation.</param>
    /// <param name="angle">The angle of rotation.</param>
    public static QuaternionD FromAxisAngle(Vector3D axis, float angle)
    {
        return FromAxisAngle(ref axis, angle);
    }

    /// <summary>
    /// Creates a <see cref="QuaternionD"/> given a rotation and an axis.
    /// </summary>
    /// <param name="axis">The axis of rotation.</param>
    /// <param name="angle">The angle of rotation.</param>
    public static QuaternionD FromAxisAngle(ref Vector3D axis, float angle)
    {
        Vector3D normalized = axis.GetNormalized();

        double half = angle * 0.5D;
        double sin = Math.Sin(half);
        double cos = Math.Cos(half);

        return new QuaternionD()
        {
            X = normalized.X * sin,
            Y = normalized.Y * sin,
            Z = normalized.Z * sin,
            W = cos     
        };
    }

    /// <summary>
    /// Creates a left-handed, look-at <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="eye">The position of the viewer's eye.</param>
    /// <param name="target">The camera look-at target.</param>
    /// <param name="up">The camera's up vector.</param>
    public static QuaternionD LookAtLH(ref Vector3D eye, ref Vector3D target, ref Vector3D up)
    {
        Matrix3D matrix = Matrix3D.LookAtLH(ref eye, ref target, ref up);
        return FromRotationMatrix(ref matrix);
    }

    /// <summary>
    /// Creates a left-handed, look-at quaternion.
    /// </summary>
    /// <param name="forward">The camera's forward direction.</param>
    /// <param name="up">The camera's up vector.</param>
    /// <returns>The created look-at quaternion.</returns>
    public static QuaternionD RotationLookAtLH(Vector3D forward, Vector3D up)
    {
        return RotationLookAtLH(ref forward, ref up);
    }

    /// <summary>
    /// Creates a left-handed, look-at <see cref="QuaternionD"/>.
    /// </summary>
    /// <param name="forward">The camera's forward direction.</param>
    /// <param name="up">The camera's up vector.</param>
    public static QuaternionD  RotationLookAtLH(ref Vector3D forward, ref Vector3D up)
    {
        Vector3D eye = Vector3D.Zero;
        return LookAtLH(ref eye, ref forward, ref up);
    }

    /// <summary>
    /// Creates a right-handed, look-at quaternion.
    /// </summary>
    /// <param name="eye">The position of the viewer's eye.</param>
    /// <param name="target">The camera look-at target.</param>
    /// <param name="up">The camera's up vector.</param>
    public static QuaternionD LookAtRH(ref Vector3D eye, ref Vector3D target, ref Vector3D up)
    {
        Matrix3D matrix = Matrix3D.LookAtRH(ref eye, ref target, ref up);
        return FromRotationMatrix(ref matrix);
    }

    /// <summary>
    /// Creates a right-handed, look-at quaternion.
    /// </summary>
    /// <param name="forward">The camera's forward direction.</param>
    /// <param name="up">The camera's up vector.</param>
    public static QuaternionD RotationLookAtRH(ref Vector3D forward, ref Vector3D up)
    {
        Vector3D eye = Vector3D.Zero;
        return LookAtRH(ref eye, ref forward, ref up);
    }

    /// <summary>
    /// Creates a left-handed spherical billboard that rotates around a specified object position.
    /// </summary>
    /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraUpVector">The up vector of the camera.</param>
    /// <param name="cameraForwardVector">The forward vector of the camera.</param>
    public static QuaternionD BillboardLH(ref Vector3D objectPosition, ref Vector3D cameraPosition, ref Vector3D cameraUpVector, ref Vector3D cameraForwardVector)
    {
        Matrix3D matrix = Matrix3D.BillboardLH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector);
        return FromRotationMatrix(ref matrix);
    }

    /// <summary>
    /// Creates a right-handed spherical billboard that rotates around a specified object position.
    /// </summary>
    /// <param name="objectPosition">The position of the object around which the billboard will rotate.</param>
    /// <param name="cameraPosition">The position of the camera.</param>
    /// <param name="cameraUpVector">The up vector of the camera.</param>
    /// <param name="cameraForwardVector">The forward vector of the camera.</param>
    public static QuaternionD BillboardRH(ref Vector3D objectPosition, ref Vector3D cameraPosition, ref Vector3D cameraUpVector, ref Vector3D cameraForwardVector)
    {
        Matrix3D matrix = Matrix3D.BillboardRH(ref objectPosition, ref cameraPosition, ref cameraUpVector, ref cameraForwardVector);
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
    public static QuaternionD operator *(double scale, QuaternionD value)
    {
        return new QuaternionD()
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
    public static QuaternionD operator *(QuaternionD value, double scale)
    {
        return new QuaternionD()
        {
            X = value.X * scale,
            Y = value.Y * scale,
            Z = value.Z * scale,
            W = value.W * scale,
        };
    }

    /// <summary>
    /// Multiplies a QuaternionD by another QuaternionD.
    /// </summary>
    /// <param name="left">A reference to the first QuaternionD to multiply.</param>
    /// <param name="right">A reference to the second QuaternionD to multiply.</param>
    /// <param name="result">An output to store the result.</param>
    /// <returns>The multiplied QuaternionD.</returns>
    public static void Multiply(ref QuaternionD left, ref QuaternionD right, out QuaternionD result)
    {
        double lx = left.X;
        double ly = left.Y;
        double lz = left.Z;
        double lw = left.W;
        double rx = right.X;
        double ry = right.Y;
        double rz = right.Z;
        double rw = right.W;
        double a = (ly * rz - lz * ry);
        double b = (lz * rx - lx * rz);
        double c = (lx * ry - ly * rx);
        double d = (lx * rx + ly * ry + lz * rz);

        result.X = (lx * rw + rx * lw) + a;
        result.Y = (ly * rw + ry * lw) + b;
        result.Z = (lz * rw + rz * lw) + c;
        result.W = lw * rw - d;
    }

    /// <summary>
    /// Multiplies a QuaternionD by another QuaternionD.
    /// </summary>
    /// <param name="left">The first QuaternionD to multiply.</param>
    /// <param name="right">The second QuaternionD to multiply.</param>
    /// <returns>The multiplied QuaternionD.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static QuaternionD operator *(QuaternionD left, QuaternionD right)
    {
        Multiply(ref left, ref right, out QuaternionD result);
        return result;
    }
#endregion

#region Operators - Subtract
	///<summary>Performs a subtract operation on two <see cref="QuaternionD"/>.</summary>
	///<param name="a">The first <see cref="QuaternionD"/> to subtract.</param>
	///<param name="b">The second <see cref="QuaternionD"/> to subtract.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Subtract(ref QuaternionD a, ref QuaternionD b, out QuaternionD result)
	{
		result.X = a.X - b.X;
		result.Y = a.Y - b.Y;
		result.Z = a.Z - b.Z;
		result.W = a.W - b.W;
	}

	///<summary>Performs a subtract operation on two <see cref="QuaternionD"/>.</summary>
	///<param name="a">The first <see cref="QuaternionD"/> to subtract.</param>
	///<param name="b">The second <see cref="QuaternionD"/> to subtract.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator -(QuaternionD a, QuaternionD b)
	{
		Subtract(ref a, ref b, out QuaternionD result);
		return result;
	}

	///<summary>Performs a subtract operation on a <see cref="QuaternionD"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="QuaternionD"/> to subtract.</param>
	///<param name="b">The <see cref="double"/> to subtract.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Subtract(ref QuaternionD a, double b, out QuaternionD result)
	{
		result.X = a.X - b;
		result.Y = a.Y - b;
		result.Z = a.Z - b;
		result.W = a.W - b;
	}

	///<summary>Performs a subtract operation on a <see cref="QuaternionD"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="QuaternionD"/> to subtract.</param>
	///<param name="b">The <see cref="double"/> to subtract.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator -(QuaternionD a, double b)
	{
		Subtract(ref a, b, out QuaternionD result);
		return result;
	}

	///<summary>Performs a subtract operation on a <see cref="double"/> and a <see cref="QuaternionD"/>.</summary>
	///<param name="a">The <see cref="double"/> to subtract.</param>
	///<param name="b">The <see cref="QuaternionD"/> to subtract.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator -(double a, QuaternionD b)
	{
		Subtract(ref b, a, out QuaternionD result);
		return result;
	}


    /// <summary>
    /// Reverses the direction of a given quaternion.
    /// </summary>
    /// <param name="value">The quaternion to negate.</param>
    /// <returns>A quaternion facing in the opposite direction.</returns>
    public static QuaternionD operator -(QuaternionD value)
    {
        return new QuaternionD()
        {
            X = -value.X,
            Y = -value.Y,
            Z = -value.Z,
            W = -value.W
        };
    }
#endregion

#region Operators - Division
	///<summary>Performs a divide operation on two <see cref="QuaternionD"/>.</summary>
	///<param name="a">The first <see cref="QuaternionD"/> to divide.</param>
	///<param name="b">The second <see cref="QuaternionD"/> to divide.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Divide(ref QuaternionD a, ref QuaternionD b, out QuaternionD result)
	{
		result.X = a.X / b.X;
		result.Y = a.Y / b.Y;
		result.Z = a.Z / b.Z;
		result.W = a.W / b.W;
	}

	///<summary>Performs a divide operation on two <see cref="QuaternionD"/>.</summary>
	///<param name="a">The first <see cref="QuaternionD"/> to divide.</param>
	///<param name="b">The second <see cref="QuaternionD"/> to divide.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator /(QuaternionD a, QuaternionD b)
	{
		Divide(ref a, ref b, out QuaternionD result);
		return result;
	}

	///<summary>Performs a divide operation on a <see cref="QuaternionD"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="QuaternionD"/> to divide.</param>
	///<param name="b">The <see cref="double"/> to divide.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Divide(ref QuaternionD a, double b, out QuaternionD result)
	{
		result.X = a.X / b;
		result.Y = a.Y / b;
		result.Z = a.Z / b;
		result.W = a.W / b;
	}

	///<summary>Performs a divide operation on a <see cref="QuaternionD"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="QuaternionD"/> to divide.</param>
	///<param name="b">The <see cref="double"/> to divide.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator /(QuaternionD a, double b)
	{
		Divide(ref a, b, out QuaternionD result);
		return result;
	}

	///<summary>Performs a divide operation on a <see cref="double"/> and a <see cref="QuaternionD"/>.</summary>
	///<param name="a">The <see cref="double"/> to divide.</param>
	///<param name="b">The <see cref="QuaternionD"/> to divide.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator /(double a, QuaternionD b)
	{
		Divide(ref b, a, out QuaternionD result);
		return result;
	}

#endregion

#region Operators - Add
	///<summary>Performs a add operation on two <see cref="QuaternionD"/>.</summary>
	///<param name="a">The first <see cref="QuaternionD"/> to add.</param>
	///<param name="b">The second <see cref="QuaternionD"/> to add.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Add(ref QuaternionD a, ref QuaternionD b, out QuaternionD result)
	{
		result.X = a.X + b.X;
		result.Y = a.Y + b.Y;
		result.Z = a.Z + b.Z;
		result.W = a.W + b.W;
	}

	///<summary>Performs a add operation on two <see cref="QuaternionD"/>.</summary>
	///<param name="a">The first <see cref="QuaternionD"/> to add.</param>
	///<param name="b">The second <see cref="QuaternionD"/> to add.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator +(QuaternionD a, QuaternionD b)
	{
		Add(ref a, ref b, out QuaternionD result);
		return result;
	}

	///<summary>Performs a add operation on a <see cref="QuaternionD"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="QuaternionD"/> to add.</param>
	///<param name="b">The <see cref="double"/> to add.</param>
	///<param name="result">Output for the result of the operation.</param>
	public static void Add(ref QuaternionD a, double b, out QuaternionD result)
	{
		result.X = a.X + b;
		result.Y = a.Y + b;
		result.Z = a.Z + b;
		result.W = a.W + b;
	}

	///<summary>Performs a add operation on a <see cref="QuaternionD"/> and a <see cref="double"/>.</summary>
	///<param name="a">The <see cref="QuaternionD"/> to add.</param>
	///<param name="b">The <see cref="double"/> to add.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator +(QuaternionD a, double b)
	{
		Add(ref a, b, out QuaternionD result);
		return result;
	}

	///<summary>Performs a add operation on a <see cref="double"/> and a <see cref="QuaternionD"/>.</summary>
	///<param name="a">The <see cref="double"/> to add.</param>
	///<param name="b">The <see cref="QuaternionD"/> to add.</param>
	///<returns>The result of the operation.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static QuaternionD operator +(double a, QuaternionD b)
	{
		Add(ref b, a, out QuaternionD result);
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
    public static bool operator ==(QuaternionD left, QuaternionD right)
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
    public static bool operator !=(QuaternionD left, QuaternionD right)
    {
        return !left.Equals(ref right);
    }
#endregion

#region Indexers
	/// <summary> Gets or sets the component at the specified index. </summary>
	/// <value>The value of the <see cref="QuaternionD"/> component, depending on the index.</value>
	/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
	/// <returns>The value of the component at the specified index value provided.</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
	public unsafe double this[int index]
	{
		get
		{
			if(index < 0 || index > 3)
				throw new IndexOutOfRangeException("index for QuaternionD must be between 0 and 3, inclusive.");

			return Values[index];
		}
		set
		{
			if(index < 0 || index > 3)
				throw new IndexOutOfRangeException("index for QuaternionD must be between 0 and 3, inclusive.");

			Values[index] = value;
		}
	}

	/// <summary> Gets or sets the component at the specified index. </summary>
	/// <value>The value of the <see cref="QuaternionD"/> component, depending on the index.</value>
	/// <param name="index">The index of the index component to access, ranging from 0 to 3, inclusive.</param>
	/// <returns>The value of the component at the specified index value provided.</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
	public unsafe double this[uint index]
	{
		get
		{
			if(index > 3)
				throw new IndexOutOfRangeException("index for QuaternionD must be between 0 and 3, inclusive.");

			return Values[index];
		}
		set
		{
			if(index > 3)
				throw new IndexOutOfRangeException("index for QuaternionD must be between 0 and 3, inclusive.");

			Values[index] = value;
		}
	}

#endregion
}

