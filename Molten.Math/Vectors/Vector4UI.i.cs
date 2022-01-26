using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "uint"/> vector comprised of 4 components.</summary>
	public partial struct Vector4UI
	{

#region Operators - Cast
        public static explicit operator Vector4F(Vector4UI value)
		{
			return new Vector4F()
			{
				X = (float)value.X,
				Y = (float)value.Y,
				Z = (float)value.Z,
				W = (float)value.W,
			};
		}
#endregion
	}
}

