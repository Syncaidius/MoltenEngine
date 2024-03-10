using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal static class VkInterop
{
    internal static GpuResourceFormat FromApi(this Format format)
    {
        switch(format)
        {
            default:
            case Format.Undefined:
                return GpuResourceFormat.Unknown;

            // RGBA 8-bit
            case Format.R8G8B8A8SNorm: return GpuResourceFormat.R8G8B8A8_SNorm;
            case Format.R8G8B8A8Unorm: return GpuResourceFormat.R8G8B8A8_UNorm;
            case Format.R8G8B8A8Sint: return GpuResourceFormat.R8G8B8A8_SInt;
            case Format.R8G8B8A8Srgb: return GpuResourceFormat.R8G8B8A8_UNorm_SRgb;

            // BGRA 8-bit
            case Format.B8G8R8A8Sint: return GpuResourceFormat.B8G8R8A8_Typeless;
            case Format.B8G8R8A8SNorm: return GpuResourceFormat.B8G8R8A8_Typeless;
            case Format.B8G8R8A8Unorm: return GpuResourceFormat.B8G8R8A8_UNorm;

            case Format.R32Sfloat: return GpuResourceFormat.R32_Float;
            case Format.R32Sint: return GpuResourceFormat.R32_SInt;
            case Format.R32Uint: return GpuResourceFormat.R32_UInt;
        }
    }

    internal static Format ToApi(this GpuResourceFormat format)
    {
        switch (format)
        {
            default:
            case GpuResourceFormat.Unknown:
                return Format.Undefined;

            // RGBA 8-bit
            case GpuResourceFormat.R8G8B8A8_SNorm: return Format.R8G8B8A8SNorm;
            case GpuResourceFormat.R8G8B8A8_UNorm: return Format.R8G8B8A8Unorm;
            case GpuResourceFormat.R8G8B8A8_SInt: return Format.R8G8B8A8Sint;
            case GpuResourceFormat.R8G8B8A8_UNorm_SRgb: return Format.R8G8B8A8Srgb;

            // BGRA 8-bit
            case GpuResourceFormat.B8G8R8A8_Typeless: return Format.B8G8R8A8SNorm;
            case GpuResourceFormat.B8G8R8A8_UNorm: return Format.B8G8R8A8Unorm;

            case GpuResourceFormat.R32_Float: return Format.R32Sfloat;
            case GpuResourceFormat.R32_SInt: return Format.R32Sint;
            case GpuResourceFormat.R32_UInt: return Format.R32Uint;
        }
    }

    internal static bool Has(this MemoryPropertyFlags flags, MemoryPropertyFlags value)
    {
        return (flags & value) == value;
    }
}
