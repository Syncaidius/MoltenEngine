namespace Molten.Graphics.Dxgi
{
    [Flags]
    public enum WindowAssociationFlags : uint
    {
        None = 0U,

        NoWindowChanges = 1U,

        NoAltEnter = 2U,

        NoPrintScreen = 4U,
    }
}
