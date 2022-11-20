// James Yarwod - double-precision version of  SharpDX's AngleSingle

// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Molten.DoublePrecision
{
    /// <summary>
    /// Represents a unit independent angle using a double-precision floating-point
    /// internal representation.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public struct AngleD : IComparable, IComparable<AngleD>, IEquatable<AngleD>, IFormattable
    {
        /// <summary>
        /// A value that specifies the size of a single degree, in revolutions.
        /// </summary>
        public const double Degree = 0.002777777777777778;

        /// <summary>
        /// A value that specifies the size of a single minute, in revolutions.
        /// </summary>
        public const double Minute = 0.000046296296296296;

        /// <summary>
        /// A value that specifies the size of a single second, in revolutions.
        /// </summary>
        public const double Second = 0.000000771604938272;

        /// <summary>
        /// A value that specifies the size of a single radian, in revolutions.
        /// </summary>
        public const double Radian = 0.159154943091895336;

        /// <summary>
        /// A value that specifies the size of a single milliradian, in revolutions.
        /// </summary>
        public const double Milliradian = 0.0001591549431;

        /// <summary>
        /// A value that specifies the size of a single gradian, in revolutions.
        /// </summary>
        public const double Gradian = 0.0025;

        /// <summary>
        /// The internal representation of the angle.
        /// </summary>
        [FieldOffset(0)]
        [DataMember]
        double radians;

        [FieldOffset(0)]
        [DataMember]
        private int radiansInt;

        /// <summary>
        /// Initializes a new instance of the <see cref="AngleD"/> structure with the
        /// given unit dependant angle and unit type.
        /// </summary>
        /// <param name="angle">A unit dependant measure of the angle.</param>
        /// <param name="type">The type of unit the angle argument is.</param>
        public AngleD(double angle, AngleType type)
        {
            radiansInt = 0;
            switch (type)
            {
                case AngleType.Revolution:
                    radians = MathHelperDP.RevolutionsToRadians(angle);
                    break;

                case AngleType.Degree:
                    radians = MathHelperDP.DegreesToRadians(angle);
                    break;

                case AngleType.Radian:
                    radians = angle;
                    break;

                case AngleType.Gradian:
                    radians = MathHelperDP.GradiansToRadians(angle);
                    break;

                default:
                    radians = 0.0;
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
            radiansInt = 0;
            radians = arcLength / radius;
        }

        /// <summary>
        /// Wraps this <see cref="AngleD"/> to be in the range [π, -π].
        /// </summary>
        public void Wrap()
        {
            double newangle = Math.IEEERemainder(radians, MathHelperDP.TwoPi);

            if (newangle <= -MathHelperDP.Pi)
                newangle += MathHelperDP.TwoPi;
            else if (newangle > MathHelperDP.Pi)
                newangle -= MathHelperDP.TwoPi;

            radians = newangle;
        }

        /// <summary>
        /// Wraps this <see cref="AngleD"/> to be in the range [0, 2π).
        /// </summary>
        public void WrapPositive()
        {
            double newangle = radians % MathHelperDP.TwoPi;

            if (newangle < 0.0)
                newangle += MathHelperDP.TwoPi;

            radians = newangle;
        }

        /// <summary>
        /// Gets or sets the total number of revolutions this <see cref="AngleD"/> represents.
        /// </summary>
        public double Revolutions
        {
            get { return MathHelperDP.RadiansToRevolutions(radians); }
            set { radians = MathHelperDP.RevolutionsToRadians(value); }
        }

        /// <summary>
        /// Gets or sets the total number of degrees this <see cref="AngleD"/> represents.
        /// </summary>
        public double Degrees
        {
            get { return MathHelperDP.RadiansToDegrees(radians); }
            set { radians = MathHelperDP.DegreesToRadians(value); }
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
                double degrees = MathHelperDP.RadiansToDegrees(radians);

                if (degrees < 0)
                {
                    double degreesfloor = Math.Ceiling(degrees);
                    return (degrees - degreesfloor) * 60.0;
                }
                else
                {
                    double degreesfloor = Math.Floor(degrees);
                    return (degrees - degreesfloor) * 60.0;
                }
            }
            set
            {
                double degrees = MathHelperDP.RadiansToDegrees(radians);
                double degreesfloor = Math.Floor(degrees);

                degreesfloor += value / 60.0;
                radians = MathHelperDP.DegreesToRadians(degreesfloor);
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
                double degrees = MathHelperDP.RadiansToDegrees(radians);

                if (degrees < 0)
                {
                    double degreesfloor = Math.Ceiling(degrees);

                    double minutes = (degrees - degreesfloor) * 60.0;
                    double minutesfloor = Math.Ceiling(minutes);

                    return (minutes - minutesfloor) * 60.0;
                }
                else
                {
                    double degreesfloor = Math.Floor(degrees);

                    double minutes = (degrees - degreesfloor) * 60.0;
                    double minutesfloor = Math.Floor(minutes);

                    return (minutes - minutesfloor) * 60.0;
                }
            }
            set
            {
                double degrees = MathHelperDP.RadiansToDegrees(radians);
                double degreesfloor = Math.Floor(degrees);

                double minutes = (degrees - degreesfloor) * 60.0;
                double minutesfloor = Math.Floor(minutes);

                minutesfloor += value / 60.0;
                degreesfloor += minutesfloor / 60.0;
                radians = MathHelperDP.DegreesToRadians(degreesfloor);
            }
        }
        
        /// <summary>
        /// Gets or sets the total number of radians this <see cref="AngleD"/> represents.
        /// </summary>
        public double Radians
        {
            get { return radians; }
            set { radians = value; }
        }

        /// <summary>
        /// Gets or sets the total number of milliradians this <see cref="AngleD"/> represents.
        /// One milliradian is equal to 1/(2000π).
        /// </summary>
        public double Milliradians
        {
            get { return radians / (Milliradian * MathHelperDP.TwoPi); }
            set { radians = value * (Milliradian * MathHelperDP.TwoPi); }
        }

        /// <summary>
        /// Gets or sets the total number of gradians this <see cref="AngleD"/> represents.
        /// </summary>
        public double Gradians
        {
            get { return MathHelperDP.RadiansToGradians(radians); }
            set { radians = MathHelperDP.RadiansToGradians(value); }
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a right angle (i.e. 90° or π/2).
        /// </summary>
        public bool IsRight
        {
            get { return radians == MathHelperDP.PiOverTwo; }
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a straight angle (i.e. 180° or π).
        /// </summary>
        public bool IsStraight
        {
            get { return radians == MathHelperDP.Pi; }
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a full rotation angle (i.e. 360° or 2π).
        /// </summary>
        public bool IsFullRotation
        {
            get { return radians == MathHelperDP.TwoPi; }
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is an oblique angle (i.e. is not 90° or a multiple of 90°).
        /// </summary>
        public bool IsOblique
        {
            get { return WrapPositive(this).radians != MathHelperDP.PiOverTwo; }
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is an acute angle (i.e. less than 90° but greater than 0°).
        /// </summary>
        public bool IsAcute
        {
            get { return radians > 0.0 && radians < MathHelperDP.PiOverTwo; }
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is an obtuse angle (i.e. greater than 90° but less than 180°).
        /// </summary>
        public bool IsObtuse
        {
            get { return radians > MathHelperDP.PiOverTwo && radians < MathHelperDP.Pi; }
        }

        /// <summary>
        /// Gets a System.Boolean that determines whether this <see cref="AngleD"/>
        /// is a reflex angle (i.e. greater than 180° but less than 360°).
        /// </summary>
        public bool IsReflex
        {
            get { return radians > MathHelperDP.Pi && radians < MathHelperDP.TwoPi; }
        }

        /// <summary>
        /// Gets a <see cref="AngleD"/> instance that complements this angle (i.e. the two angles add to 90°).
        /// </summary>
        public AngleD Complement
        {
            get { return new AngleD(MathHelperDP.PiOverTwo - radians, AngleType.Radian); }
        }

        /// <summary>
        /// Gets a <see cref="AngleD"/> instance that supplements this angle (i.e. the two angles add to 180°).
        /// </summary>
        public AngleD Supplement
        {
            get { return new AngleD(MathHelperDP.Pi - radians, AngleType.Radian); }
        }

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
        public static AngleD Min(AngleD left, AngleD right)
        {
            if (left.radians < right.radians)
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
            if (left.radians > right.radians)
                return left;

            return right;
        }

        /// <summary>
        /// Adds two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to add.</param>
        /// <param name="right">The second object to add.</param>
        /// <returns>The value of the two objects added together.</returns>
        public static AngleD Add(AngleD left, AngleD right)
        {
            return new AngleD(left.radians + right.radians, AngleType.Radian);
        }

        /// <summary>
        /// Subtracts two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to subtract.</param>
        /// <param name="right">The second object to subtract.</param>
        /// <returns>The value of the two objects subtracted.</returns>
        public static AngleD Subtract(AngleD left, AngleD right)
        {
            return new AngleD(left.radians - right.radians, AngleType.Radian);
        }

        /// <summary>
        /// Multiplies two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to multiply.</param>
        /// <param name="right">The second object to multiply.</param>
        /// <returns>The value of the two objects multiplied together.</returns>
        public static AngleD Multiply(AngleD left, AngleD right)
        {
            return new AngleD(left.radians * right.radians, AngleType.Radian);
        }

        /// <summary>
        /// Divides two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The numerator object.</param>
        /// <param name="right">The denominator object.</param>
        /// <returns>The value of the two objects divided.</returns>
        public static AngleD Divide(AngleD left, AngleD right)
        {
            return new AngleD(left.radians / right.radians, AngleType.Radian);
        }

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the zero angle (i.e. 0°).
        /// </summary>
        public static AngleD ZeroAngle
        {
            get { return new AngleD(0.0, AngleType.Radian); }
        }

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the right angle (i.e. 90° or π/2).
        /// </summary>
        public static AngleD RightAngle
        {
            get { return new AngleD(MathHelperDP.PiOverTwo, AngleType.Radian); }
        }

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the straight angle (i.e. 180° or π).
        /// </summary>
        public static AngleD StraightAngle
        {
            get { return new AngleD(MathHelperDP.Pi, AngleType.Radian); }
        }

        /// <summary>
        /// Gets a new <see cref="AngleD"/> instance that represents the full rotation angle (i.e. 360° or 2π).
        /// </summary>
        public static AngleD FullRotationAngle
        {
            get { return new AngleD(MathHelperDP.TwoPi, AngleType.Radian); }
        }

        /// <summary>
        /// Returns a System.Boolean that indicates whether the values of two <see cref="AngleD"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>True if the left and right parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(AngleD left, AngleD right)
        {
            return left.radians == right.radians;
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
            return left.radians != right.radians;
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
            return left.radians < right.radians;
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
            return left.radians > right.radians;
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
            return left.radians <= right.radians;
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
            return left.radians >= right.radians;
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
            return new AngleD(-value.radians, AngleType.Radian);
        }

        /// <summary>
        /// Adds two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to add.</param>
        /// <param name="right">The second object to add.</param>
        /// <returns>The value of the two objects added together.</returns>
        public static AngleD operator +(AngleD left, AngleD right)
        {
            return new AngleD(left.radians + right.radians, AngleType.Radian);
        }

        /// <summary>
        /// Subtracts two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to subtract</param>
        /// <param name="right">The second object to subtract.</param>
        /// <returns>The value of the two objects subtracted.</returns>
        public static AngleD operator -(AngleD left, AngleD right)
        {
            return new AngleD(left.radians - right.radians, AngleType.Radian);
        }

        /// <summary>
        /// Multiplies two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The first object to multiply.</param>
        /// <param name="right">The second object to multiply.</param>
        /// <returns>The value of the two objects multiplied together.</returns>
        public static AngleD operator *(AngleD left, AngleD right)
        {
            return new AngleD(left.radians * right.radians, AngleType.Radian);
        }

        /// <summary>
        /// Divides two <see cref="AngleD"/> objects and returns the result.
        /// </summary>
        /// <param name="left">The numerator object.</param>
        /// <param name="right">The denominator object.</param>
        /// <returns>The value of the two objects divided.</returns>
        public static AngleD operator /(AngleD left, AngleD right)
        {
            return new AngleD(left.radians / right.radians, AngleType.Radian);
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

            double radians = ((AngleD)other).radians;

            if (this.radians > radians)
                return 1;

            if (this.radians < radians)
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
            if (this.radians > other.radians)
                return 1;

            if (this.radians < other.radians)
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
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, MathHelperDP.RadiansToDegrees(radians).ToString("0.##°"));
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

            return string.Format(CultureInfo.CurrentCulture, "{0}°", MathHelperDP.RadiansToDegrees(radians).ToString(format, CultureInfo.CurrentCulture));
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
            return string.Format(formatProvider, MathHelperDP.RadiansToDegrees(radians).ToString("0.##°"));
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

            return string.Format(formatProvider, "{0}°", MathHelperDP.RadiansToDegrees(radians).ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a hash code for this <see cref="AngleD"/> instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return radiansInt;
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
