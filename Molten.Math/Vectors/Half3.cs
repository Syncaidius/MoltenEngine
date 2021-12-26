using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 3 components.</summary>
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

		///<summary>Creates a new instance of <see cref = "Half3"/>.</summary>
		public Half3(short x, short y, short z)
		{
			X = x;
			Y = y;
			Z = z;
		}

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

