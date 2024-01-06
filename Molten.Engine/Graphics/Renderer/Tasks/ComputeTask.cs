namespace Molten.Graphics;

public delegate void ComputeTaskCompletionCallback();

internal class ComputeTask : RenderTask<ComputeTask>
{
    public ComputeTaskCompletionCallback CompletionCallback;

    internal HlslShader Shader;

    internal Vector3UI Groups;

    public override void ClearForPool()
    {
        Shader = null;
        Groups = Vector3UI.Zero;
        CompletionCallback = null;
    }

    public override void Process(RenderService renderer)
    {
        //renderer.Device.Queue.Begin();
        renderer.Device.Queue.Dispatch(Shader, Groups);
        //renderer.Device.Queue.End();
        CompletionCallback?.Invoke();
        Recycle(this);
    }
}
