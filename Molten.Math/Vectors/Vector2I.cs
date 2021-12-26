using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector2I
	{
		///<summary>The X component.</summary>
		public int X;

		///<summary>The Y component.</summary>
		public int Y;


		///<summary>The size of <see cref="Vector2I"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2I));

		public static Vector2I One = new Vector2I(1, 1);

		/// <summary>
        /// The X unit <see cref="Vector2I"/>.
        /// </summary>
		public static Vector2I UnitX = new Vector2I(1, 0);

		/// <summary>
        /// The Y unit <see cref="Vector2I"/>.
        /// </summary>
		public static Vector2I UnitY = new Vector2I(0, 1);

		public static Vector2I Zero = new Vector2I(0, 0);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector2I"/>.</summary>
		public Vector2I(int x, int y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2I"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2I(int[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2I.");

			X = values[0];
			Y = values[1];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2I"/> vectors.
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
		public static void DistanceSquared(ref Vector2I value1, ref Vector2I value2, out int result)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2I"/> vectors.
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
		public static int DistanceSquared(ref Vector2I value1, ref Vector2I value2)
        {
            int x = value1.X - value2.X;
            int y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public int[] ToArray()
        {
            return new int[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2I"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2I"/> facing the opposite direction.</returns>
		public Vector2I Negate()
		{
			return new Vector2I(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2I"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Vector2I start, ref Vector2I end, float amount, out Vector2I result)
        {
			result.X = (int)((1f - amount) * start.X + amount * end.X);
			result.Y = (int)((1f - amount) * start.Y + amount * end.Y);
        }
#endregion

#region Add operators
		public static Vector2I operator +(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2I operator +(Vector2I left, int right)
		{
			return new Vector2I(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2I operator -(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2I operator -(Vector2I left, int right)
		{
			return new Vector2I(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2I operator /(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2I operator /(Vector2I left, int right)
		{
			return new Vector2I(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2I operator *(Vector2I left, Vector2I right)
		{
			return new Vector2I(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2I operator *(Vector2I left, int right)
		{
			return new Vector2I(left.X * right, left.Y * right);
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
        
		public int this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2I run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2I run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

