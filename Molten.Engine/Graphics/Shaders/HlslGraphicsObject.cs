namespace Molten.Graphics
{
    /// <summary>
    /// An a base class implementation for key shader components, such as materials, material passes or compute tasks.
    /// </summary>
    public abstract class HlslGraphicsObject : GraphicsObject
    {
        protected HlslGraphicsObject(GraphicsDevice device, GraphicsBindTypeFlags bindFlags) :
            base(device, bindFlags)
        {

        }

        protected override void OnApply(GraphicsCommandQueue cmd) { }
    }
}
