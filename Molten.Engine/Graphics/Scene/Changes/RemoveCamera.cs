namespace Molten.Graphics;

/// <summary>A <see cref="GraphicsTask"/> for removing a <see cref="RenderCamera"/> from a scene.</summary>
internal class RemoveCamera : GraphicsTask
{
    public RenderCamera Camera;
    public SceneRenderData Data;

    public override void ClearForPool()
    {
        Camera = null;
        Data = null;
    }

    public override bool Validate() => true;

    protected override bool OnProcess(RenderService renderer, GpuCommandQueue queue)
    {
        Data.Cameras.Remove(Camera);
        return true;
    }
}
