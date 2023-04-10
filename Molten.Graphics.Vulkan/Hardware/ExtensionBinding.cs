namespace Molten.Graphics.Vulkan
{
    internal class ExtensionBinding
    {
        internal Dictionary<string, VulkanExtension> Extensions = new Dictionary<string, VulkanExtension>();

        internal List<string> Layers = new List<string>();
    }
}
