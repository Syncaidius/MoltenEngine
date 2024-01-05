using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal static class DescriptionInterop
{
    internal static HeapType ToHeapType(this GraphicsResourceFlags flags)
    {
        if (flags.Has(GraphicsResourceFlags.CpuRead)
            || flags.Has(GraphicsResourceFlags.CpuWrite)
            || flags.Has(GraphicsResourceFlags.GpuRead)
            || flags.Has(GraphicsResourceFlags.GpuWrite))
        {
            // GPU read.
            if (flags.Has(GraphicsResourceFlags.GpuRead))
            {
                // GPU read/write.
                if (flags.Has(GraphicsResourceFlags.GpuWrite))
                {
                    // D3D12_HEAP_TYPE_DEFAULT - GPU read/write, CPU must be inaccessible.
                    if (!flags.Has(GraphicsResourceFlags.CpuRead) && !flags.Has(GraphicsResourceFlags.CpuWrite))
                        return HeapType.Default;
                }
                else
                {
                    // D3D12_HEAP_TYPE_UPLOAD - GPU read, CPU write.
                    if (flags.Has(GraphicsResourceFlags.CpuWrite))
                        return HeapType.Upload;
                }
            }
            else if (flags.Has(GraphicsResourceFlags.GpuWrite)) // GPU write
            {
                // D3D12_HEAP_TYPE_READBACK - GPU write, CPU read.
                if (flags.Has(GraphicsResourceFlags.CpuRead))
                    return HeapType.Readback;
            }

            // None of the expected read/write permissions matched the built-in heap types, so we'll use a custom heap type.
            return HeapType.Custom;
        }
        else
        {
            return 0;
        }
    }

    internal static ResourceStates ToResourceState(this GraphicsResourceFlags flags)
    {
        HeapType heapType = flags.ToHeapType();

        if(heapType != 0)
        {
            return heapType switch
            {
                HeapType.Default => ResourceStates.Common,
                HeapType.Upload => ResourceStates.GenericRead,
                HeapType.Readback => ResourceStates.CopyDest,
                _ => ResourceStates.Common,
            };
        }

        return ResourceStates.Common;
    }
}
