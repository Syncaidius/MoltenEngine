using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "byte"/> vector comprised of 3 components.</summary>
	public partial struct Byte3
	{

#region Operators - Cast
        public static explicit operator Vector3F(Byte3 value)
		{
			return new Vector3F()
			{
				X = (float)value.X,
				Y = (float)value.Y,
				Z = (float)value.Z,
			};
		}
#endregion
	}
}

