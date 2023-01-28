using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten
{
    ///<summary>Represents a four dimensional mathematical AngleF.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [Serializable]
	public partial struct AngleF : IFormattable, IEquatable<AngleF>
	{
		/// <summary>
        /// A value that specifies the size of a single degree.
        /// </summary>
        public const float Degree = 0.002777777777777778F;

        /// <summary>
        /// A value that specifies the size of a single minute.
        /// </summary>
        public const float Minute = 0.000046296296296296F;

        /// <summary>
        /// A value that specifies the size of a single second.
        /// </summary>
        public const float Second = 0.000000771604938272F;

        /// <summary>
        /// A value that specifies the size of a single radian.
        /// </summary>
        public const float Radian = 0.159154943091895336F;

        /// <summary>
        /// A value that specifies the size of a single milliradian.
        /// </summary>
        public const float Milliradian = 0.0001591549431F;

        /// <summary>
        /// A value that specifies the size of a single gradian.
        /// </summary>
        public const float Gradian = 0.0025F;

		/// <summary>The radians component.</summary>
		[DataMember]
		public float Radians;



         /// <summary>
        /// Initializes a new instance of the <see cref="AngleF"/> structure with the
        /// given unit dependant angle and unit type.
        /// </summary>
        /// <param name="angle">A unit dependant measure of the angle.</param>
        /// <param name="type">The type of unit the angle argument is.</param>
        public AngleF(float angle, AngleType type)
        {
            switch (type)
            {
                case AngleType.Revolution:
                    Radians = MathHelper.RevolutionsToRadians(angle);
                    break;

                case AngleType.Degree:
                    Radians = MathHelper.DegreesToRadians(angle);
                    break;

                case AngleType.Radian:
                    Radians = angle;
                    break;

                case AngleType.Gradian:
                    Radians = MathHelper.GradiansToRadians(angle);
                    break;

                default:
                    Radians = 0F;
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AngleF"/> structure using the
        /// arc length formula (θ = s/r).
        /// </summary>
        /// <param name="arcLength">The measure of the arc.</param>
        /// <param name="radius">The radius of the circle.</param>
        public AngleF(float arcLength, float radius)
        {
            Radians = arcLength / radius;
        }

        /// <summary>
        /// Wraps this <see cref="AngleF"/> to be in the range [π, -π].
        /// </summary>
        public void Wrap()
        {
            Radians = MathHelper.WrapAngle(Radians);
        }

        /// <summary>
        /// Wraps this <see cref="AngleF"/> to be in the range [0, 2π).
        /// </summary>
        public void WrapPositive()
        {
            float newangle = Radians % float.Tau;

            if (newangle < 0F)
                newangle += float.Tau;

            Radians = newangle;
        }

        /// <summary>
        /// Gets or sets the total number of revolutions this <see cref="AngleF"/> represents.
        /// </summary>
        public float Revolutions
        {
            get => MathHelper.RadiansToRevolutions(Radians);
            set => Radians = MathHelper.RevolutionsToRadians(value);
        }

        /// <summary>
        /// Gets or sets the total number of degrees this <see cref="AngleF"/> represents.
        /// </summary>
        public float Degrees
        {
            get => MathHelper.RadiansToDegrees(Radians);
            set => Radians = MathHelper.DegreesToRadians(value);
        }

        /// <summary>
        /// Gets or sets the minutes component of the degrees this <see cref="AngleF"/> represents.
        /// When setting the minutes, if the value is in the range (-60, 60) the whole degrees are
        /// not changed; otherwise, the whole degrees may be changed. Fractional values may set
        /// the seconds component.
        /// </summary>
        public float Minutes
        {
            get
            {
                float degrees = MathHelper.RadiansToDegrees(Radians);

                if (degrees < 0)
                {
                    float degreesfloor = MathF.Ceiling(degrees);
                    return (degrees - degreesfloor) * 60.0F;
                }
                else
                {
                    float degreesfloor = MathF.Floor(degrees);
                    return (degrees - degreesfloor) * 60.0F;
                }
            }
            set
            {
                float degrees = MathHelper.RadiansToDegrees(Radians);
                float degreesfloor = MathF.Floor(degrees);

                degreesfloor += value / 60.0F;
                Radians = MathHelper.DegreesToRadians(degreesfloor);
            }
        }

        /// <summary>
        /// Gets or sets the seconds of the degrees this <see cref="AngleF"/> represents.
        /// When setting the seconds, if the value is in the range (-60, 60) the whole minutes
        /// or whole degrees are not changed; otherwise, the whole minutes or whole degrees
        /// may be changed.
        /// </summary>
        public float Seconds
        {
            get
            {
                float degrees = MathHelper.RadiansToDegrees(Radians);

                if (degrees < 0)
                {
                    float degreesfloor = MathF.Ceiling(degrees);

                    float minutes = (degrees - degreesfloor) * 60.0F;
                    float minutesfloor = MathF.Ceiling(minutes);

                    return (minutes - minutesfloor) * 60.0F;
                }
                else
                {
                    float degreesfloor = MathF.Floor(degrees);

                    float minutes = (degrees - degreesfloor) * 60.0F;
                    float minutesfloor = MathF.Floor(minutes);

                    return (minutes - minutesfloor) * 60.0F;
                }
            }
            set
            {
                float degrees = MathHelper.RadiansToDegrees(Radians);
                float degreesfloor = MathF.Floor(degrees);

                float minutes = (degrees - degreesfloor) * 60.0F;
                float minutesfloor = MathF.Floor(minutes);

                minutesfloor += value / 60.0F;
                degreesfloor += minutesfloor / 60.0F;
                Radians = MathHelper.DegreesToRadians(degreesfloor);
            }
        }

        /// <summary>
        /// Gets or sets the total number of milliradians this <see cref="AngleF"/> represents.
        /// One milliradian is equal to 1/(2000π).
        /// </summary>
        public float Milliradians
        {
            get => Radians / (Milliradian * float.Tau);
            set => Radians = value * (Milliradian * float.Tau);
        }

        /// <summary>
        /// Gets or sets the total number of gradians this <see cref="AngleF"/> represents.
        /// </summary>
        public float Gradians
        {
            get => MathHelper.RadiansToGradians(Radians);
            set => Radians = MathHelper.RadiansToGradians(value);
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleF"/>
        /// is a right angle (i.e. 90° or π/2).
        /// </summary>
        public bool IsRight => Radians == MathHelper.Constants<float>.PiOverTwo; 

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleF"/>
        /// is a straight angle (i.e. 180° or π).
        /// </summary>
        public bool IsStraight => Radians == float.Pi;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleF"/>
        /// is a full rotation angle (i.e. 360° or 2π).
        /// </summary>
        public bool IsFullRotation => Radians == float.Tau;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleF"/>
        /// is an oblique angle (i.e. is not 90° or a multiple of 90°).
        /// </summary>
        public bool IsOblique => WrapPositive(this).Radians != MathHelper.Constants<float>.PiOverTwo; 

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleF"/>
        /// is an acute angle (i.e. less than 90° but greater than 0°).
        /// </summary>
        public bool IsAcute => Radians > 0.0 && Radians < MathHelper.Constants<float>.PiOverTwo;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleF"/>
        /// is an obtuse angle (i.e. greater than 90° but less than 180°).
        /// </summary>
        public bool IsObtuse => Radians > MathHelper.Constants<float>.PiOverTwo && Radians < float.Pi;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleF"/>
        /// is a reflex angle (i.e. greater than 180° but less than 360°).
        /// </summary>
        public bool IsReflex => Radians > float.Pi && Radians < float.Tau;

        /// <summary>
        /// Gets a <see cref="AngleF"/> instance that complements this angle (i.e. the two angles add to 90°).
        /// </summary>
        public AngleF Complement => new AngleF(MathHelper.Constants<float>.PiOverTwo - Radians, AngleType.Radian);

        /// <summary>
        /// Gets a <see cref="AngleF"/> instance that supplements this angle (i.e. the two angles add to 180°).
        /// </summary>
        public AngleF Supplement => new AngleF(float.Pi - Radians, AngleType.Radian);

        /// <summary>
        /// Wraps the <see cref="AngleF"/> given in the value argument to be in the range [π, -π].
        /// </summary>
        /// <param name="value">A <see cref="AngleF"/> to wrap.</param>
        /// <returns>The <see cref="AngleF"/> that is wrapped.</returns>
        public static AngleF Wrap(AngleF value)
        {
            value.Wrap();
            return value;
        }

                /// <summary>
        /// Wraps the <see cref="AngleF"/> given in the value argument to be in the range [π, -π].
        /// </summary>
        /// <param name="value">A <see cref="AngleF"/> to wrap.</param>
        /// <returns>The <see cref="AngleF"/> that is wrapped.</returns>
        public static AngleF Wrap(ref AngleF value)
        {
            value.Wrap();
            return value;
        }

        /// <summary>
        /// Wraps the <see cref="AngleF"/> given in the value argument to be in the range [0, 2π).
        /// </summary>
        /// <param name="value">A <see cref="AngleF"/> to wrap.</param>
        /// <returns>The <see cref="AngleF"/> that is wrapped.</returns>
        public static AngleF WrapPositive(AngleF value)
        {
            value.WrapPositive();
            return value;
        }

                /// <summary>
        /// Compares two <see cref="AngleF"/> instances and returns the smaller angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleF"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleF"/> instance to compare.</param>
        /// <returns>The smaller of the two given <see cref="AngleF"/> instances.</returns>
        public static AngleF Min(ref AngleF left, ref AngleF right)
        {
            if (left.Radians < right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Compares two <see cref="AngleF"/> instances and returns the smaller angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleF"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleF"/> instance to compare.</param>
        /// <returns>The smaller of the two given <see cref="AngleF"/> instances.</returns>
        public static AngleF Min(AngleF left, AngleF right)
        {
            if (left.Radians < right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Compares two <see cref="AngleF"/> instances and returns the greater angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleF"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleF"/> instance to compare.</param>
        /// <returns>The greater of the two given <see cref="AngleF"/> instances.</returns>
        public static AngleF Max(ref AngleF left, ref AngleF right)
        {
            if (left.Radians > right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Compares two <see cref="AngleF"/> instances and returns the greater angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleF"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleF"/> instance to compare.</param>
        /// <returns>The greater of the two given <see cref="AngleF"/> instances.</returns>
        public static AngleF Max(AngleF left, AngleF right)
        {
            if (left.Radians > right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Adds two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to add.</param>
        /// <param name="right">The second object to add.</param>
        /// <returns>The value of the two objects added together.</returns>
        public static AngleF Add(ref AngleF left, ref AngleF right)
        {
            return new AngleF(left.Radians + right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Subtracts two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to subtract.</param>
        /// <param name="right">The second object to subtract.</param>
        /// <returns>The value of the two objects subtracted.</returns>
        public static AngleF Subtract(ref AngleF left, ref AngleF right)
        {
            return new AngleF(left.Radians - right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Multiplies two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to multiply.</param>
        /// <param name="right">The second object to multiply.</param>
        /// <returns>The value of the two objects multiplied together.</returns>
        public static AngleF Multiply(ref AngleF left, ref AngleF right)
        {
            return new AngleF(left.Radians * right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Divides two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The numerator object.</param>
        /// <param name="right">The denominator object.</param>
        /// <returns>The value of the two objects divided.</returns>
        public static AngleF Divide(ref AngleF left, ref AngleF right)
        {
            return new AngleF(left.Radians / right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Gets a new <see cref="AngleF"/> instance that represents the zero angle (i.e. 0°).
        /// </summary>
        public static AngleF ZeroAngle => new AngleF(0.0F, AngleType.Radian); 

        /// <summary>
        /// Gets a new <see cref="AngleF"/> instance that represents the right angle (i.e. 90° or π/2).
        /// </summary>
        public static AngleF RightAngle => new AngleF(MathHelper.Constants<float>.PiOverTwo, AngleType.Radian);

        /// <summary>
        /// Gets a new <see cref="AngleF"/> instance that represents the straight angle (i.e. 180° or π).
        /// </summary>
        public static AngleF StraightAngle => new AngleF(float.Pi, AngleType.Radian);

        /// <summary>
        /// Gets a new <see cref="AngleF"/> instance that represents the full rotation angle (i.e. 360° or 2π).
        /// </summary>
        public static AngleF FullRotationAngle => new AngleF(float.Tau, AngleType.Radian);

        /// <summary>
        /// Returns a System.Boolean that indicates whether the values of two <see cref="AngleF"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the left and right parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(AngleF left, AngleF right)
        {
            return left.Radians == right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether the values of two <see cref="AngleF"/>
        /// objects are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the left and right parameters do not have the same value; otherwise, false.</returns>
        public static bool operator !=(AngleF left, AngleF right)
        {
            return left.Radians != right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleF"/>
        /// object is less than another <see cref="AngleF"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is less than right; otherwise, false.</returns>
        public static bool operator <(AngleF left, AngleF right)
        {
            return left.Radians < right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleF"/>
        /// object is greater than another <see cref="AngleF"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is greater than right; otherwise, false.</returns>
        public static bool operator >(AngleF left, AngleF right)
        {
            return left.Radians > right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleF"/>
        /// object is less than or equal to another <see cref="AngleF"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is less than or equal to right; otherwise, false.</returns>
        public static bool operator <=(AngleF left, AngleF right)
        {
            return left.Radians <= right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleF"/>
        /// object is greater than or equal to another <see cref="AngleF"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is greater than or equal to right; otherwise, false.</returns>
        public static bool operator >=(AngleF left, AngleF right)
        {
            return left.Radians >= right.Radians;
        }

        /// <summary>
        /// Returns the value of the <see cref="AngleF"/> operand. (The sign of
        /// the operand is unchanged.)
        /// </summary>
        /// <param name="value">A <see cref="AngleF"/> object.</param>
        /// <returns>The value of the value parameter.</returns>
        public static AngleF operator +(AngleF value)
        {
            return value;
        }

        /// <summary>
        /// Returns the the negated value of the <see cref="AngleF"/> operand.
        /// </summary>
        /// <param name="value">A <see cref="AngleF"/> object.</param>
        /// <returns>The negated value of the value parameter.</returns>
        public static AngleF operator -(AngleF value)
        {
            return new AngleF(-value.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Adds two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to add.</param>
        /// <param name="right">The second object to add.</param>
        /// <returns>The value of the two objects added together.</returns>
        public static AngleF operator +(AngleF left, AngleF right)
        {
            return new AngleF(left.Radians + right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Subtracts two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to subtract</param>
        /// <param name="right">The second object to subtract.</param>
        /// <returns>The value of the two objects subtracted.</returns>
        public static AngleF operator -(AngleF left, AngleF right)
        {
            return new AngleF(left.Radians - right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Multiplies two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to multiply.</param>
        /// <param name="right">The second object to multiply.</param>
        /// <returns>The value of the two objects multiplied together.</returns>
        public static AngleF operator *(AngleF left, AngleF right)
        {
            return new AngleF(left.Radians * right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Divides two <see cref="AngleF"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The numerator object.</param>
        /// <param name="right">The denominator object.</param>
        /// <returns>The value of the two objects divided.</returns>
        public static AngleF operator /(AngleF left, AngleF right)
        {
            return new AngleF(left.Radians / right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Compares this instance to a specified object and returns an integer that
        /// indicates whether the value of this instance is less than, equal to, or greater
        /// than the value of the specified object.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relationship of the current instance
        /// to the obj parameter. If the value is less than zero, the current instance
        /// is less than the other. If the value is zero, the current instance is equal
        /// to the other. If the value is greater than zero, the current instance is
        /// greater than the other.
        /// </returns>
        public int CompareTo(object other)
        {
            if (other == null)
                return 1;

            if (!(other is AngleF))
                throw new ArgumentException("Argument must be of type Angle.", "other");

            float rad = ((AngleF)other).Radians;

            if (this.Radians > rad)
                return 1;

            if (this.Radians < rad)
                return -1;

            return 0;
        }

        /// <summary>
        /// Compares this instance to a second <see cref="AngleF"/> and returns
        /// an integer that indicates whether the value of this instance is less than,
        /// equal to, or greater than the value of the specified object.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relationship of the current instance
        /// to the obj parameter. If the value is less than zero, the current instance
        /// is less than the other. If the value is zero, the current instance is equal
        /// to the other. If the value is greater than zero, the current instance is
        /// greater than the other.
        /// </returns>
        public int CompareTo(AngleF other)
        {
            if (this.Radians > other.Radians)
                return 1;

            if (this.Radians < other.Radians)
                return -1;

            return 0;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified
        /// <see cref="AngleF"/> object have the same value.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>
        /// Returns true if this <see cref="AngleF"/> object and another have the same value;
        /// otherwise, false.
        /// </returns>
        public bool Equals(AngleF other)
        {
            return this == other;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, MathHelper.RadiansToDegrees(Radians).ToString("0.##°"));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "{0}°", MathHelper.RadiansToDegrees(Radians).ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, MathHelper.RadiansToDegrees(Radians).ToString("0.##°"));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "{0}°", MathHelper.RadiansToDegrees(Radians).ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a hash code for this <see cref="AngleF"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Radians.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified
        /// object have the same value.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>
        /// Returns true if the obj parameter is a <see cref="AngleF"/> object or a type
        /// capable of implicit conversion to a <see cref="AngleF"/> value, and
        /// its value is equal to the value of the current <see cref="AngleF"/>
        /// object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is AngleF) && (this == (AngleF)obj);
        }
	}
}

