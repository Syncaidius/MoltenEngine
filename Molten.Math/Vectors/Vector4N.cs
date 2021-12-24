using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "nint"/> vector comprised of 4 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=0)]
	public partial struct Vector4N
	{
		///<summary>The X component.</summary>
		public nint X;

		///<summary>The Y component.</summary>
		public nint Y;

		///<summary>The Z component.</summary>
		public nint Z;

		///<summary>The W component.</summary>
		public nint W;


		///<summary>The size of <see cref="Vector4N"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4N));

		public static Vector4N One = new Vector4N(1, 1, 1, 1);

		public static Vector4N Zero = new Vector4N(0, 0, 0, 0);

		///<summary>Creates a new instance of <see cref = "Vector4N"/></summary>
		public Vector4N(nint x, nint y, nint z, nint w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4N"/> vectors.
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
		public static void DistanceSquared(ref Vector4N value1, ref Vector4N value2, out nint result)
        {
            nint x = value1.X - value2.X;
            nint y = value1.Y - value2.Y;
            nint z = value1.Z - value2.Z;
            nint w = value1.W - value2.W;

            result = (x * x) + (y * y) + (z * z) + (w * w);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4N"/> vectors.
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
		public static nint DistanceSquared(ref Vector4N value1, ref Vector4N value2)
        {
            nint x = value1.X - value2.X;
            nint y = value1.Y - value2.Y;
            nint z = value1.Z - value2.Z;
            nint w = value1.W - value2.W;

            return (x * x) + (y * y) + (z * z) + (w * w);
        }


#endregion

#region Add operators
		public static Vector4N operator +(Vector4N left, Vector4N right)
		{
			return new Vector4N(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		public static Vector4N operator +(Vector4N left, nint right)
		{
			return new Vector4N(left.X + right, left.Y + right, left.Z + right, left.W + right);
		}
#endregion

#region Subtract operators
		public static Vector4N operator -(Vector4N left, Vector4N right)
		{
			return new Vector4N(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		public static Vector4N operator -(Vector4N left, nint right)
		{
			return new Vector4N(left.X - right, left.Y - right, left.Z - right, left.W - right);
		}
#endregion

#region division operators
		public static Vector4N operator /(Vector4N left, Vector4N right)
		{
			return new Vector4N(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
		}

		public static Vector4N operator /(Vector4N left, nint right)
		{
			return new Vector4N(left.X / right, left.Y / right, left.Z / right, left.W / right);
		}
#endregion

#region Multiply operators
		public static Vector4N operator *(Vector4N left, Vector4N right)
		{
			return new Vector4N(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
		}

		public static Vector4N operator *(Vector4N left, nint right)
		{
			return new Vector4N(left.X * right, left.Y * right, left.Z * right, left.W * right);
		}
#endregion

#region Indexers
		public nint this[int index]
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4N run from 0 to 3, inclusive.");
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
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4N run from 0 to 3, inclusive.");
			}
		}
#endregion
	}
}

