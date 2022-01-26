using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "double"/> vector comprised of 2 components.</summary>
	public partial struct Vector2D
	{
		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0D ? 0D : X > 1D ? 1D : X;
			Y = Y < 0D ? 0D : Y > 1D ? 1D : Y;
        }

		/// <summary>Rounds all components down to the nearest unit.</summary>
        public void Floor()
        {
			X = Math.Floor(X);
			Y = Math.Floor(Y);
        }

        /// <summary>Rounds all components up to the nearest unit.</summary>
        public void Ceiling()
        {
			X = Math.Ceiling(X);
			Y = Math.Ceiling(Y);
        }

		/// <summary>Truncate each near-zero component of the current vector towards zero.</summary>
        public void Truncate()
        {
			X = (Math.Abs(X) - 0.0001D < 0) ? 0 : X;
			Y = (Math.Abs(Y) - 0.0001D < 0) ? 0 : Y;
        }

		/// <summary>Updates the component values to the power of the specified value.</summary>
        /// <param name="power"></param>
        public void Pow(double power)
        {
			X = Math.Pow(X, power);
			Y = Math.Pow(Y, power);
        }

#region Static Methods
		/// <summary>Truncate each near-zero component of a vector towards zero.</summary>
        /// <param name="value">The Vector2D to be truncated.</param>
        /// <returns></returns>
        public static Vector2D Truncate(Vector2D value)
        {
            return new Vector2D()
            {
				X = (Math.Abs(value.X) - 0.0001D < 0) ? 0 : value.X,
				Y = (Math.Abs(value.Y) - 0.0001D < 0) ? 0 : value.X,
            };
        }
#endregion

#region Operators - Cast
        public static explicit operator Vector2L(Vector2D value)
		{
			return new Vector2L()
			{
				X = (long)value.X,
				Y = (long)value.Y,
			};
		}
#endregion
	}
}

