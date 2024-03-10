using Molten.IO;

namespace Molten.Graphics;

/// <summary>
/// A graphics-specific implementation of <see cref="RawStream"/> intended for use with mapped <see cref="GpuResource"/> instances.
/// </summary>
public class GpuStream : RawStream
{
    public unsafe GpuStream(GpuCommandList cmd, GpuResource resource, ref GpuResourceMap map) : 
        base(map.Ptr, 
            map.DepthPitch, 
            resource.Flags.Has(GpuResourceFlags.CpuRead), 
            resource.Flags.Has(GpuResourceFlags.CpuWrite))
    {
        Map = map;
        Cmd = cmd;
        Resource = resource;
    }

    protected override void Dispose(bool disposing)
    {
        Cmd.UnmapResource(this);
        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the <see cref="GpuCommandList"/> that the current <see cref="GpuStream"/> is bound to.
    /// </summary>
    public GpuCommandList Cmd { get; }

    /// <summary>
    /// Gets the <see cref="GpuResource"/> that the current <see cref="GpuStream"/> points to.
    /// </summary>
    public GpuResource Resource { get; }

    /// <summary>
    /// Gets the index of the sub-resource of <see cref="Resource"/> that the current <see cref="GpuStream"/> points to.
    /// </summary>
    public uint SubResourceIndex { get; }

    /// <summary>
    /// Gets the number of bytes in a single row of the mapped resource. E.g. a row of pixels in an uncompressed texture, or a row of blocks in a compressed texture.
    /// </summary>
    public GpuResourceMap Map { get; }
}
