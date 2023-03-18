namespace Molten.Graphics
{
    internal interface ITextureTask
    {
        /// <summary>
        /// Processes the texture change request. Returns true if the parent texture was altered.
        /// </summary>
        /// <param name="cmd">The command queue that is used for processing the task.</param>
        /// <param name="texture"></param>
        /// <returns></returns>
        bool Process(CommandQueueDX11 cmd, TextureDX11 texture);
    }
}
