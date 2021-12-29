using System;
using System.Runtime.InteropServices;
using System.Globalization;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector4D : IFormattable
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;

		///<summary>The Z component.</summary>
		public double Z;

		///<summary>The W component.</summary>
		public double W;


		///<summary>The size of <see cref="Vector4D"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4D));

		public static Vector4D One = new Vector4D(1D, 1D, 1D, 1D);

		/// <summary>
        /// The X unit <see cref="Vector4D"/>.
        /// </summary>
		public static Vector4D UnitX = new Vector4D(1D, 0D, 0D, 0D);

		/// <summary>
        /// The Y unit <see cref="Vector4D"/>.
        /// </summary>
		public static Vector4D UnitY = new Vector4D(0D, 1D, 0D, 0D);

		/// <summary>
        /// The Z unit <see cref="Vector4D"/>.
        /// </summary>
		public static Vector4D UnitZ = new Vector4D(0D, 0D, 1D, 0D);

		/// <summary>
        /// The W unit <see cref="Vector4D"/>.
        /// </summary>
		public static Vector4D UnitW = new Vector4D(0D, 0D, 0D, 1D);

		public static Vector4D Zero = new Vector4D(0D, 0D, 0D, 0D);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector4D"/>.</summary>
		public Vector4D(double x, double y, double z, double w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector4D"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the vector. This must be an array with 4 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector4D(double[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be 4 and only 4 input values for Vector4D.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
        }

		public unsafe Vector4D(double* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
			W = ptr[3];
		}
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4D"/> vectors.
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
		public static void DistanceSquared(ref Vector4D value1, ref Vector4D value2, out double result)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4D"/> vectors.
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
		public static double DistanceSquared(ref Vector4D value1, ref Vector4D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;
            double z = value1.Z - value2.Z;
            double w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector4D"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public double[] ToArray()
        {
            return new double[] { X, Y, Z, W};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector4D"/>.
        /// </summary>
        /// <returns>A <see cref="Vector4D"/> facing the opposite direction.</returns>
		public Vector4D Negate()
		{
			return new Vector4D(-X, -Y, -Z, -W);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector4D Lerp(ref Vector4D start, ref Vector4D end, float amount)
        {
			return new Vector4D()
			{
				X = (double)((1f - amount) * start.X + amount * end.X),
				Y = (double)((1f - amount) * start.Y + amount * end.Y),
				Z = (double)((1f - amount) * start.Z + amount * end.Z),
				W = (double)((1f - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Returns a <see cref="Vector4D"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the smallest components of the source vectors.</returns>
		public static Vector4D Min(Vector4D left, Vector4D right)
		{
			return new Vector4D()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Returns a <see cref="Vector4D"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4D"/>.</param>
        /// <param name="right">The second source <see cref="Vector4D"/>.</param>
        /// <returns>A <see cref="Vector4D"/> containing the largest components of the source vectors.</returns>
		public static Vector4D Max(Vector4D left, Vector4D right)
		{
			return new Vector4D()
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
        public void Clamp(double min, double max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
			W = W < min ? min : W > max ? max : W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector4D min, Vector4D max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
			W = W < min.W ? min.W : W > max.W ? max.W : W;
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector4D"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector4D"/> source vector</param>
        /// <param name="right">Second <see cref="Vector4D"/> source vector.</param>
        /// <param name="result">When the method completes, contains the dot product of the two <see cref="Vector4D"/> vectors.</param>
        public static double Dot(Vector4D left, Vector4D right)
        {
			return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);
        }

		/// <summary>
        /// Performs a Hermite spline interpolation.
        /// </summary>
        /// <param name="value1">First source position <see cref="Vector4D"/> vector.</param>
        /// <param name="tangent1">First source tangent <see cref="Vector4D"/> vector.</param>
        /// <param name="value2">Second source position <see cref="Vector4D"/> vector.</param>
        /// <param name="tangent2">Second source tangent <see cref="Vector4D"/> vector.</param>
        /// <param name="amount">Weighting factor.</param>
        public static Vector4D Hermite(ref Vector4D value1, ref Vector4D tangent1, ref Vector4D value2, ref Vector4D tangent2, double amount)
        {
            double squared = amount * amount;
            double cubed = amount * squared;
            double part1 = ((2.0D * cubed) - (3.0D * squared)) + 1.0D;
            double part2 = (-2.0D * cubed) + (3.0D * squared);
            double part3 = (cubed - (2.0D * squared)) + amount;
            double part4 = cubed - squared;

			return new Vector4D()
			{
				X = (((value1.X * part1) + (value2.X * part2)) + (tangent1.X * part3)) + (tangent2.X * part4),
				Y = (((value1.Y * part1) + (value2.Y * part2)) + (tangent1.Y * part3)) + (tangent2.Y * part4),
				Z = (((value1.Z * part1) + (value2.Z * part2)) + (tangent1.Z * part3)) + (tangent2.Z * part4),
				W = (((value1.W * part1) + (value2.W * part2)) + (tangent1.W * part3)) + (tangent2.W * part4),
			};
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4D"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
#endregion

#region Add operators
		public static Vector4D operator +(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4D operator +(Vector4D left, double right)
		{
			return new Vector4D(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector4D"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector4D"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector4D"/>.</returns>
        public static Vector4D operator +(Vector4D value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector4D operator -(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4D operator -(Vector4D left, double right)
		{
			return new Vector4D(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector4D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4D"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector4D"/>.</returns>
        public static Vector4D operator -(Vector4D value)
        {
            return new Vector4D(-value.X, -value.Y, -value.Z, -value.W);
        }
#endregion

#region division operators
		public static Vector4D operator /(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4D operator /(Vector4D left, double right)
		{
			return new Vector4D(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4D operator *(Vector4D left, Vector4D right)
		{
			return new Vector4D(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4D operator *(Vector4D left, double right)
		{
			return new Vector4D(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion

#region Properties

#endregion

#region Static Methods
		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4D"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector4D Clamp(Vector4D value, double min, double max)
        {
			return new Vector4D()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
				W = value.W < min ? min : value.W > max ? max : value.W,
			};
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4D"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector4D Clamp(Vector4D value, Vector4D min, Vector4D max)
        {
			return new Vector4D()
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
        
		public double this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4D run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4D run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

