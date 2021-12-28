using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct Byte2
	{
		///<summary>The X component.</summary>
		public byte X;

		///<summary>The Y component.</summary>
		public byte Y;


		///<summary>The size of <see cref="Byte2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte2));

		public static Byte2 One = new Byte2(1, 1);

		/// <summary>
        /// The X unit <see cref="Byte2"/>.
        /// </summary>
		public static Byte2 UnitX = new Byte2(1, 0);

		/// <summary>
        /// The Y unit <see cref="Byte2"/>.
        /// </summary>
		public static Byte2 UnitY = new Byte2(0, 1);

		public static Byte2 Zero = new Byte2(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Byte2"/>.</summary>
		public Byte2(byte x, byte y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Byte2"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Byte2(byte[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Byte2.");

			X = values[0];
			Y = values[1];
        }

		public unsafe Byte2(byte* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte2"/> vectors.
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
		public static void DistanceSquared(ref Byte2 value1, ref Byte2 value2, out byte result)
        {
            byte x = value1.X - value2.X;
            byte y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte2"/> vectors.
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
		public static byte DistanceSquared(ref Byte2 value1, ref Byte2 value2)
        {
            byte x = value1.X - value2.X;
            byte y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Byte2"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public byte[] ToArray()
        {
            return new byte[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Byte2"/>.
        /// </summary>
        /// <returns>A <see cref="Byte2"/> facing the opposite direction.</returns>
		public Byte2 Negate()
		{
			return new Byte2(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Byte2"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Byte2 Lerp(ref Byte2 start, ref Byte2 end, float amount)
        {
			return new Byte2()
			{
				X = (byte)((1f - amount) * start.X + amount * end.X),
				Y = (byte)((1f - amount) * start.Y + amount * end.Y),
			};
        }

		/// <summary>
        /// Returns a <see cref="Byte2"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the smallest components of the source vectors.</returns>
		public static Byte2 Min(Byte2 left, Byte2 right)
		{
			return new Byte2()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
			};
		}

		/// <summary>
        /// Returns a <see cref="Byte2"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Byte2"/>.</param>
        /// <param name="right">The second source <see cref="Byte2"/>.</param>
        /// <returns>A <see cref="Byte2"/> containing the largest components of the source vectors.</returns>
		public static Byte2 Max(Byte2 left, Byte2 right)
		{
			return new Byte2()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
			};
		}
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", X, Y);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Byte2"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Byte2 operator +(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X + right.X, left.Y + right.Y);
		}

		public static Byte2 operator +(Byte2 left, byte right)
		{
			return new Byte2(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="Byte2"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Byte2"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Byte2"/>.</returns>
        public static Byte2 operator +(Byte2 value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Byte2 operator -(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X - right.X, left.Y - right.Y);
		}

		public static Byte2 operator -(Byte2 left, byte right)
		{
			return new Byte2(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Byte2"/>.
        /// </summary>
        /// <param name="value">The <see cref="Byte2"/> to reverse.</param>
        /// <returns>The reversed <see cref="Byte2"/>.</returns>
        public static Byte2 operator -(Byte2 value)
        {
            return new Byte2(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static Byte2 operator /(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X / right.X, left.Y / right.Y);
		}

		public static Byte2 operator /(Byte2 left, byte right)
		{
			return new Byte2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Byte2 operator *(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X * right.X, left.Y * right.Y);
		}

		public static Byte2 operator *(Byte2 left, byte right)
		{
			return new Byte2(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods

#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X or Y component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>
        
		public byte this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Byte2 run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Byte2 run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

