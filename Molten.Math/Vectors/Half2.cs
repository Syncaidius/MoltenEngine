using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half2
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;


		///<summary>The size of <see cref="Half2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half2));

		public static Half2 One = new Half2((short)1, (short)1);

		/// <summary>
        /// The X unit <see cref="Half2"/>.
        /// </summary>
		public static Half2 UnitX = new Half2((short)1, 0);

		/// <summary>
        /// The Y unit <see cref="Half2"/>.
        /// </summary>
		public static Half2 UnitY = new Half2(0, (short)1);

		public static Half2 Zero = new Half2(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half2"/>.</summary>
		public Half2(short x, short y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half2"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half2(short[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Half2.");

			X = values[0];
			Y = values[1];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half2"/> vectors.
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
		public static void DistanceSquared(ref Half2 value1, ref Half2 value2, out short result)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half2"/> vectors.
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
		public static short DistanceSquared(ref Half2 value1, ref Half2 value2)
        {
            short x = value1.X - value2.X;
            short y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Half2"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public short[] ToArray()
        {
            return new short[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Half2"/>.
        /// </summary>
        /// <returns>A <see cref="Half2"/> facing the opposite direction.</returns>
		public Half2 Negate()
		{
			return new Half2(-X, -Y);
		}
#endregion

#region Add operators
		public static Half2 operator +(Half2 left, Half2 right)
		{
			return new Half2(left.X + right.X, left.Y + right.Y);
		}

		public static Half2 operator +(Half2 left, short right)
		{
			return new Half2(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Half2 operator -(Half2 left, Half2 right)
		{
			return new Half2(left.X - right.X, left.Y - right.Y);
		}

		public static Half2 operator -(Half2 left, short right)
		{
			return new Half2(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Half2 operator /(Half2 left, Half2 right)
		{
			return new Half2(left.X / right.X, left.Y / right.Y);
		}

		public static Half2 operator /(Half2 left, short right)
		{
			return new Half2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Half2 operator *(Half2 left, Half2 right)
		{
			return new Half2(left.X * right.X, left.Y * right.Y);
		}

		public static Half2 operator *(Half2 left, short right)
		{
			return new Half2(left.X * right, left.Y * right);
		}
#endregion

#region Properties

#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X or Y component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>
        
		public short this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half2 run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half2 run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

