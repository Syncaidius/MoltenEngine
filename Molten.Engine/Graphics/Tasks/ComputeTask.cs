namespace Molten.Graphics;

internal class ComputeTask : GraphicsTask
{
    internal Shader Shader;

    internal Vector3UI Groups;

    public override void ClearForPool()
    {
        Shader = null;
        Groups = Vector3UI.Zero;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        //queue.Begin();
        queue.Dispatch(Shader, Groups);
        //queue.End();
        return true;
    }
}
