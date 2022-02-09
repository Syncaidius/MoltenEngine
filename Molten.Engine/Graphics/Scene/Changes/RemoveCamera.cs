namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for removing a <see cref="ICamera"/> from a scene.</summary>
    internal class RemoveCamera : RenderSceneChange<RemoveCamera>
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
            Data.Cameras.Remove(Camera);
            Recycle(this);
        }
    }
}
