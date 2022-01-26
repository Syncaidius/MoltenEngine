using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "ulong"/> vector comprised of 3 components.</summary>
	public partial struct Vector3UL
	{

#region Operators - Cast
        public static explicit operator Vector3D(Vector3UL value)
		{
			return new Vector3D()
			{
				X = (double)value.X,
				Y = (double)value.Y,
				Z = (double)value.Z,
			};
		}
#endregion
	}
}

