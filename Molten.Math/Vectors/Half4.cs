using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half4
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;

		///<summary>The Z component.</summary>
		public short Z;

		///<summary>The W component.</summary>
		public short W;


		///<summary>The size of <see cref="Half4"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half4));

		public static Half4 One = new Half4((short)1, (short)1, (short)1, (short)1);

		/// <summary>
        /// The X unit <see cref="Half4"/>.
        /// </summary>
		public static Half4 UnitX = new Half4((short)1, 0, 0, 0);

		/// <summary>
        /// The Y unit <see cref="Half4"/>.
        /// </summary>
		public static Half4 UnitY = new Half4(0, (short)1, 0, 0);

		/// <summary>
        /// The Z unit <see cref="Half4"/>.
        /// </summary>
		public static Half4 UnitZ = new Half4(0, 0, (short)1, 0);

		/// <summary>
        /// The W unit <see cref="Half4"/>.
        /// </summary>
		public static Half4 UnitW = new Half4(0, 0, 0, (short)1);

		public static Half4 Zero = new Half4(0, 0, 0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half4"/>.</summary>
		public Half4(short x, short y, short z, short w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half4"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the vector. This must be an array with 4 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half4(short[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be 4 and only 4 input values for Half4.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half4"/> vectors.
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
		public static void DistanceSquared(ref Half4 value1, ref Half4 value2, out short result)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;
            short z = value1.Z - value2.Z;
            short w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half4"/> vectors.
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
		public static short DistanceSquared(ref Half4 value1, ref Half4 value2)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;
            short z = value1.Z - value2.Z;
            short w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }


#endregion

#region Add operators
		public static Half4 operator +(Half4 left, Half4 right)
		{
			return new Half4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Half4 operator +(Half4 left, short right)
		{
			return new Half4(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Half4 operator -(Half4 left, Half4 right)
		{
			return new Half4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Half4 operator -(Half4 left, short right)
		{
			return new Half4(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Half4 operator /(Half4 left, Half4 right)
		{
			return new Half4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Half4 operator /(Half4 left, short right)
		{
			return new Half4(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Half4 operator *(Half4 left, Half4 right)
		{
			return new Half4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Half4 operator *(Half4 left, short right)
		{
			return new Half4(left.X * right, left.Y * right, left.Z * right, left.W * right);
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
        
		public short this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for Half4 run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for Half4 run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

