




using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 2 components.</summary>
	public partial struct Vector2F
	{
		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0F ? 0F : X > 1F ? 1F : X;
			Y = Y < 0F ? 0F : Y > 1F ? 1F : Y;
        }

		/// <summary>Rounds all components down to the nearest unit.</summary>
        public void Floor()
        {
			X = (float)Math.Floor(X);
			Y = (float)Math.Floor(Y);
        }

        /// <summary>Rounds all components up to the nearest unit.</summary>
        public void Ceiling()
        {
			X = (float)Math.Ceiling(X);
			Y = (float)Math.Ceiling(Y);
        }

		/// <summary>Truncate each near-zero component of the current vector towards zero.</summary>
        public void Truncate()
        {
			X = (Math.Abs(X) - 0.0001F < 0) ? 0 : X;
			Y = (Math.Abs(Y) - 0.0001F < 0) ? 0 : Y;
        }

		/// <summary>Updates the component values to the power of the specified value.</summary>
        /// <param name="power"></param>
        public void Pow(float power)
        {
			X = (float)Math.Pow(X, power);
			Y = (float)Math.Pow(Y, power);
        }

#region Static Methods
		/// <summary>Truncate each near-zero component of a vector towards zero.</summary>
        /// <param name="value">The Vector2F to be truncated.</param>
        /// <returns></returns>
        public static Vector2F Truncate(Vector2F value)
        {
            return new Vector2F()
            {
				X = (Math.Abs(value.X) - 0.0001F < 0) ? 0 : value.X,
				Y = (Math.Abs(value.Y) - 0.0001F < 0) ? 0 : value.X,
            };
        }
#endregion
	}
}

