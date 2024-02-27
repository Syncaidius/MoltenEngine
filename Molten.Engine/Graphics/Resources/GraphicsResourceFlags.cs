using System.Runtime.CompilerServices;

namespace Molten.Graphics;

[Flags]
public enum GraphicsResourceFlags
{
    /// <summary>
    /// No flags. This is usually invalid.
    /// </summary>
    None = 0,

    /// <summary>
    /// Allow the CPU to read/copy from the resource.
    /// </summary>
    CpuRead = 1,

    /// <summary>
    /// Allow the CPU to write to the resource.
    /// </summary>
    CpuWrite = 1 << 1,

    /// <summary>
    /// Allow the GPU to read/copy from the resource.
    /// </summary>
    GpuRead = 1 << 2,

    /// <summary>
    /// Allow the GPU to write to the resource.
    /// </summary>
    GpuWrite = 1 << 3,

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

    /// <summary>
    /// All of the GPU and CPU read/write flags. Generally used by staging resources.
    /// </summary>
    AllReadWrite = (CpuRead | CpuWrite | GpuRead | GpuWrite),
}

public static class ResourceFlagsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has(this GraphicsResourceFlags flags, GraphicsResourceFlags value)
    {
        return (flags & value) == value;
    }

    public static bool IsImmutable(this GraphicsResourceFlags flags)
    {
        return (!flags.Has(GraphicsResourceFlags.GpuWrite) && 
            !flags.Has(GraphicsResourceFlags.CpuRead) && 
            !flags.Has(GraphicsResourceFlags.CpuWrite));
    }

    public static bool IsDiscard(this GraphicsResourceFlags flags)
    {
        return flags.Has(GraphicsResourceFlags.CpuWrite) && 
            !flags.Has(GraphicsResourceFlags.CpuRead) && !flags.Has(GraphicsResourceFlags.GpuWrite);
    }
}
