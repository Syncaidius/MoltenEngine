using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector4D
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

