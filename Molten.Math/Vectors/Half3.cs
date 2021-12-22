namespace Molten.Math
{
	///<summary>A <see cref = "short"/> vector comprised of 3 components.</summary>
	public partial struct Half3
	{
		///<summary>The X component.</summary>
		public short X;

		///<summary>The Y component.</summary>
		public short Y;

		///<summary>The Z component.</summary>
		public short Z;

		///<summary>Creates a new instance of <see cref = "Half3"/></summary>
		public Half3(short x, short y, short z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Half3 operator +(Half3 left, Half3 right)
		{
			return new Half3()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
				Z = left.Z + right.Z,
			};
		}
#endregion
	}
}

