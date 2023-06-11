using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal struct DepthClearTaskVK : IGraphicsResourceTask
    {
        public float DepthValue;

        public uint StencilValue;

        public unsafe bool Process(GraphicsQueue queue, GraphicsResource resource)
        {
            // TODO Implement proper handling of barrier transitions.
            //  -- Transition from the current layout to the one we need.
            //  -- Transition back to the original layout once we're done.

            DepthSurfaceVK surface = resource as DepthSurfaceVK;
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

            vkCmd.ClearDepthImage(*surface.ImageHandle, ImageLayout.TransferDstOptimal, DepthValue, StencilValue, &range, 1);
            surface.Transition(vkCmd, ImageLayout.TransferDstOptimal, ImageLayout.DepthAttachmentOptimal);
            vkCmd.Sync();

            return true;
        }
    }
}
