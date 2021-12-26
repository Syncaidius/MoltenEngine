using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
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

