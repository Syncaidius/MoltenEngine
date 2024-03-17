namespace Molten.Graphics;

/// <summary>A <see cref="GpuTask"/> for adding a <see cref="RenderCamera"/> to a scene.</summary>
internal class AddCamera : GpuTask
{
    public RenderCamera Camera;
    public SceneRenderData Data;

    public override void ClearForPool()
    {
        Camera = null;
        Data = null;
    }

    public override bool Validate() => true;

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        Data.Cameras.Add(Camera);
        return true;
    }
}
