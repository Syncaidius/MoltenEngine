namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class RendererAddScene : RendererTask<RendererAddScene>
    {
        public SceneRenderData Data;

        public override void Clear()
        {
            Data = null;
        }

        public override void Process(RenderService renderer)
        {
            renderer.Scenes.Add(Data);
            Recycle(this);
        }
    }
}
