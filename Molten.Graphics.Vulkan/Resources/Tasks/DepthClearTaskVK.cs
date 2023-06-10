using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal struct DepthClearTaskVK : IGraphicsResourceTask
    {
        public DepthSurfaceVK Surface;

        public float DepthValue;

        public uint StencilValue;

        public unsafe bool Process(GraphicsQueue queue, GraphicsResource resource)
        {
            // TODO Implement proper handling of barrier transitions.
            //  -- Transition from the current layout to the one we need.
            //  -- Transition back to the original layout once we're done.

            GraphicsQueueVK vkCmd = queue as GraphicsQueueVK;
            Surface.Apply(queue);

            vkCmd.Sync(GraphicsCommandListFlags.SingleSubmit);
            Surface.Transition(vkCmd, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

            ImageSubresourceRange range = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = Surface.ArraySize,
                BaseMipLevel = 0,
                LevelCount = Surface.MipMapCount,
            };

            vkCmd.ClearDepthImage(*Surface.ImageHandle, ImageLayout.TransferDstOptimal, DepthValue, StencilValue, &range, 1);
            Surface.Transition(vkCmd, ImageLayout.TransferDstOptimal, ImageLayout.ColorAttachmentOptimal);
            vkCmd.Sync();

            return true;
        }
    }
}
