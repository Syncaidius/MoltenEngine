namespace Molten.Math
{
	///<summary>A <see cref = "ushort"/> vector comprised of 3 components.</summary>
	public partial struct Half3U
	{
		///<summary>The X component.</summary>
		public ushort X;

		///<summary>The Y component.</summary>
		public ushort Y;

		///<summary>The Z component.</summary>
		public ushort Z;

		///<summary>Creates a new instance of <see cref = "Half3U"/></summary>
		public Half3U(ushort x, ushort y, ushort z)
		{
			X = x;
			Y = y;
			Z = z;
		}

#region operators
		public static Half3U operator +(Half3U left, Half3U right)
		{
			return new Half3U()
			{
				X = left.X + right.X,
				Y = left.Y + right.Y,
				Z = left.Z + right.Z,
			};
		}
#endregion
	}
}

