using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "long"/> vector comprised of 3 components.</summary>
	public partial struct Vector3L
	{

#region Operators - Cast
        public static explicit operator Vector3D(Vector3L value)
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

