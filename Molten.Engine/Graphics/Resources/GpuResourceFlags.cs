using System.Runtime.CompilerServices;

namespace Molten.Graphics;

[Flags]
public enum GpuResourceFlags
{
    /// <summary>
    /// No flags. This is usually invalid.
    /// </summary>
    None = 0,

    /// <summary>
    /// Default memory access. This provides GPU read and write access, but no CPU access.
    /// </summary>
    DefaultMemory = 1,

    /// <summary>
    /// Upload memory access. This provides CPU write access and GPU read access. 
    /// This is generally useful for staging or dynamic resources, where frequent transfers from CPU to GPU are required.
    /// </summary>
    UploadMemory = 1 << 1,

    /// <summary>
    /// Download (read-back) memory access. This provides CPU read access and GPU write access.
    /// </summary>
    DownloadMemory = 1 << 2,

    /// <summary>
    /// Allow unordered/storage access from supported shader stages.
    /// <para>These are UAV resources in DX11 and DX12.</para>
    /// <para>These are Shader Storage Objects (SSO) resources in OpenGL and Vulkan.</para>
    /// </summary>
    UnorderedAccess = 1 << 4,

    /// <summary>
    /// Do not allow shader access. For example in DX11 this would prevent a shader resource view (SRV) from being bound to the resource.
    /// </summary>
    DenyShaderAccess = 1 << 5,

    /// <summary>
    /// Allows the resource to be shared between logical devices, queues or processes. 
    /// For example:
    /// <list type="bullet">
    /// <item>Multiple ID3D11Device, ID3D12Device or vkDevice instances within the same application.</item>
    /// <item>Two separate applcations which share data via GPU resources, such as an external game debugging editor or tool</item>
    /// <item>Multiple command queues which need to write to the same resource at different locations.</item>
    /// </list>
    /// </summary>
    SharedAccess = 1 << 6,

    /// <summary>
    /// Allows the resource to be shared between physical devices e.g. Two descreet GPUs or one integrated and one descreet GPU.
    /// </summary>
    CrossAdapter = 1 << 7,

    /// <summary>
    /// Allow mi-pmap generation for the resource.
    /// </summary>
    MipMapGeneration = 1 << 8,
}

public static class ResourceFlagsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has(this GpuResourceFlags flags, GpuResourceFlags value)
    {
        return (flags & value) == value;
    }

    public static bool IsGpuReadable(this GpuResourceFlags flags)
    {
        return flags.Has(GpuResourceFlags.DefaultMemory) || flags.Has(GpuResourceFlags.UploadMemory);
    }

    public static bool IsGpuWritable(this GpuResourceFlags flags)
    {
        return flags.Has(GpuResourceFlags.DefaultMemory) || flags.Has(GpuResourceFlags.DownloadMemory);
    }

    public static bool IsCpuReadable(this GpuResourceFlags flags)
    {
        return flags.Has(GpuResourceFlags.DownloadMemory);
    }

    public static bool IsCpuWritable(this GpuResourceFlags flags)
    {
        return flags.Has(GpuResourceFlags.UploadMemory);
    }
}
