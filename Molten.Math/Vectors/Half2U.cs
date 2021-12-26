using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half2U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;


		///<summary>The size of <see cref="Half2U"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half2U));

		public static Half2U One = new Half2U((ushort)1, (ushort)1);

		/// <summary>
        /// The X unit <see cref="Half2U"/>.
        /// </summary>
		public static Half2U UnitX = new Half2U((ushort)1, 0);

		/// <summary>
        /// The Y unit <see cref="Half2U"/>.
        /// </summary>
		public static Half2U UnitY = new Half2U(0, (ushort)1);

		public static Half2U Zero = new Half2U(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Half2U"/>.</summary>
		public Half2U(ushort x, ushort y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Half2U"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Half2U(ushort[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Half2U.");

			X = values[0];
			Y = values[1];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half2U"/> vectors.
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
		public static void DistanceSquared(ref Half2U value1, ref Half2U value2, out ushort result)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half2U"/> vectors.
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
		public static ushort DistanceSquared(ref Half2U value1, ref Half2U value2)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Half2U"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public ushort[] ToArray()
        {
            return new ushort[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Half2U"/>.
        /// </summary>
        /// <returns>A <see cref="Half2U"/> facing the opposite direction.</returns>
		public Half2U Negate()
		{
			return new Half2U(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Half2U"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Half2U start, ref Half2U end, float amount, out Half2U result)
        {
			result.X = (ushort)((1f - amount) * start.X + amount * end.X);
			result.Y = (ushort)((1f - amount) * start.Y + amount * end.Y);
        }
#endregion

#region Add operators
		public static Half2U operator +(Half2U left, Half2U right)
		{
			return new Half2U(left.X + right.X, left.Y + right.Y);
		}

		public static Half2U operator +(Half2U left, ushort right)
		{
			return new Half2U(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Half2U operator -(Half2U left, Half2U right)
		{
			return new Half2U(left.X - right.X, left.Y - right.Y);
		}

		public static Half2U operator -(Half2U left, ushort right)
		{
			return new Half2U(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Half2U operator /(Half2U left, Half2U right)
		{
			return new Half2U(left.X / right.X, left.Y / right.Y);
		}

		public static Half2U operator /(Half2U left, ushort right)
		{
			return new Half2U(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Half2U operator *(Half2U left, Half2U right)
		{
			return new Half2U(left.X * right.X, left.Y * right.Y);
		}

		public static Half2U operator *(Half2U left, ushort right)
		{
			return new Half2U(left.X * right, left.Y * right);
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
        
		public ushort this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half2U run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half2U run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

