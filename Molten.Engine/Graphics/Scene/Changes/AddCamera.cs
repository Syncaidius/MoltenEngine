namespace Molten.Graphics;

/// <summary>A <see cref="GraphicsTask"/> for adding a <see cref="RenderCamera"/> to a scene.</summary>
internal class AddCamera : GraphicsTask
{
    public RenderCamera Camera;
    public SceneRenderData Data;

    public override void ClearForPool()
    {
        Camera = null;
        Data = null;
    }

    public override bool Validate() => true;

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        Data.Cameras.Add(Camera);
        return true;
    }
}
