using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "double"/> vector comprised of 3 components.</summary>
	public partial struct Vector3D
	{
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
	}
}

