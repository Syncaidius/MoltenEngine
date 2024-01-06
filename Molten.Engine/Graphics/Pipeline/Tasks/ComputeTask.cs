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

    public override void Process(RenderService renderer, GraphicsDevice taskDevice)
    {
        //taskDevice.Queue.Begin();
        taskDevice.Queue.Dispatch(Shader, Groups);
        //taskDevice.Queue.End();
        CompletionCallback?.Invoke();
    }
}
