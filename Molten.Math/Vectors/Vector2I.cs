using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "int"/> vector comprised of 2 components.</summary>
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

		///<summary>Creates a new instance of <see cref = "Vector2I"/>.</summary>
		public Vector2I(int x, int y)
		{
			X = x;
			Y = y;
		}

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

