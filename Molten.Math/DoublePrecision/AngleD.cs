using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision
{
	///<summary>Represents a four dimensional mathematical AngleD.</summary>
	[StructLayout(LayoutKind.Explicit)]
    [Serializable]
	public partial struct AngleD : IFormattable, IEquatable<AngleD>
	{
		/// <summary>
        /// A value that specifies the size of a single degree.
        /// </summary>
        public const double Degree = 0.002777777777777778D;

        /// <summary>
        /// A value that specifies the size of a single minute.
        /// </summary>
        public const double Minute = 0.000046296296296296D;

        /// <summary>
        /// A value that specifies the size of a single second.
        /// </summary>
        public const double Second = 0.000000771604938272D;

        /// <summary>
        /// A value that specifies the size of a single radian.
        /// </summary>
        public const double Radian = 0.159154943091895336D;

        /// <summary>
        /// A value that specifies the size of a single milliradian.
        /// </summary>
        public const double Milliradian = 0.0001591549431D;

        /// <summary>
        /// A value that specifies the size of a single gradian.
        /// </summary>
        public const double Gradian = 0.0025D;

		/// <summary>The radians component.</summary>
		[DataMember]
		[FieldOffset(0)]
		public double Radians;



         /// <summary>
        /// Initializes a new instance of the <see cref="AngleD"/> structure with the
        /// given unit dependant angle and unit type.
        /// </summary>
        /// <param name="angle">A unit dependant measure of the angle.</param>
        /// <param name="type">The type of unit the angle argument is.</param>
        public AngleD(double angle, AngleType type)
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
                    Radians = 0D;
                    break;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AngleD"/> structure using the
        /// arc length formula (θ = s/r).
        /// </summary>
        /// <param name="arcLength">The measure of the arc.</param>
        /// <param name="radius">The radius of the circle.</param>
        public AngleD(double arcLength, double radius)
        {
            Radians = arcLength / radius;
        }

        /// <summary>
        /// Wraps this <see cref="AngleD"/> to be in the range [π, -π].
        /// </summary>
        public void Wrap()
        {
            Radians = MathHelper.WrapAngle(Radians);
        }

        /// <summary>
        /// Wraps this <see cref="AngleD"/> to be in the range [0, 2π).
        /// </summary>
        public void WrapPositive()
        {
            double newangle = Radians % double.Tau;

            if (newangle < 0D)
                newangle += double.Tau;

            Radians = newangle;
        }

        /// <summary>
        /// Gets or sets the total number of revolutions this <see cref="AngleD"/> represents.
        /// </summary>
        public double Revolutions
        {
            get => MathHelper.RadiansToRevolutions(Radians);
            set => Radians = MathHelper.RevolutionsToRadians(value);
        }

        /// <summary>
        /// Gets or sets the total number of degrees this <see cref="AngleD"/> represents.
        /// </summary>
        public double Degrees
        {
            get => MathHelper.RadiansToDegrees(Radians);
            set => Radians = MathHelper.DegreesToRadians(value);
        }

        /// <summary>
        /// Gets or sets the minutes component of the degrees this <see cref="AngleD"/> represents.
        /// When setting the minutes, if the value is in the range (-60, 60) the whole degrees are
        /// not changed; otherwise, the whole degrees may be changed. Fractional values may set
        /// the seconds component.
        /// </summary>
        public double Minutes
        {
            get
            {
                double degrees = MathHelper.RadiansToDegrees(Radians);

                if (degrees < 0)
                {
                    double degreesfloor = Math.Ceiling(degrees);
                    return (degrees - degreesfloor) * 60.0D;
                }
                else
                {
                    double degreesfloor = Math.Floor(degrees);
                    return (degrees - degreesfloor) * 60.0D;
                }
            }
            set
            {
                double degrees = MathHelper.RadiansToDegrees(Radians);
                double degreesfloor = Math.Floor(degrees);

                degreesfloor += value / 60.0D;
                Radians = MathHelper.DegreesToRadians(degreesfloor);
            }
        }

        /// <summary>
        /// Gets or sets the seconds of the degrees this <see cref="AngleD"/> represents.
        /// When setting the seconds, if the value is in the range (-60, 60) the whole minutes
        /// or whole degrees are not changed; otherwise, the whole minutes or whole degrees
        /// may be changed.
        /// </summary>
        public double Seconds
        {
            get
            {
                double degrees = MathHelper.RadiansToDegrees(Radians);

                if (degrees < 0)
                {
                    double degreesfloor = Math.Ceiling(degrees);

                    double minutes = (degrees - degreesfloor) * 60.0D;
                    double minutesfloor = Math.Ceiling(minutes);

                    return (minutes - minutesfloor) * 60.0D;
                }
                else
                {
                    double degreesfloor = Math.Floor(degrees);

                    double minutes = (degrees - degreesfloor) * 60.0D;
                    double minutesfloor = Math.Floor(minutes);

                    return (minutes - minutesfloor) * 60.0D;
                }
            }
            set
            {
                double degrees = MathHelper.RadiansToDegrees(Radians);
                double degreesfloor = Math.Floor(degrees);

                double minutes = (degrees - degreesfloor) * 60.0D;
                double minutesfloor = Math.Floor(minutes);

                minutesfloor += value / 60.0D;
                degreesfloor += minutesfloor / 60.0D;
                Radians = MathHelper.DegreesToRadians(degreesfloor);
            }
        }

        /// <summary>
        /// Gets or sets the total number of milliradians this <see cref="AngleD"/> represents.
        /// One milliradian is equal to 1/(2000π).
        /// </summary>
        public double Milliradians
        {
            get => Radians / (Milliradian * double.Tau);
            set => Radians = value * (Milliradian * double.Tau);
        }

        /// <summary>
        /// Gets or sets the total number of gradians this <see cref="AngleD"/> represents.
        /// </summary>
        public double Gradians
        {
            get => MathHelper.RadiansToGradians(Radians);
            set => Radians = MathHelper.RadiansToGradians(value);
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a right angle (i.e. 90° or π/2).
        /// </summary>
        public bool IsRight => Radians == MathHelper.Constants<double>.PiOverTwo; 

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a straight angle (i.e. 180° or π).
        /// </summary>
        public bool IsStraight => Radians == double.Pi;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a full rotation angle (i.e. 360° or 2π).
        /// </summary>
        public bool IsFullRotation => Radians == double.Tau;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is an oblique angle (i.e. is not 90° or a multiple of 90°).
        /// </summary>
        public bool IsOblique => WrapPositive(this).Radians != MathHelper.Constants<double>.PiOverTwo; 

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is an acute angle (i.e. less than 90° but greater than 0°).
        /// </summary>
        public bool IsAcute => Radians > 0.0 && Radians < MathHelper.Constants<double>.PiOverTwo;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is an obtuse angle (i.e. greater than 90° but less than 180°).
        /// </summary>
        public bool IsObtuse => Radians > MathHelper.Constants<double>.PiOverTwo && Radians < double.Pi;

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a reflex angle (i.e. greater than 180° but less than 360°).
        /// </summary>
        public bool IsReflex => Radians > double.Pi && Radians < double.Tau;

        /// <summary>
        /// Gets a <see cref="AngleD"/> instance that complements this angle (i.e. the two angles add to 90°).
        /// </summary>
        public AngleD Complement => new AngleD(MathHelper.Constants<double>.PiOverTwo - Radians, AngleType.Radian);

        /// <summary>
        /// Gets a <see cref="AngleD"/> instance that supplements this angle (i.e. the two angles add to 180°).
        /// </summary>
        public AngleD Supplement => new AngleD(double.Pi - Radians, AngleType.Radian);

        /// <summary>
        /// Wraps the <see cref="AngleD"/> given in the value argument to be in the range [π, -π].
        /// </summary>
        /// <param name="value">A <see cref="AngleD"/> to wrap.</param>
        /// <returns>The <see cref="AngleD"/> that is wrapped.</returns>
        public static AngleD Wrap(AngleD value)
        {
            value.Wrap();
            return value;
        }

                /// <summary>
        /// Wraps the <see cref="AngleD"/> given in the value argument to be in the range [π, -π].
        /// </summary>
        /// <param name="value">A <see cref="AngleD"/> to wrap.</param>
        /// <returns>The <see cref="AngleD"/> that is wrapped.</returns>
        public static AngleD Wrap(ref AngleD value)
        {
            value.Wrap();
            return value;
        }

        /// <summary>
        /// Wraps the <see cref="AngleD"/> given in the value argument to be in the range [0, 2π).
        /// </summary>
        /// <param name="value">A <see cref="AngleD"/> to wrap.</param>
        /// <returns>The <see cref="AngleD"/> that is wrapped.</returns>
        public static AngleD WrapPositive(AngleD value)
        {
            value.WrapPositive();
            return value;
        }

                /// <summary>
        /// Compares two <see cref="AngleD"/> instances and returns the smaller angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleD"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleD"/> instance to compare.</param>
        /// <returns>The smaller of the two given <see cref="AngleD"/> instances.</returns>
        public static AngleD Min(ref AngleD left, ref AngleD right)
        {
            if (left.Radians < right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Compares two <see cref="AngleD"/> instances and returns the smaller angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleD"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleD"/> instance to compare.</param>
        /// <returns>The smaller of the two given <see cref="AngleD"/> instances.</returns>
        public static AngleD Min(AngleD left, AngleD right)
        {
            if (left.Radians < right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Compares two <see cref="AngleD"/> instances and returns the greater angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleD"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleD"/> instance to compare.</param>
        /// <returns>The greater of the two given <see cref="AngleD"/> instances.</returns>
        public static AngleD Max(ref AngleD left, ref AngleD right)
        {
            if (left.Radians > right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Compares two <see cref="AngleD"/> instances and returns the greater angle.
        /// </summary>
        /// <param name="left">The first <see cref="AngleD"/> instance to compare.</param>
        /// <param name="right">The second <see cref="AngleD"/> instance to compare.</param>
        /// <returns>The greater of the two given <see cref="AngleD"/> instances.</returns>
        public static AngleD Max(AngleD left, AngleD right)
        {
            if (left.Radians > right.Radians)
                return left;

            return right;
        }

        /// <summary>
        /// Adds two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to add.</param>
        /// <param name="right">The second object to add.</param>
        /// <returns>The value of the two objects added together.</returns>
        public static AngleD Add(ref AngleD left, ref AngleD right)
        {
            return new AngleD(left.Radians + right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Subtracts two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to subtract.</param>
        /// <param name="right">The second object to subtract.</param>
        /// <returns>The value of the two objects subtracted.</returns>
        public static AngleD Subtract(ref AngleD left, ref AngleD right)
        {
            return new AngleD(left.Radians - right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Multiplies two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to multiply.</param>
        /// <param name="right">The second object to multiply.</param>
        /// <returns>The value of the two objects multiplied together.</returns>
        public static AngleD Multiply(ref AngleD left, ref AngleD right)
        {
            return new AngleD(left.Radians * right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Divides two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The numerator object.</param>
        /// <param name="right">The denominator object.</param>
        /// <returns>The value of the two objects divided.</returns>
        public static AngleD Divide(ref AngleD left, ref AngleD right)
        {
            return new AngleD(left.Radians / right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the zero angle (i.e. 0°).
        /// </summary>
        public static AngleD ZeroAngle => new AngleD(0.0D, AngleType.Radian); 

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the right angle (i.e. 90° or π/2).
        /// </summary>
        public static AngleD RightAngle => new AngleD(MathHelper.Constants<double>.PiOverTwo, AngleType.Radian);

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the straight angle (i.e. 180° or π).
        /// </summary>
        public static AngleD StraightAngle => new AngleD(double.Pi, AngleType.Radian);

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the full rotation angle (i.e. 360° or 2π).
        /// </summary>
        public static AngleD FullRotationAngle => new AngleD(double.Tau, AngleType.Radian);

        /// <summary>
        /// Returns a System.Boolean that indicates whether the values of two <see cref="AngleD"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the left and right parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(AngleD left, AngleD right)
        {
            return left.Radians == right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether the values of two <see cref="AngleD"/>
        /// objects are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the left and right parameters do not have the same value; otherwise, false.</returns>
        public static bool operator !=(AngleD left, AngleD right)
        {
            return left.Radians != right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleD"/>
        /// object is less than another <see cref="AngleD"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is less than right; otherwise, false.</returns>
        public static bool operator <(AngleD left, AngleD right)
        {
            return left.Radians < right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleD"/>
        /// object is greater than another <see cref="AngleD"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is greater than right; otherwise, false.</returns>
        public static bool operator >(AngleD left, AngleD right)
        {
            return left.Radians > right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleD"/>
        /// object is less than or equal to another <see cref="AngleD"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is less than or equal to right; otherwise, false.</returns>
        public static bool operator <=(AngleD left, AngleD right)
        {
            return left.Radians <= right.Radians;
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether a <see cref="AngleD"/>
        /// object is greater than or equal to another <see cref="AngleD"/> object.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if left is greater than or equal to right; otherwise, false.</returns>
        public static bool operator >=(AngleD left, AngleD right)
        {
            return left.Radians >= right.Radians;
        }

        /// <summary>
        /// Returns the value of the <see cref="AngleD"/> operand. (The sign of
        /// the operand is unchanged.)
        /// </summary>
        /// <param name="value">A <see cref="AngleD"/> object.</param>
        /// <returns>The value of the value parameter.</returns>
        public static AngleD operator +(AngleD value)
        {
            return value;
        }

        /// <summary>
        /// Returns the the negated value of the <see cref="AngleD"/> operand.
        /// </summary>
        /// <param name="value">A <see cref="AngleD"/> object.</param>
        /// <returns>The negated value of the value parameter.</returns>
        public static AngleD operator -(AngleD value)
        {
            return new AngleD(-value.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Adds two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to add.</param>
        /// <param name="right">The second object to add.</param>
        /// <returns>The value of the two objects added together.</returns>
        public static AngleD operator +(AngleD left, AngleD right)
        {
            return new AngleD(left.Radians + right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Subtracts two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to subtract</param>
        /// <param name="right">The second object to subtract.</param>
        /// <returns>The value of the two objects subtracted.</returns>
        public static AngleD operator -(AngleD left, AngleD right)
        {
            return new AngleD(left.Radians - right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Multiplies two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to multiply.</param>
        /// <param name="right">The second object to multiply.</param>
        /// <returns>The value of the two objects multiplied together.</returns>
        public static AngleD operator *(AngleD left, AngleD right)
        {
            return new AngleD(left.Radians * right.Radians, AngleType.Radian);
        }

        /// <summary>
        /// Divides two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The numerator object.</param>
        /// <param name="right">The denominator object.</param>
        /// <returns>The value of the two objects divided.</returns>
        public static AngleD operator /(AngleD left, AngleD right)
        {
            return new AngleD(left.Radians / right.Radians, AngleType.Radian);
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

            if (!(other is AngleD))
                throw new ArgumentException("Argument must be of type Angle.", "other");

            double rad = ((AngleD)other).Radians;

            if (this.Radians > rad)
                return 1;

            if (this.Radians < rad)
                return -1;

            return 0;
        }

        /// <summary>
        /// Compares this instance to a second <see cref="AngleD"/> and returns
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
        public int CompareTo(AngleD other)
        {
            if (this.Radians > other.Radians)
                return 1;

            if (this.Radians < other.Radians)
                return -1;

            return 0;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified
        /// <see cref="AngleD"/> object have the same value.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>
        /// Returns true if this <see cref="AngleD"/> object and another have the same value;
        /// otherwise, false.
        /// </returns>
        public bool Equals(AngleD other)
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
        /// Returns a hash code for this <see cref="AngleD"/> instance.
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
        /// Returns true if the obj parameter is a <see cref="AngleD"/> object or a type
        /// capable of implicit conversion to a <see cref="AngleD"/> value, and
        /// its value is equal to the value of the current <see cref="AngleD"/>
        /// object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return (obj is AngleD) && (this == (AngleD)obj);
        }
	}
}

