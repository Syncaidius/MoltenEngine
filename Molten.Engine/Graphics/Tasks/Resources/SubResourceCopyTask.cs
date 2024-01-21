namespace Molten.Graphics;

public class SubResourceCopyTask : GraphicsResourceTask<GraphicsResource>
{
    public ResourceRegion? SrcRegion;

    public uint SrcSubResource;

    /// <summary>The start offset within the resource.
    /// <para>For a buffer, only the X dimension needs to be set equal to the number of bytes to offset.</para>
    /// <para>For textures, this will vary depending on the number of texture dimensions.</para></summary>
    public Vector3UI DestStart;

    public GraphicsResource DestResource;

    public uint DestSubResource;

    public override void ClearForPool()
    {
        SrcRegion = null;
        SrcSubResource = 0;
        DestResource = null;
        DestSubResource = 0;
        DestStart = Vector3UI.Zero;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        if (DestResource.Flags.Has(GraphicsResourceFlags.GpuWrite))
            throw new ResourceCopyException(Resource, DestResource, "The destination resource must have GPU write access for writing the copied data.");

        if (Resource is GraphicsBuffer buffer && buffer.BufferType == GraphicsBufferType.Staging)
            Resource.Apply(queue);

        queue.CopyResourceRegion(Resource, SrcSubResource, SrcRegion, DestResource, DestSubResource, DestStart);
        queue.Profiler.SubResourceCopyCalls++;

        return true;
    }
}
