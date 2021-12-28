using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half3
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;

		///<summary>The Z component.</summary>
		public short Z;


		///<summary>The size of <see cref="Half3"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half3));

		public static Half3 One = new Half3((short)1, (short)1, (short)1);

		/// <summary>
        /// The X unit <see cref="Half3"/>.
        /// </summary>
		public static Half3 UnitX = new Half3((short)1, 0, 0);

		/// <summary>
        /// The Y unit <see cref="Half3"/>.
        /// </summary>
		public static Half3 UnitY = new Half3(0, (short)1, 0);

		/// <summary>
        /// The Z unit <see cref="Half3"/>.
        /// </summary>
		public static Half3 UnitZ = new Half3(0, 0, (short)1);

		public static Half3 Zero = new Half3(0, 0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half3"/>.</summary>
		public Half3(short x, short y, short z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half3"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half3(short[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Half3.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		public unsafe Half3(short* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half3"/> vectors.
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
		public static void DistanceSquared(ref Half3 value1, ref Half3 value2, out short result)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;
            short z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half3"/> vectors.
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
		public static short DistanceSquared(ref Half3 value1, ref Half3 value2)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;
            short z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Half3"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public short[] ToArray()
        {
            return new short[] { X, Y, Z};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Half3"/>.
        /// </summary>
        /// <returns>A <see cref="Half3"/> facing the opposite direction.</returns>
		public Half3 Negate()
		{
			return new Half3(-X, -Y, -Z);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Half3"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Half3 Lerp(ref Half3 start, ref Half3 end, float amount)
        {
			return new Half3()
			{
				X = (short)((1f - amount) * start.X + amount * end.X),
				Y = (short)((1f - amount) * start.Y + amount * end.Y),
				Z = (short)((1f - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Returns a <see cref="Half3"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half3"/>.</param>
        /// <param name="right">The second source <see cref="Half3"/>.</param>
        /// <returns>A <see cref="Half3"/> containing the smallest components of the source vectors.</returns>
		public static Half3 Min(Half3 left, Half3 right)
		{
			return new Half3()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Returns a <see cref="Half3"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half3"/>.</param>
        /// <param name="right">The second source <see cref="Half3"/>.</param>
        /// <returns>A <see cref="Half3"/> containing the largest components of the source vectors.</returns>
		public static Half3 Max(Half3 left, Half3 right)
		{
			return new Half3()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(short min, short max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Half3 min, Half3 max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Half3 operator +(Half3 left, Half3 right)
		{
			return new Half3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Half3 operator +(Half3 left, short right)
		{
			return new Half3(left.X + right, left.Y + right, left.Z + right);
		}

		/// <summary>
        /// Assert a <see cref="Half3"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Half3"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Half3"/>.</returns>
        public static Half3 operator +(Half3 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Half3 operator -(Half3 left, Half3 right)
		{
			return new Half3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Half3 operator -(Half3 left, short right)
		{
			return new Half3(left.X - right, left.Y - right, left.Z - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Half3"/>.
        /// </summary>
        /// <param name="value">The <see cref="Half3"/> to reverse.</param>
        /// <returns>The reversed <see cref="Half3"/>.</returns>
        public static Half3 operator -(Half3 value)
        {
            return new Half3(-value.X, -value.Y, -value.Z);
        }
#endregion

#region division operators
		public static Half3 operator /(Half3 left, Half3 right)
		{
			return new Half3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Half3 operator /(Half3 left, short right)
		{
			return new Half3(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Half3 operator *(Half3 left, Half3 right)
		{
			return new Half3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Half3 operator *(Half3 left, short right)
		{
			return new Half3(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half3 Clamp(Half3 value, short min, short max)
        {
			return new Half3()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half3"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half3 Clamp(Half3 value, Half3 min, Half3 max)
        {
			return new Half3()
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
        
		public short this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half3 run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half3 run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

