namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="ICamera"/> to a scene.</summary>
    internal class AddCamera : RenderSceneChange<AddCamera>
    {
        public RenderCamera Camera;
        public SceneRenderData Data;

        public override void ClearForPool()
        {
            Camera = null;
            Data = null;
        }

        public override void Process()
        {
            Data.Cameras.Add(Camera);
            Recycle(this);
        }
    }
}
