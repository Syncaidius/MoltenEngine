namespace Molten.Graphics;

public delegate void ComputeTaskCompletionCallback();

internal class ComputeTask : GraphicsTask
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

    public override void Process(RenderService renderer, GraphicsQueue queue)
    {
        //queue.Begin();
        queue.Dispatch(Shader, Groups);
        //queue.End();
        CompletionCallback?.Invoke();
    }
}
