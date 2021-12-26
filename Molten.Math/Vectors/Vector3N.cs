using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector3N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;

		///<summary>The Z component.</summary>
		public nint Z;


		///<summary>The size of <see cref="Vector3N"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3N));

		public static Vector3N One = new Vector3N(1, 1, 1);

		/// <summary>
        /// The X unit <see cref="Vector3N"/>.
        /// </summary>
		public static Vector3N UnitX = new Vector3N(1, 0, 0);

		/// <summary>
        /// The Y unit <see cref="Vector3N"/>.
        /// </summary>
		public static Vector3N UnitY = new Vector3N(0, 1, 0);

		/// <summary>
        /// The Z unit <see cref="Vector3N"/>.
        /// </summary>
		public static Vector3N UnitZ = new Vector3N(0, 0, 1);

		public static Vector3N Zero = new Vector3N(0, 0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector3N"/>.</summary>
		public Vector3N(nint x, nint y, nint z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3N"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3N(nint[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3N.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3N"/> vectors.
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
		public static void DistanceSquared(ref Vector3N value1, ref Vector3N value2, out nint result)
        {
            nint x = value1.X - value2.X;
            nint y = value1.Y - value2.Y;
            nint z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3N"/> vectors.
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
		public static nint DistanceSquared(ref Vector3N value1, ref Vector3N value2)
        {
            nint x = value1.X - value2.X;
            nint y = value1.Y - value2.Y;
            nint z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }


#endregion

#region Add operators
		public static Vector3N operator +(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3N operator +(Vector3N left, nint right)
		{
			return new Vector3N(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3N operator -(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3N operator -(Vector3N left, nint right)
		{
			return new Vector3N(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3N operator /(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3N operator /(Vector3N left, nint right)
		{
			return new Vector3N(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3N operator *(Vector3N left, Vector3N right)
		{
			return new Vector3N(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3N operator *(Vector3N left, nint right)
		{
			return new Vector3N(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Properties

#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y or Z component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 2].</exception>
        
		public nint this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3N run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3N run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

