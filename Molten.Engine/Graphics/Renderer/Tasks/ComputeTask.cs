namespace Molten.Graphics
{
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
            renderer.Device.Cmd.BeginDraw();
            renderer.Device.Cmd.Dispatch(Shader, Groups);
            renderer.Device.Cmd.EndDraw();
            CompletionCallback?.Invoke();
            Recycle(this);
        }
    }
}
