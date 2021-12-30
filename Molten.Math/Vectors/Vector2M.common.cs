using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 2 components.</summary>
	public partial struct Vector2M
	{
		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0M ? 0M : X > 1M ? 1M : X;
			Y = Y < 0M ? 0M : Y > 1M ? 1M : Y;
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
				X = (Math.Abs(X) - 0.0001M < 0) ? 0 : X;
				Y = (Math.Abs(Y) - 0.0001M < 0) ? 0 : X;
        }

#region Static Methods
		/// <summary>Truncate each near-zero component of a vector towards zero.</summary>
        /// <param name="vec">The Vector2M to be truncated.</param>
        /// <returns></returns>
        public static Vector2M Truncate(Vector2M value)
        {
            return new Vector2M()
            {
				X = (Math.Abs(value.X) - 0.0001M < 0) ? 0 : value.X,
				Y = (Math.Abs(value.Y) - 0.0001M < 0) ? 0 : value.X,
            };
        }
#endregion
	}
}

