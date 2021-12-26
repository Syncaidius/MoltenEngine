using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 2 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector2UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;


		///<summary>The size of <see cref="Vector2UI"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2UI));

		public static Vector2UI One = new Vector2UI(1U, 1U);

		/// <summary>
        /// The X unit <see cref="Vector2UI"/>.
        /// </summary>
		public static Vector2UI UnitX = new Vector2UI(1U, 0U);

		/// <summary>
        /// The Y unit <see cref="Vector2UI"/>.
        /// </summary>
		public static Vector2UI UnitY = new Vector2UI(0U, 1U);

		public static Vector2UI Zero = new Vector2UI(0U, 0U);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector2UI"/>.</summary>
		public Vector2UI(uint x, uint y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2UI"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2UI(uint[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2UI.");

			X = values[0];
			Y = values[1];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2UI"/> vectors.
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
		public static void DistanceSquared(ref Vector2UI value1, ref Vector2UI value2, out uint result)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2UI"/> vectors.
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
		public static uint DistanceSquared(ref Vector2UI value1, ref Vector2UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2UI"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public uint[] ToArray()
        {
            return new uint[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2UI"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2UI"/> facing the opposite direction.</returns>
		public Vector2UI Negate()
		{
			return new Vector2UI(-X, -Y);
		}
#endregion

#region Add operators
		public static Vector2UI operator +(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2UI operator +(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X + right, left.Y + right);
		}
#endregion

#region Subtract operators
		public static Vector2UI operator -(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2UI operator -(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X - right, left.Y - right);
		}
#endregion

#region division operators
		public static Vector2UI operator /(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2UI operator /(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2UI operator *(Vector2UI left, Vector2UI right)
		{
			return new Vector2UI(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2UI operator *(Vector2UI left, uint right)
		{
			return new Vector2UI(left.X * right, left.Y * right);
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
        
		public uint this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2UI run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2UI run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

