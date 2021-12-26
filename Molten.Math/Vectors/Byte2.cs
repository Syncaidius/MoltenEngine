using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "byte"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public partial struct Byte2
	{
		///<summary>The X component.</summary>
		public byte X;

		///<summary>The Y component.</summary>
		public byte Y;


		///<summary>The size of <see cref="Byte2"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Byte2));

		public static Byte2 One = new Byte2(1, 1);

		/// <summary>
        /// The X unit <see cref="Byte2"/>.
        /// </summary>
		public static Byte2 UnitX = new Byte2(1, 0);

		/// <summary>
        /// The Y unit <see cref="Byte2"/>.
        /// </summary>
		public static Byte2 UnitY = new Byte2(0, 1);

		public static Byte2 Zero = new Byte2(0, 0);

		///<summary>Creates a new instance of <see cref = "Byte2"/>.</summary>
		public Byte2(byte x, byte y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte2"/> vectors.
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
		public static void DistanceSquared(ref Byte2 value1, ref Byte2 value2, out byte result)
        {
            byte x = value1.X - value2.X;
            byte y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Byte2"/> vectors.
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
		public static byte DistanceSquared(ref Byte2 value1, ref Byte2 value2)
        {
            byte x = value1.X - value2.X;
            byte y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }


#endregion

#region Add operators
		public static Byte2 operator +(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X + right.X, left.Y + right.Y);
		}

		public static Byte2 operator +(Byte2 left, byte right)
		{
			return new Byte2(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Byte2 operator -(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X - right.X, left.Y - right.Y);
		}

		public static Byte2 operator -(Byte2 left, byte right)
		{
			return new Byte2(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Byte2 operator /(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X / right.X, left.Y / right.Y);
		}

		public static Byte2 operator /(Byte2 left, byte right)
		{
			return new Byte2(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Byte2 operator *(Byte2 left, Byte2 right)
		{
			return new Byte2(left.X * right.X, left.Y * right.Y);
		}

		public static Byte2 operator *(Byte2 left, byte right)
		{
			return new Byte2(left.X * right, left.Y * right);
		}
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X or Y component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 1].</exception>
        
		public byte this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Byte2 run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Byte2 run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

