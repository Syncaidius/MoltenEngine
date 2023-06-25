namespace Molten.Graphics
{
    /// <summary>
    /// Has an identical layout to a D3D Box struct.
    /// </summary>
    public struct ResourceRegion
    {
        public uint Left;

        public uint Top;

        public uint Front;

        public uint Right;

        public uint Bottom;

        public uint Back;

        public ResourceRegion(uint left, uint top, uint front, uint right, uint bottom, uint back)
        {
            Left = left;
            Top = top;
            Front = front;
            Right = right;
            Bottom = bottom;
            Back = back;
        }

        public ResourceRegion(uint left, uint top, uint right, uint bottom)
        {
            Left = left;
            Top = top;
            Front = 0;
            Right = right;
            Bottom = bottom;
            Back = 1;
        }

        public bool Contains(ResourceRegion region)
        {
            return region.Left >= Left
                && region.Top >= Top
                && region.Front >= Front
                && region.Right <= Right
                && region.Bottom <= Bottom
                && region.Back <= Back;
        }

        public uint Width
        {
            get => Right - Left;
            set => Right = Left + value;
        }

        public uint Height
        {
            get => Bottom - Top;
            set => Bottom = Top + value;
        }

        public uint Depth
        {
            get => Back - Front;
            set => Back = Front + value;
        }

        public uint X
        {
            get => Left;
            set
            {
                Right = value + Width; 
                Left = value;
            }
        }

        public uint Y
        {

            get => Top;
            set
            {
                Bottom = value + Height;
                Top = value;
            }
        }
    }
}
