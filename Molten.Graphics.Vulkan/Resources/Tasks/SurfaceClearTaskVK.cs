using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal struct SurfaceClearTaskVK : IGraphicsResourceTask
    {
        public Color Color;

        public unsafe bool Process(GraphicsQueue queue, GraphicsResource resource)
        {
            // TODO Implement proper handling of barrier transitions.
            //  -- Transition from the current layout to the one we need.
            //  -- Transition back to the original layout once we're done.

            RenderSurface2DVK surface = resource as RenderSurface2DVK;
            GraphicsQueueVK vkCmd = queue as GraphicsQueueVK;
            surface.Apply(queue);

            vkCmd.Sync(GraphicsCommandListFlags.SingleSubmit);
            surface.Transition(vkCmd, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

            ImageSubresourceRange range = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = surface.ArraySize,
                BaseMipLevel = 0,
                LevelCount = surface.MipMapCount,
            };

            vkCmd.ClearImage(*surface.ImageHandle, ImageLayout.TransferDstOptimal, Color, &range, 1);
            surface.Transition(vkCmd, ImageLayout.TransferDstOptimal, ImageLayout.ColorAttachmentOptimal);
            vkCmd.Sync();

            // Clear Surface via 
            // TODO See: https://stackoverflow.com/questions/69915270/what-is-the-best-way-to-clear-a-vkimage-to-a-single-color
            // NOTE:    Don't do vkCmdClearColorImage() if you can avoid it; on many architectures (in particular mobile)
            //          it's much more efficient to use a clear loadOp on the render pass.

            // See: https://developer.nvidia.com/blog/advanced-api-performance-vulkan-clearing-and-presenting/
            // NOTE:    Use VK_ATTACHMENT_LOAD_OP_CLEAR to clear attachments at the beginning of a subpass instead of clear commands.
            //          This can allow the driver to skip loading unnecessary data.
            //
            //          Outside of a render pass instance, prefer the usage of vkCmdClearColorImage instead of a CS invocation to clear images.
            //          This path enables bandwidth optimizations.

            //          If possible, batch clears to avoid interleaving single clears between dispatches.

            //          Coordinate VkClearDepthStencilValue with the test function to achieve better depth testing performance:
            //          0.5 ≤ depth value< 1.0 correlates with VK_COMPARE_OP_LESS_OR_EQUAL
            //          0.0 ≤ depth value< 0.5 correlates with VK_COMPARE_OP_GREATER_OR_EQUAL
            return true;
        }
    }
}
