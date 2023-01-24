namespace Molten.Graphics
{
    internal abstract class ContextStateProvider : EngineObject
    {
        internal ContextStateProvider(CommandQueueDX11 queue) { }

        /// <summary>
        /// Called when the current <see cref="ContextStateProvider"/> is to be bound to it's parent <see cref="CommandQueueDX11"/>
        /// </summary>
        internal abstract void Bind(CommandQueueDX11 queue);
    }
}
