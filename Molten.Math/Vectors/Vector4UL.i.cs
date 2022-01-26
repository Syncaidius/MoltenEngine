using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "ulong"/> vector comprised of 4 components.</summary>
	public partial struct Vector4UL
	{

#region Operators - Cast
        public static explicit operator Vector4D(Vector4UL value)
		{
			return new Vector4D()
			{
				X = (double)value.X,
				Y = (double)value.Y,
				Z = (double)value.Z,
				W = (double)value.W,
			};
		}
#endregion
	}
}

