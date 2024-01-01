namespace Molten.Graphics.Vulkan
{
    internal interface IRenderSurfaceVK : IRenderSurface
    {
        /// <summary>
        /// Gets surface clear color, if any.
        /// </summary>
        Color? ClearColor { get; set; }
    }
}
