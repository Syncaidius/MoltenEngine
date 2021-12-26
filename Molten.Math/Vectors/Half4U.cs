using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=2)]
	public partial struct Half4U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>The Z component.</summary>
		public ushort Z;

		///<summary>The W component.</summary>
		public ushort W;


		///<summary>The size of <see cref="Half4U"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Half4U));

		public static Half4U One = new Half4U((ushort)1, (ushort)1, (ushort)1, (ushort)1);

		/// <summary>
        /// The X unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitX = new Half4U((ushort)1, 0, 0, 0);

		/// <summary>
        /// The Y unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitY = new Half4U(0, (ushort)1, 0, 0);

		/// <summary>
        /// The Z unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitZ = new Half4U(0, 0, (ushort)1, 0);

		/// <summary>
        /// The W unit <see cref="Half4U"/>.
        /// </summary>
		public static Half4U UnitW = new Half4U(0, 0, 0, (ushort)1);

		public static Half4U Zero = new Half4U(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Half4U"/>.</summary>
		public Half4U(ushort x, ushort y, ushort z, ushort w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Half4U"/> vectors.
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
		public static void DistanceSquared(ref Half4U value1, ref Half4U value2, out ushort result)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;
            ushort z = value1.Z - value2.Z;
            ushort w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Half4U"/> vectors.
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
		public static ushort DistanceSquared(ref Half4U value1, ref Half4U value2)
        {
            ushort x = value1.X - value2.X;
            ushort y = value1.Y - value2.Y;
            ushort z = value1.Z - value2.Z;
            ushort w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }


#endregion

#region Add operators
		public static Half4U operator +(Half4U left, Half4U right)
		{
			return new Half4U(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Half4U operator +(Half4U left, ushort right)
		{
			return new Half4U(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Half4U operator -(Half4U left, Half4U right)
		{
			return new Half4U(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Half4U operator -(Half4U left, ushort right)
		{
			return new Half4U(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Half4U operator /(Half4U left, Half4U right)
		{
			return new Half4U(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Half4U operator /(Half4U left, ushort right)
		{
			return new Half4U(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Half4U operator *(Half4U left, Half4U right)
		{
			return new Half4U(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Half4U operator *(Half4U left, ushort right)
		{
			return new Half4U(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>
        
		public ushort this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
					case 3: return W;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half4U run from 0 to 3, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
					case 3: W = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Half4U run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

