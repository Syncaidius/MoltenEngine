using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal static class DescriptionInterop
{
    internal static HeapType ToHeapType(this GpuResourceFlags flags)
    {
        if (flags.Has(GpuResourceFlags.DefaultMemory))
        {
            if (flags.Has(GpuResourceFlags.DownloadMemory) || flags.Has(GpuResourceFlags.UploadMemory))
                throw new Exception("Cannot have both default memory and upload/download memory flags.");

            return HeapType.Default;
        }
        else if (flags.Has(GpuResourceFlags.UploadMemory))
        {
            if(flags.Has(GpuResourceFlags.DownloadMemory))
                throw new Exception("Cannot have both upload and download memory flags.");

            return HeapType.Upload;
        }
        else if (flags.Has(GpuResourceFlags.DownloadMemory))
        {
            return HeapType.Readback;
        }
        else
        {
            throw new Exception("Heap type cannot be determined due to lack of memory flags.");
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
