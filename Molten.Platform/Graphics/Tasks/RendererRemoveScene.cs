namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class RendererRemoveScene : RendererTask<RendererRemoveScene>
    {
        public SceneRenderData Data;

        public override void ClearForPool()
        {
            Data = null;
        }

        public override void Process(RenderService renderer)
        {
            renderer.Scenes.Remove(Data);
            Recycle(this);
        }
    }
}
