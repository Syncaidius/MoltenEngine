namespace Molten.Graphics.Dxgi
{
    /// <summary>
    /// See: https://docs.microsoft.com/en-us/windows/win32/direct3ddxgi/dxgi-usage
    /// </summary>
    [Flags]
    public enum DxgiUsage : uint
    {
        None = 0,

        ShaderInput = 1 << (0 + 4),

        RenderTargetOutput = 1 << (1 + 4),

        BackBuffer = 1 << (2+4),

        Shared = 1 << (3 + 4),

        ReadOnly = 1 << (4 + 4),

        DiscardOnPresnet = 1 << (5+4),

        UnorderedAccess = 1 << (6 + 4),
    }
}
