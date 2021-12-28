using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector4NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;

		///<summary>The Z component.</summary>
		public nuint Z;

		///<summary>The W component.</summary>
		public nuint W;


		///<summary>The size of <see cref="Vector4NU"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4NU));

		public static Vector4NU One = new Vector4NU(1U, 1U, 1U, 1U);

		/// <summary>
        /// The X unit <see cref="Vector4NU"/>.
        /// </summary>
		public static Vector4NU UnitX = new Vector4NU(1U, 0U, 0U, 0U);

		/// <summary>
        /// The Y unit <see cref="Vector4NU"/>.
        /// </summary>
		public static Vector4NU UnitY = new Vector4NU(0U, 1U, 0U, 0U);

		/// <summary>
        /// The Z unit <see cref="Vector4NU"/>.
        /// </summary>
		public static Vector4NU UnitZ = new Vector4NU(0U, 0U, 1U, 0U);

		/// <summary>
        /// The W unit <see cref="Vector4NU"/>.
        /// </summary>
		public static Vector4NU UnitW = new Vector4NU(0U, 0U, 0U, 1U);

		public static Vector4NU Zero = new Vector4NU(0U, 0U, 0U, 0U);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector4NU"/>.</summary>
		public Vector4NU(nuint x, nuint y, nuint z, nuint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector4NU"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the vector. This must be an array with 4 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector4NU(nuint[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be 4 and only 4 input values for Vector4NU.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
        }

		public unsafe Vector4NU(nuint* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
			W = ptr[3];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4NU"/> vectors.
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
		public static void DistanceSquared(ref Vector4NU value1, ref Vector4NU value2, out nuint result)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;
            nuint z = value1.Z - value2.Z;
            nuint w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4NU"/> vectors.
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
		public static nuint DistanceSquared(ref Vector4NU value1, ref Vector4NU value2)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;
            nuint z = value1.Z - value2.Z;
            nuint w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector4NU"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public nuint[] ToArray()
        {
            return new nuint[] { X, Y, Z, W};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector4NU"/>.
        /// </summary>
        /// <returns>A <see cref="Vector4NU"/> facing the opposite direction.</returns>
		public Vector4NU Negate()
		{
			return new Vector4NU(-X, -Y, -Z, -W);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4NU"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector4NU Lerp(ref Vector4NU start, ref Vector4NU end, float amount)
        {
			return new Vector4NU()
			{
				X = (nuint)((1f - amount) * start.X + amount * end.X),
				Y = (nuint)((1f - amount) * start.Y + amount * end.Y),
				Z = (nuint)((1f - amount) * start.Z + amount * end.Z),
				W = (nuint)((1f - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector4NU"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4NU"/>.</param>
        /// <param name="right">The second source <see cref="Vector4NU"/>.</param>
        /// <returns>A <see cref="Vector4NU"/> containing the smallest components of the source vectors.</returns>
		public static Vector4NU Min(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector4NU"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4NU"/>.</param>
        /// <param name="right">The second source <see cref="Vector4NU"/>.</param>
        /// <returns>A <see cref="Vector4NU"/> containing the largest components of the source vectors.</returns>
		public static Vector4NU Max(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
				W = (left.W > right.W) ? left.W : right.W,
			};
		}
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4NU"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Vector4NU operator +(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4NU operator +(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector4NU"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector4NU"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector4NU"/>.</returns>
        public static Vector4NU operator +(Vector4NU value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector4NU operator -(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4NU operator -(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector4NU"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4NU"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector4NU"/>.</returns>
        public static Vector4NU operator -(Vector4NU value)
        {
            return new Vector4NU(-value.X, -value.Y, -value.Z, -value.W);
        }
#endregion

#region division operators
		public static Vector4NU operator /(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4NU operator /(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4NU operator *(Vector4NU left, Vector4NU right)
		{
			return new Vector4NU(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4NU operator *(Vector4NU left, nuint right)
		{
			return new Vector4NU(left.X * right, left.Y * right, left.Z * right, left.W * right);
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
        /// <value>The value of the X, Y, Z or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        
		public nuint this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4NU run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4NU run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

