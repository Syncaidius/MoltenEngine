namespace Molten.Graphics;

public class ResourceCopyTask : GraphicsResourceTask<GpuResource>
{
    public GpuResource Destination;

    public override void ClearForPool()
    {
        Destination = null;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        if (Resource is GpuBuffer buffer && buffer.BufferType == GpuBufferType.Staging)
            Resource.Apply(cmd);

        cmd.CopyResource(Resource, Destination);

        return true;
    }
}
