namespace Molten.UI
{
    [Flags]
    public enum UIAnchorFlags
    {
        None = 0,

        Left = 1,

        Right = 1 << 1,

        Top = 1 << 2,

        Bottom = 1 << 3
    }
}
