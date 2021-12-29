using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half4U : IFormattable
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>The Z component.</summary>
		public ushort Z;

		///<summary>The W component.</summary>
		public ushort W;


		///<summary>The size of <see cref="Half4U"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half4U));

		public static Half4U One = new Half4U((ushort)1, (ushort)1, (ushort)1, (ushort)1);

		/// <summary>
        /// The X unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitX = new Half4U((ushort)1, 0, 0, 0);

		/// <summary>
        /// The Y unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitY = new Half4U(0, (ushort)1, 0, 0);

		/// <summary>
        /// The Z unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitZ = new Half4U(0, 0, (ushort)1, 0);

		/// <summary>
        /// The W unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitW = new Half4U(0, 0, 0, (ushort)1);

		public static Half4U Zero = new Half4U(0, 0, 0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half4U"/>.</summary>
		public Half4U(ushort x, ushort y, ushort z, ushort w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half4U"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the vector. This must be an array with 4 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half4U(ushort[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be 4 and only 4 input values for Half4U.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
        }

		public unsafe Half4U(ushort* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
			W = ptr[3];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half4U"/> vectors.
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
		public static void DistanceSquared(ref Half4U value1, ref Half4U value2, out ushort result)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;
            ushort z = value1.Z - value2.Z;
            ushort w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half4U"/> vectors.
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
		public static ushort DistanceSquared(ref Half4U value1, ref Half4U value2)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;
            ushort z = value1.Z - value2.Z;
            ushort w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Half4U"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public ushort[] ToArray()
        {
            return new ushort[] { X, Y, Z, W};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Half4U"/>.
        /// </summary>
        /// <returns>A <see cref="Half4U"/> facing the opposite direction.</returns>
		public Half4U Negate()
		{
			return new Half4U(-X, -Y, -Z, -W);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Half4U"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Half4U Lerp(ref Half4U start, ref Half4U end, float amount)
        {
			return new Half4U()
			{
				X = (ushort)((1f - amount) * start.X + amount * end.X),
				Y = (ushort)((1f - amount) * start.Y + amount * end.Y),
				Z = (ushort)((1f - amount) * start.Z + amount * end.Z),
				W = (ushort)((1f - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Returns a <see cref="Half4U"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half4U"/>.</param>
        /// <param name="right">The second source <see cref="Half4U"/>.</param>
        /// <returns>A <see cref="Half4U"/> containing the smallest components of the source vectors.</returns>
		public static Half4U Min(Half4U left, Half4U right)
		{
			return new Half4U()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Returns a <see cref="Half4U"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Half4U"/>.</param>
        /// <param name="right">The second source <see cref="Half4U"/>.</param>
        /// <returns>A <see cref="Half4U"/> containing the largest components of the source vectors.</returns>
		public static Half4U Max(Half4U left, Half4U right)
		{
			return new Half4U()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
				W = (left.W > right.W) ? left.W : right.W,
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
			W = W < min ? min : W > max ? max : W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Half4U min, Half4U max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
			W = W < min.W ? min.W : W > max.W ? max.W : W;
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Half4U"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Half4U"/> source vector</param>
        /// <param name="right">Second <see cref="Half4U"/> source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two <see cref="Half4U"/> vectors.</param>
        public static ushort Dot(Half4U left, Half4U right)
        {
			return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Half4U"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Half4U"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Half4U"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Half4U"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Half4U Hermite(ref Half4U value1, ref Half4U tangent1, ref Half4U value2, ref Half4U tangent2, ushort amount)
        {
            float squared = amount * amount;
            float cubed = amount * squared;
            float part1 = ((2.0F * cubed) - (3.0F * squared)) + 1.0F;
            float part2 = (-2.0F * cubed) + (3.0F * squared);
            float part3 = (cubed - (2.0F * squared)) + amount;
            float part4 = cubed - squared;

			return new Half4U()
			{
				X = (ushort)((((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4)),
				Y = (ushort)((((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4)),
				Z = (ushort)((((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4)),
				W = (ushort)((((value1.W * part1) + (value2.W * part2)) + (tangent1.W * part3)) + (tangent2.W * part4)),
			};
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Half4U"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Half4U operator +(Half4U left, Half4U right)
		{
			return new Half4U(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Half4U operator +(Half4U left, ushort right)
		{
			return new Half4U(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}

		/// <summary>
        /// Assert a <see cref="Half4U"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Half4U"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Half4U"/>.</returns>
        public static Half4U operator +(Half4U value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Half4U operator -(Half4U left, Half4U right)
		{
			return new Half4U(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Half4U operator -(Half4U left, ushort right)
		{
			return new Half4U(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Half4U"/>.
        /// </summary>
        /// <param name="value">The <see cref="Half4U"/> to reverse.</param>
        /// <returns>The reversed <see cref="Half4U"/>.</returns>
        public static Half4U operator -(Half4U value)
        {
            return new Half4U(-value.X, -value.Y, -value.Z, -value.W);
        }
#endregion

#region division operators
		public static Half4U operator /(Half4U left, Half4U right)
		{
			return new Half4U(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Half4U operator /(Half4U left, ushort right)
		{
			return new Half4U(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Half4U operator *(Half4U left, Half4U right)
		{
			return new Half4U(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Half4U operator *(Half4U left, ushort right)
		{
			return new Half4U(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half4U"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half4U Clamp(Half4U value, ushort min, ushort max)
        {
			return new Half4U()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
				W = value.W < min ? min : value.W > max ? max : value.W,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Half4U"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Half4U Clamp(Half4U value, Half4U min, Half4U max)
        {
			return new Half4U()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
				Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
				W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W,
			};
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        
		public ushort this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
					case 3: return W;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half4U run from 0 to 3, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
					case 3: W = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half4U run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

