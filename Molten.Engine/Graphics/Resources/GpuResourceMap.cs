namespace Molten.Graphics;

public struct GpuResourceMap
{
    public unsafe void* Ptr;

    public ulong RowPitch;

    public ulong DepthPitch;

    public unsafe GpuResourceMap(void* ptr, ulong rowPitch, ulong depthPitch)
    {
        Ptr = ptr;
        RowPitch = rowPitch;
        DepthPitch = depthPitch;
    }
}
