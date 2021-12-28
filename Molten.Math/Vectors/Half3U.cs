using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half3U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>The Z component.</summary>
		public ushort Z;


		///<summary>The size of <see cref="Half3U"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half3U));

		public static Half3U One = new Half3U((ushort)1, (ushort)1, (ushort)1);

		/// <summary>
        /// The X unit <see cref="Half3U"/>.
        /// </summary>
		public static Half3U UnitX = new Half3U((ushort)1, 0, 0);

		/// <summary>
        /// The Y unit <see cref="Half3U"/>.
        /// </summary>
		public static Half3U UnitY = new Half3U(0, (ushort)1, 0);

		/// <summary>
        /// The Z unit <see cref="Half3U"/>.
        /// </summary>
		public static Half3U UnitZ = new Half3U(0, 0, (ushort)1);

		public static Half3U Zero = new Half3U(0, 0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half3U"/>.</summary>
		public Half3U(ushort x, ushort y, ushort z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half3U"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half3U(ushort[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Half3U.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }

		public unsafe Half3U(ushort* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half3U"/> vectors.
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
		public static void DistanceSquared(ref Half3U value1, ref Half3U value2, out ushort result)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;
            ushort z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half3U"/> vectors.
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
		public static ushort DistanceSquared(ref Half3U value1, ref Half3U value2)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;
            ushort z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Half3U"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public ushort[] ToArray()
        {
            return new ushort[] { X, Y, Z};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Half3U"/>.
        /// </summary>
        /// <returns>A <see cref="Half3U"/> facing the opposite direction.</returns>
		public Half3U Negate()
		{
			return new Half3U(-X, -Y, -Z);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Half3U"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Half3U Lerp(ref Half3U start, ref Half3U end, float amount)
        {
			return new Half3U()
			{
				X = (ushort)((1f - amount) * start.X + amount * end.X),
				Y = (ushort)((1f - amount) * start.Y + amount * end.Y),
				Z = (ushort)((1f - amount) * start.Z + amount * end.Z),
			};
        }

		/// <summary>
        /// Returns a <see cref="Half3U"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half3U"/>.</param>
        /// <param name="right">The second source <see cref="Half3U"/>.</param>
        /// <returns>A <see cref="Half3U"/> containing the smallest components of the source vectors.</returns>
		public static Half3U Min(Half3U left, Half3U right)
		{
			return new Half3U()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>
        /// Returns a <see cref="Half3U"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half3U"/>.</param>
        /// <param name="right">The second source <see cref="Half3U"/>.</param>
        /// <returns>A <see cref="Half3U"/> containing the largest components of the source vectors.</returns>
		public static Half3U Max(Half3U left, Half3U right)
		{
			return new Half3U()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
			};
		}

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(ushort min, ushort max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Half3U min, Half3U max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", X, Y, Z);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half3U"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Half3U operator +(Half3U left, Half3U right)
		{
			return new Half3U(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Half3U operator +(Half3U left, ushort right)
		{
			return new Half3U(left.X + right, left.Y + right, left.Z + right);
		}

		/// <summary>
        /// Assert a <see cref="Half3U"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Half3U"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Half3U"/>.</returns>
        public static Half3U operator +(Half3U value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Half3U operator -(Half3U left, Half3U right)
		{
			return new Half3U(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Half3U operator -(Half3U left, ushort right)
		{
			return new Half3U(left.X - right, left.Y - right, left.Z - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Half3U"/>.
        /// </summary>
        /// <param name="value">The <see cref="Half3U"/> to reverse.</param>
        /// <returns>The reversed <see cref="Half3U"/>.</returns>
        public static Half3U operator -(Half3U value)
        {
            return new Half3U(-value.X, -value.Y, -value.Z);
        }
#endregion

#region division operators
		public static Half3U operator /(Half3U left, Half3U right)
		{
			return new Half3U(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Half3U operator /(Half3U left, ushort right)
		{
			return new Half3U(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Half3U operator *(Half3U left, Half3U right)
		{
			return new Half3U(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Half3U operator *(Half3U left, ushort right)
		{
			return new Half3U(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half3U"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half3U Clamp(Half3U value, ushort min, ushort max)
        {
			return new Half3U()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half3U"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half3U Clamp(Half3U value, Half3U min, Half3U max)
        {
			return new Half3U()
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
        
		public ushort this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half3U run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half3U run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

