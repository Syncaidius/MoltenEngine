namespace Molten.Graphics;

public struct ResourceMap
{
    public unsafe void* Ptr;

    public ulong RowPitch;

    public ulong DepthPitch;

    public unsafe ResourceMap(void* ptr, ulong rowPitch, ulong depthPitch)
    {
        Ptr = ptr;
        RowPitch = rowPitch;
        DepthPitch = depthPitch;
    }
}
