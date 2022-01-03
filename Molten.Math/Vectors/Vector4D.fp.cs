using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "double"/> vector comprised of 4 components.</summary>
	public partial struct Vector4D
	{
		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0D ? 0D : X > 1D ? 1D : X;
			Y = Y < 0D ? 0D : Y > 1D ? 1D : Y;
			Z = Z < 0D ? 0D : Z > 1D ? 1D : Z;
			W = W < 0D ? 0D : W > 1D ? 1D : W;
        }

		/// <summary>Rounds all components down to the nearest unit.</summary>
        public void Floor()
        {
			X = Math.Floor(X);
			Y = Math.Floor(Y);
			Z = Math.Floor(Z);
			W = Math.Floor(W);
        }

        /// <summary>Rounds all components up to the nearest unit.</summary>
        public void Ceiling()
        {
			X = Math.Ceiling(X);
			Y = Math.Ceiling(Y);
			Z = Math.Ceiling(Z);
			W = Math.Ceiling(W);
        }

		/// <summary>Truncate each near-zero component of the current vector towards zero.</summary>
        public void Truncate()
        {
			X = (Math.Abs(X) - 0.0001D < 0) ? 0 : X;
			Y = (Math.Abs(Y) - 0.0001D < 0) ? 0 : Y;
			Z = (Math.Abs(Z) - 0.0001D < 0) ? 0 : Z;
			W = (Math.Abs(W) - 0.0001D < 0) ? 0 : W;
        }

		/// <summary>Updates the component values to the power of the specified value.</summary>
        /// <param name="power"></param>
        public void Pow(double power)
        {
			X = Math.Pow(X, power);
			Y = Math.Pow(Y, power);
			Z = Math.Pow(Z, power);
			W = Math.Pow(W, power);
        }

#region Static Methods
		/// <summary>Truncate each near-zero component of a vector towards zero.</summary>
        /// <param name="value">The Vector4D to be truncated.</param>
        /// <returns></returns>
        public static Vector4D Truncate(Vector4D value)
        {
            return new Vector4D()
            {
				X = (Math.Abs(value.X) - 0.0001D < 0) ? 0 : value.X,
				Y = (Math.Abs(value.Y) - 0.0001D < 0) ? 0 : value.X,
				Z = (Math.Abs(value.Z) - 0.0001D < 0) ? 0 : value.X,
				W = (Math.Abs(value.W) - 0.0001D < 0) ? 0 : value.X,
            };
        }
#endregion
	}
}

