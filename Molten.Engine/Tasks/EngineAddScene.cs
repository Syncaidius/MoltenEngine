namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class EngineAddScene : EngineTask<EngineAddScene>
    {
        public Scene Scene;

        public override void ClearForPool()
        {
            Scene = null;
        }

        public override void Process(Engine engine, Timing time)
        {
            engine.Scenes.Add(Scene);
            Recycle(this);
        }
    }
}
