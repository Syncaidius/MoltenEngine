namespace Molten.Graphics
{
    public struct Thickness
    {
        public static readonly Thickness One = new Thickness(1f);

        public static readonly Thickness None = new Thickness(0f);

        public float Left;

        public float Top;

        public float Right;

        public float Bottom;

        public Thickness(float value)
        {
            Left = Top = Right = Bottom = value;
        }

        public Thickness(float leftRight, float topBottom)
        {
            Left = Right = leftRight;
            Top = Bottom = topBottom;
        }

        public Thickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public void Zero()
        {
            Left = Top = Right = Bottom = 0;
        }
    }
}
