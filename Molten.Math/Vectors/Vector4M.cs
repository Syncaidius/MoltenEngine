using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=16)]
	public partial struct Vector4M
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>The Z component.</summary>
		public decimal Z;

		///<summary>The W component.</summary>
		public decimal W;


		///<summary>The size of <see cref="Vector4M"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4M));

		public static Vector4M One = new Vector4M(1M, 1M, 1M, 1M);

		/// <summary>
        /// The X unit <see cref="Vector4M"/>.
        /// </summary>
		public static Vector4M UnitX = new Vector4M(1M, 0M, 0M, 0M);

		/// <summary>
        /// The Y unit <see cref="Vector4M"/>.
        /// </summary>
		public static Vector4M UnitY = new Vector4M(0M, 1M, 0M, 0M);

		/// <summary>
        /// The Z unit <see cref="Vector4M"/>.
        /// </summary>
		public static Vector4M UnitZ = new Vector4M(0M, 0M, 1M, 0M);

		/// <summary>
        /// The W unit <see cref="Vector4M"/>.
        /// </summary>
		public static Vector4M UnitW = new Vector4M(0M, 0M, 0M, 1M);

		public static Vector4M Zero = new Vector4M(0M, 0M, 0M, 0M);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector4M"/>.</summary>
		public Vector4M(decimal x, decimal y, decimal z, decimal w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector4M"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the vector. This must be an array with 4 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector4M(decimal[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be 4 and only 4 input values for Vector4M.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4M"/> vectors.
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
		public static void DistanceSquared(ref Vector4M value1, ref Vector4M value2, out decimal result)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;
            decimal z = value1.Z - value2.Z;
            decimal w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4M"/> vectors.
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
		public static decimal DistanceSquared(ref Vector4M value1, ref Vector4M value2)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;
            decimal z = value1.Z - value2.Z;
            decimal w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector4M"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public decimal[] ToArray()
        {
            return new decimal[] { X, Y, Z, W};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector4M"/>.
        /// </summary>
        /// <returns>A <see cref="Vector4M"/> facing the opposite direction.</returns>
		public Vector4M Negate()
		{
			return new Vector4M(-X, -Y, -Z, -W);
		}
#endregion

#region Add operators
		public static Vector4M operator +(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4M operator +(Vector4M left, decimal right)
		{
			return new Vector4M(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4M operator -(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4M operator -(Vector4M left, decimal right)
		{
			return new Vector4M(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4M operator /(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4M operator /(Vector4M left, decimal right)
		{
			return new Vector4M(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4M operator *(Vector4M left, Vector4M right)
		{
			return new Vector4M(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4M operator *(Vector4M left, decimal right)
		{
			return new Vector4M(left.X * right, left.Y * right, left.Z * right, left.W * right);
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
        
		public decimal this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4M run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4M run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

