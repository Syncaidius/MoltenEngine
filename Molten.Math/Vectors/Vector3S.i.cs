using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "short"/> vector comprised of 3 components.</summary>
	public partial struct Vector3S
	{

#region Operators - Cast
        public static explicit operator Vector3F(Vector3S value)
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

