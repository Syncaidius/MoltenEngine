namespace Molten.Graphics;

/// <summary>A <see cref="RenderRemoveScene"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
internal class RenderRemoveScene : GraphicsTask
{
    public SceneRenderData Data;

    public override void ClearForPool()
    {
        Data = null;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        cmd.Device.Renderer.Scenes.Remove(Data);
        return true;
    }
}
