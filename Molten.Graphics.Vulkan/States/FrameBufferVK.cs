using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class FrameBufferVK : GpuObject<DeviceVK>
{
    Framebuffer _frameBuffer;
    FramebufferCreateInfo _info;
    ImageView* _views;

    public FrameBufferVK(DeviceVK device, RenderPassVK renderPass, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface) : base(device)
    {
        _info = new FramebufferCreateInfo()
        {
            SType = StructureType.FramebufferCreateInfo,
            RenderPass = renderPass,
            AttachmentCount = renderPass.Info.AttachmentCount,
            PAttachments = _views,
            Width = surfaces[0].Width,
            Height = surfaces[0].Height,
            Layers = 1, // Array count?
        };

        _views = EngineUtil.AllocArray<ImageView>(renderPass.Info.AttachmentCount);
        uint i = 0;
        for (; i < surfaces.Length; i++)
        {
            ResourceHandleVK<Image, ImageHandleVK> handle = surfaces[i].Handle as ResourceHandleVK<Image, ImageHandleVK>;
            _views[i] = *handle.SubHandle.ViewPtr;
        }

        if(depthSurface != null)
            _views[i] = *depthSurface.Handle.SubHandle.ViewPtr;

        Result r = device.VK.CreateFramebuffer(device, _info, null, out _frameBuffer);
        r.Throw(device, () => "Failed to create frame buffer");
    }

    internal bool DoSurfacesMatch(DeviceVK device, RenderPassVK pass, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface)
    {
        uint surfaceCount = (uint)(depthSurface == null ? surfaces.Length : surfaces.Length + 1);

        if (_info.RenderPass.Handle != pass.Handle.Handle)
            return false;

        if (_info.AttachmentCount != surfaceCount
            || _info.Width != surfaces[0].Width
            || _info.Height != surfaces[0].Height)
        {
            return false;
        }

        // Check if render surface handles match
        int i = 0;
        for (; i < surfaces.Length; i++)
        {
            ResourceHandleVK<Image, ImageHandleVK> handle = surfaces[i].Handle as ResourceHandleVK<Image, ImageHandleVK>;
            if (_views[i].Handle == handle.SubHandle.ViewPtr->Handle)
                return false;
        }

        // Check if depth surface handle matches
        if (depthSurface != null)
        {
            if(depthSurface.Handle.SubHandle.ViewPtr->Handle != _views[i].Handle)
                return false;
        }

        return true;
    }

    protected override void OnGraphicsRelease()
    {
        if(_frameBuffer.Handle != 0)
            Device.VK.DestroyFramebuffer(Device, _frameBuffer, null);

        EngineUtil.Free(ref _views);
    }

    public static implicit operator Framebuffer(FrameBufferVK fb)
    {
          return fb._frameBuffer;
    }

    internal ref FramebufferCreateInfo Info => ref _info;
}
