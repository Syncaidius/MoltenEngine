using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nuint"/> vector comprised of two components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector2NU
	{
		///<summary>The X component.</summary>
		public nuint X;

		///<summary>The Y component.</summary>
		public nuint Y;


		///<summary>The size of <see cref="Vector2NU"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector2NU));

		public static Vector2NU One = new Vector2NU(1U, 1U);

		/// <summary>
        /// The X unit <see cref="Vector2NU"/>.
        /// </summary>
		public static Vector2NU UnitX = new Vector2NU(1U, 0U);

		/// <summary>
        /// The Y unit <see cref="Vector2NU"/>.
        /// </summary>
		public static Vector2NU UnitY = new Vector2NU(0U, 1U);

		public static Vector2NU Zero = new Vector2NU(0U, 0U);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector2NU"/>.</summary>
		public Vector2NU(nuint x, nuint y)
		{
			X = x;
			Y = y;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector2NU"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X and Y components of the vector. This must be an array with 2 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector2NU(nuint[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 2)
                throw new ArgumentOutOfRangeException("values", "There must be 2 and only 2 input values for Vector2NU.");

			X = values[0];
			Y = values[1];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2NU"/> vectors.
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
		public static void DistanceSquared(ref Vector2NU value1, ref Vector2NU value2, out nuint result)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;

            result = (x * x) + (y * y);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector2NU"/> vectors.
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
		public static nuint DistanceSquared(ref Vector2NU value1, ref Vector2NU value2)
        {
            nuint x = value1.X - value2.X;
            nuint y = value1.Y - value2.Y;

            return (x * x) + (y * y);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector2NU"/>.
        /// </summary>
        /// <returns>A two-element array containing the components of the vector.</returns>
        public nuint[] ToArray()
        {
            return new nuint[] { X, Y};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector2NU"/>.
        /// </summary>
        /// <returns>A <see cref="Vector2NU"/> facing the opposite direction.</returns>
		public Vector2NU Negate()
		{
			return new Vector2NU(-X, -Y);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector2NU"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector2NU Lerp(ref Vector2NU start, ref Vector2NU end, float amount)
        {
			return new Vector2NU()
			{
				X = (nuint)((1f - amount) * start.X + amount * end.X),
				Y = (nuint)((1f - amount) * start.Y + amount * end.Y),
			};
        }
#endregion

#region Add operators
		public static Vector2NU operator +(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2NU operator +(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X + right, left.Y + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector2NU"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector2NU"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector2NU"/>.</returns>
        public static Vector2NU operator +(Vector2NU value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector2NU operator -(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2NU operator -(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X - right, left.Y - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector2NU"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector2NU"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector2NU"/>.</returns>
        public static Vector2NU operator -(Vector2NU value)
        {
            return new Vector2NU(-value.X, -value.Y);
        }
#endregion

#region division operators
		public static Vector2NU operator /(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2NU operator /(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X / right, left.Y / right);
		}
#endregion

#region Multiply operators
		public static Vector2NU operator *(Vector2NU left, Vector2NU right)
		{
			return new Vector2NU(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2NU operator *(Vector2NU left, nuint right)
		{
			return new Vector2NU(left.X * right, left.Y * right);
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
        
		public nuint this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2NU run from 0 to 1, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector2NU run from 0 to 1, inclusive.");
			}
		}
#endregion
	}
}

