using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector2D
	{
		///<summary>The X component.</summary>
		public double X;

		///<summary>The Y component.</summary>
		public double Y;


		///<summary>The size of <see cref="Vector2D"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2D));

		public static Vector2D One = new Vector2D(1D, 1D);

		/// <summary>
        /// The X unit <see cref="Vector2D"/>.
        /// </summary>
		public static Vector2D UnitX = new Vector2D(1D, 0D);

		/// <summary>
        /// The Y unit <see cref="Vector2D"/>.
        /// </summary>
		public static Vector2D UnitY = new Vector2D(0D, 1D);

		public static Vector2D Zero = new Vector2D(0D, 0D);

		///<summary>Creates a new instance of <see cref = "Vector2D"/>.</summary>
		public Vector2D(double x, double y)
		{
			X = x;
			Y = y;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2D"/> vectors.
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
		public static void DistanceSquared(ref Vector2D value1, ref Vector2D value2, out double result)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2D"/> vectors.
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
		public static double DistanceSquared(ref Vector2D value1, ref Vector2D value2)
        {
            double x = value1.X - value2.X;
            double y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }


#endregion

#region Add operators
		public static Vector2D operator +(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2D operator +(Vector2D left, double right)
		{
			return new Vector2D(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2D operator -(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2D operator -(Vector2D left, double right)
		{
			return new Vector2D(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2D operator /(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2D operator /(Vector2D left, double right)
		{
			return new Vector2D(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2D operator *(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2D operator *(Vector2D left, double right)
		{
			return new Vector2D(left.X * right, left.Y * right);
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
        
		public double this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2D run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2D run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

