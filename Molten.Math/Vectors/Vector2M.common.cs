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
	}
}

