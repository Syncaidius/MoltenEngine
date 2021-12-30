using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 4 components.</summary>
	public partial struct Vector4M
	{
		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0M ? 0M : X > 1M ? 1M : X;
			Y = Y < 0M ? 0M : Y > 1M ? 1M : Y;
			Z = Z < 0M ? 0M : Z > 1M ? 1M : Z;
			W = W < 0M ? 0M : W > 1M ? 1M : W;
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
	}
}

