using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "uint"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=4)]
	public partial struct Vector4UI
	{
		///<summary>The X component.</summary>
		public uint X;

		///<summary>The Y component.</summary>
		public uint Y;

		///<summary>The Z component.</summary>
		public uint Z;

		///<summary>The W component.</summary>
		public uint W;


		///<summary>The size of <see cref="Vector4UI"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4UI));

		public static Vector4UI One = new Vector4UI(1U, 1U, 1U, 1U);

		public static Vector4UI Zero = new Vector4UI(0U, 0U, 0U, 0U);

		///<summary>Creates a new instance of <see cref = "Vector4UI"/></summary>
		public Vector4UI(uint x, uint y, uint z, uint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4UI"/> vectors.
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
		public static void DistanceSquared(ref Vector4UI value1, ref Vector4UI value2, out uint result)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;
            uint w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4UI"/> vectors.
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
		public static uint DistanceSquared(ref Vector4UI value1, ref Vector4UI value2)
        {
            uint x = value1.X - value2.X;
            uint y = value1.Y - value2.Y;
            uint z = value1.Z - value2.Z;
            uint w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }


#endregion

#region Add operators
		public static Vector4UI operator +(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4UI operator +(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4UI operator -(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4UI operator -(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4UI operator /(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4UI operator /(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4UI operator *(Vector4UI left, Vector4UI right)
		{
			return new Vector4UI(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4UI operator *(Vector4UI left, uint right)
		{
			return new Vector4UI(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion

#region Indexers
		public uint this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4UI run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4UI run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

