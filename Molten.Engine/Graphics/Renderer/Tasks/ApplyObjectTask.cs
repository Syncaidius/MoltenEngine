namespace Molten.Graphics
{
    /// <summary>A render task which calls <see cref="GraphicsObject.Apply(GraphicsQueue)"/>.</summary>
    public class ApplyObjectTask : RenderTask<ApplyObjectTask>
    {
        public GraphicsObject Object;

        public override void ClearForPool()
        {
            Object = null;
        }

        public override void Process(RenderService renderer)
        {
            Object.Apply(renderer.Device.Queue);
            Recycle(this);
        }
    }
}
