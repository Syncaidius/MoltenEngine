using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "sbyte"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct SByte3
	{
		///<summary>The X component.</summary>
		public sbyte X;

		///<summary>The Y component.</summary>
		public sbyte Y;

		///<summary>The Z component.</summary>
		public sbyte Z;


		///<summary>The size of <see cref="SByte3"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(SByte3));

		public static SByte3 One = new SByte3(1, 1, 1);

		/// <summary>
        /// The X unit <see cref="SByte3"/>.
        /// </summary>
		public static SByte3 UnitX = new SByte3(1, 0, 0);

		/// <summary>
        /// The Y unit <see cref="SByte3"/>.
        /// </summary>
		public static SByte3 UnitY = new SByte3(0, 1, 0);

		/// <summary>
        /// The Z unit <see cref="SByte3"/>.
        /// </summary>
		public static SByte3 UnitZ = new SByte3(0, 0, 1);

		public static SByte3 Zero = new SByte3(0, 0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "SByte3"/>.</summary>
		public SByte3(sbyte x, sbyte y, sbyte z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="SByte3"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public SByte3(sbyte[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for SByte3.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		public unsafe SByte3(sbyte* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="SByte3"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector</param>
        /// <param name="result">When the method completes, contains the squared distance between the two vectors.</param>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
		public static void DistanceSquared(ref SByte3 value1, ref SByte3 value2, out sbyte result)
        {
            sbyte x = value1.X - value2.X;
            sbyte y = value1.Y - value2.Y;
            sbyte z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="SByte3"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
		public static sbyte DistanceSquared(ref SByte3 value1, ref SByte3 value2)
        {
            sbyte x = value1.X - value2.X;
            sbyte y = value1.Y - value2.Y;
            sbyte z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="SByte3"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public sbyte[] ToArray()
        {
            return new sbyte[] { X, Y, Z};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="SByte3"/>.
        /// </summary>
        /// <returns>A <see cref="SByte3"/> facing the opposite direction.</returns>
		public SByte3 Negate()
		{
			return new SByte3(-X, -Y, -Z);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="SByte3"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static SByte3 Lerp(ref SByte3 start, ref SByte3 end, float amount)
        {
			return new SByte3()
			{
				X = (sbyte)((1f - amount) * start.X + amount * end.X),
				Y = (sbyte)((1f - amount) * start.Y + amount * end.Y),
				Z = (sbyte)((1f - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Returns a <see cref="SByte3"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte3"/>.</param>
        /// <param name="right">The second source <see cref="SByte3"/>.</param>
        /// <returns>A <see cref="SByte3"/> containing the smallest components of the source vectors.</returns>
		public static SByte3 Min(SByte3 left, SByte3 right)
		{
			return new SByte3()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Returns a <see cref="SByte3"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="SByte3"/>.</param>
        /// <param name="right">The second source <see cref="SByte3"/>.</param>
        /// <returns>A <see cref="SByte3"/> containing the largest components of the source vectors.</returns>
		public static SByte3 Max(SByte3 left, SByte3 right)
		{
			return new SByte3()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(sbyte min, sbyte max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(SByte3 min, SByte3 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="SByte3"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static SByte3 operator +(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static SByte3 operator +(SByte3 left, sbyte right)
		{
			return new SByte3(left.X + right, left.Y + right, left.Z + right);
		}

		/// <summary>
        /// Assert a <see cref="SByte3"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="SByte3"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="SByte3"/>.</returns>
        public static SByte3 operator +(SByte3 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static SByte3 operator -(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static SByte3 operator -(SByte3 left, sbyte right)
		{
			return new SByte3(left.X - right, left.Y - right, left.Z - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="SByte3"/>.
        /// </summary>
        /// <param name="value">The <see cref="SByte3"/> to reverse.</param>
        /// <returns>The reversed <see cref="SByte3"/>.</returns>
        public static SByte3 operator -(SByte3 value)
        {
            return new SByte3(-value.X, -value.Y, -value.Z);
        }
#endregion

#region division operators
		public static SByte3 operator /(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static SByte3 operator /(SByte3 left, sbyte right)
		{
			return new SByte3(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static SByte3 operator *(SByte3 left, SByte3 right)
		{
			return new SByte3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static SByte3 operator *(SByte3 left, sbyte right)
		{
			return new SByte3(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="SByte3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static SByte3 Clamp(SByte3 value, sbyte min, sbyte max)
        {
			return new SByte3()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="SByte3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static SByte3 Clamp(SByte3 value, SByte3 min, SByte3 max)
        {
			return new SByte3()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
				Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
			};
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y or Z component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 2].</exception>
        
		public sbyte this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for SByte3 run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for SByte3 run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

