using System.Runtime.InteropServices;
using System;

namespace Molten
{
	///<summary>A <see cref = "int"/> vector comprised of 3 components.</summary>
	public partial struct Vector3I
	{

#region Operators - Cast
        public static explicit operator Vector3F(Vector3I value)
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

