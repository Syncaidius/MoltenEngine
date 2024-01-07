using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal class DepthClearTaskVK : GraphicsResourceTask<DepthSurfaceVK>
{
    public float DepthValue;

    public uint StencilValue;

    public override void ClearForPool()
    {
        DepthValue = 0;
        StencilValue = 0;
    }

    public override bool Validate()
    {
        return true;
    }

    protected unsafe override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        // TODO Implement proper handling of barrier transitions.
        //  -- Transition from the current layout to the one we need.
        //  -- Transition back to the original layout once we're done.

        if (Resource.ApplyQueue.Count > 0)
        {
            Resource.ClearValue = null;

            GraphicsQueueVK vkCmd = queue as GraphicsQueueVK;
            Resource.Ensure(queue);

            vkCmd.Sync(GraphicsCommandListFlags.SingleSubmit);
            Resource.Transition(vkCmd, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);

            ImageSubresourceRange range = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = Resource.ArraySize,
                BaseMipLevel = 0,
                LevelCount = Resource.MipMapCount,
            };

            vkCmd.ClearDepthImage(*Resource.Handle.NativePtr, ImageLayout.TransferDstOptimal, DepthValue, StencilValue, &range, 1);
            Resource.Transition(vkCmd, ImageLayout.TransferDstOptimal, ImageLayout.DepthAttachmentOptimal);
            vkCmd.Sync();
        }
        else
        {
            Resource.ClearValue = new ClearDepthStencilValue(DepthValue, StencilValue);
        }

        return true;
    }
}
