using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal static class DescriptionInterop
{
    internal static HeapType ToHeapType(this GpuResourceFlags flags)
    {
        if (flags.Has(GpuResourceFlags.CpuRead)
            || flags.Has(GpuResourceFlags.CpuWrite)
            || flags.Has(GpuResourceFlags.GpuRead)
            || flags.Has(GpuResourceFlags.GpuWrite))
        {
            // GPU read.
            if (flags.Has(GpuResourceFlags.GpuRead))
            {
                // GPU read/write.
                if (flags.Has(GpuResourceFlags.GpuWrite))
                {
                    // D3D12_HEAP_TYPE_DEFAULT - GPU read/write, CPU must be inaccessible.
                    if (!flags.Has(GpuResourceFlags.CpuRead) && !flags.Has(GpuResourceFlags.CpuWrite))
                        return HeapType.Default;
                }
                else
                {
                    // D3D12_HEAP_TYPE_UPLOAD - GPU read, CPU write.
                    if (flags.Has(GpuResourceFlags.CpuWrite))
                        return HeapType.Upload;
                }
            }
            else if (flags.Has(GpuResourceFlags.GpuWrite)) // GPU write
            {
                // D3D12_HEAP_TYPE_READBACK - GPU write, CPU read.
                if (flags.Has(GpuResourceFlags.CpuRead))
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

    internal static ResourceStates ToResourceState(this GpuResourceFlags flags)
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
