namespace Molten.Graphics
{
    internal abstract class ContextStateProvider : EngineObject
    {
        internal ContextStateProvider(DeviceContextState parent) { }

        /// <summary>
        /// Called when the current <see cref="ContextStateProvider"/> is to be bound to it's parent <see cref="CommandQueueDX11"/>
        /// </summary>
        internal abstract void Bind(DeviceContextState state, CommandQueueDX11 context);
    }
}
