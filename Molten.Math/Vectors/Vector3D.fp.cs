using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
	public partial struct Vector3D
	{
    	/// <summary>
        /// Gets a value indicting whether this instance is normalized.
        /// </summary>
        public bool IsNormalized
        {
            get => MathHelperDP.IsOne((X * X) + (Y * Y) + (Z * Z));
        }

        /// <summary>
        /// Orthonormalizes a list of vectors.
        /// </summary>
        /// <param name="destination">The list of orthonormalized vectors.</param>
        /// <param name="source">The list of vectors to orthonormalize.</param>
        /// <remarks>
        /// <para>Orthonormalization is the process of making all vectors orthogonal to each
        /// other and making all vectors of unit length. This means that any given vector will
        /// be orthogonal to any other given vector in the list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthonormalize(Vector3D[] destination, params Vector3D[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //Because we are making unit vectors, we can optimize the math for orthogonalization
            //and simplify the projection operation to remove the division.
            //q1 = m1 / |m1|
            //q2 = (m2 - (q1 ⋅ m2) * q1) / |m2 - (q1 ⋅ m2) * q1|
            //q3 = (m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2) / |m3 - (q1 ⋅ m3) * q1 - (q2 ⋅ m3) * q2|
            //q4 = (m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3) / |m4 - (q1 ⋅ m4) * q1 - (q2 ⋅ m4) * q2 - (q3 ⋅ m4) * q3|
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector3D newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= Dot(destination[r], newvector) * destination[r];

                newvector.Normalize();
                destination[i] = newvector;
            }
        }

        /// <summary>
        /// Converts the <see cref="Vector3D"/> into a unit vector.
        /// </summary>
        /// <param name="value">The <see cref="Vector3D"/> to normalize.</param>
        /// <returns>The normalized <see cref="Vector3D"/>.</returns>
        public static Vector3D Normalize(Vector3D value)
        {
            value.Normalize();
            return value;
        }

        /// <summary>
        /// Returns a normalized unit vector of the original vector.
        /// </summary>
        public Vector3D GetNormalized(bool allowZero = true)
        {
            double length = Length();
            if (!MathHelperDP.IsZero(length))
            {
                double inverse = 1.0D / length;
                return new Vector3D()
                {
			        X = this.X * inverse,
			        Y = this.Y * inverse,
			        Z = this.Z * inverse,
                };
            }
            else
            {
                return new Vector3D()
                {
                    X = 0,
                    Y = 1,
                    Z = 0,
                };
            }
        }

        /// <summary>
        /// Converts the vector into a unit vector.
        /// </summary>
        public void Normalize(bool allowZero = true)
        {
            double length = Length();
            if (!MathHelperDP.IsZero(length))
            {
                double inverse = 1.0D / length;
			    X = (X * inverse);
			    Y = (Y * inverse);
			    Z = (Z * inverse);
            }
            else
            {
                X = 0;
                Y = 1;
                Z = 0;
            }
        }

		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0D ? 0D : X > 1D ? 1D : X;
			Y = Y < 0D ? 0D : Y > 1D ? 1D : Y;
			Z = Z < 0D ? 0D : Z > 1D ? 1D : Z;
        }

		/// <summary>Rounds all components down to the nearest unit.</summary>
        public void Floor()
        {
			X = Math.Floor(X);
			Y = Math.Floor(Y);
			Z = Math.Floor(Z);
        }

        /// <summary>Rounds all components up to the nearest unit.</summary>
        public void Ceiling()
        {
			X = Math.Ceiling(X);
			Y = Math.Ceiling(Y);
			Z = Math.Ceiling(Z);
        }

		/// <summary>Truncate each near-zero component of the current vector towards zero.</summary>
        public void Truncate()
        {
			X = (Math.Abs(X) - 0.0001D < 0) ? 0 : X;
			Y = (Math.Abs(Y) - 0.0001D < 0) ? 0 : Y;
			Z = (Math.Abs(Z) - 0.0001D < 0) ? 0 : Z;
        }

		/// <summary>Updates the component values to the power of the specified value.</summary>
        /// <param name="power"></param>
        public void Pow(double power)
        {
			X = Math.Pow(X, power);
			Y = Math.Pow(Y, power);
			Z = Math.Pow(Z, power);
        }

#region Static Methods
		/// <summary>Truncate each near-zero component of a vector towards zero.</summary>
        /// <param name="value">The Vector3D to be truncated.</param>
        /// <returns></returns>
        public static Vector3D Truncate(Vector3D value)
        {
            return new Vector3D()
            {
				X = (Math.Abs(value.X) - 0.0001D < 0) ? 0 : value.X,
				Y = (Math.Abs(value.Y) - 0.0001D < 0) ? 0 : value.X,
				Z = (Math.Abs(value.Z) - 0.0001D < 0) ? 0 : value.X,
            };
        }
#endregion

#region Operators - Cast
        public static explicit operator Vector3L(Vector3D value)
		{
			return new Vector3L()
			{
				X = (long)value.X,
				Y = (long)value.Y,
				Z = (long)value.Z,
			};
		}
#endregion
	}
}

