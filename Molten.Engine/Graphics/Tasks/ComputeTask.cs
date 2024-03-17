namespace Molten.Graphics;

internal class ComputeTask : GpuTask
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

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        //queue.Begin();
        cmd.Dispatch(Shader, Groups);
        //queue.End();
        return true;
    }
}
