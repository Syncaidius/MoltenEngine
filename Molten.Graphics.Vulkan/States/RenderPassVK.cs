using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class RenderPassVK : GpuObject<DeviceVK>, IEquatable<RenderPassVK>
{
    // TODO abstract the render pass system into Molten.Engine.RenderStep.
    //  - Each RenderStep will provide a RenderPass.
    //  - This makes it easier to check PipelineStateVK equality for determining if a new pipeline needs creating during render.

    RenderPass _handle;
    RenderPassCreateInfo _info;

    public RenderPassVK(DeviceVK device, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface) :
        base(device)
    {
        _info = new RenderPassCreateInfo()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = (uint)(surfaces.Length + (depthSurface != null ? 1 : 0)),
            PAttachments = EngineUtil.AllocArray<AttachmentDescription>(_info.AttachmentCount),
            SubpassCount = 1,
            PSubpasses = EngineUtil.AllocArray<SubpassDescription>(1),
            DependencyCount = 1,
            PDependencies = EngineUtil.AllocArray<SubpassDependency>(1),
            Flags = RenderPassCreateFlags.None,
            PNext = null,
        };

        _info.PSubpasses[0] = new SubpassDescription()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = (uint)surfaces.Length,
            PColorAttachments = EngineUtil.AllocArray<AttachmentReference>((uint)surfaces.Length),
            PDepthStencilAttachment = depthSurface != null ? EngineUtil.Alloc<AttachmentReference>() : null,
            Flags = SubpassDescriptionFlags.None,
            InputAttachmentCount = 0,
            PInputAttachments = null,
            PreserveAttachmentCount = 0,
            PPreserveAttachments = null,
            PResolveAttachments = null
        };

        _info.PDependencies[0] = new SubpassDependency()
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            SrcAccessMask = AccessFlags.None,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit,
            DependencyFlags = DependencyFlags.None,
        };

        // Surface/color attachment info.
        uint i = 0; // Attachment index
        for (; i < surfaces.Length; i++)
        {
            GetAttachmentDesc(surfaces[i], out _info.PAttachments[i]);
            _info.PSubpasses[0].PColorAttachments[i] = GetAttachmentRef(ref _info.PAttachments[i], i);
        }

        // Depth-stencil attachment info
        if (depthSurface != null)
        {
            GetAttachmentDesc(depthSurface, out _info.PAttachments[i]);
            _info.PSubpasses[0].PDepthStencilAttachment[0] = GetAttachmentRef(ref _info.PAttachments[i], i);
        }

        // Attempt to create the Vulkan render pass.
        Result r = Result.Success;
        fixed (RenderPassCreateInfo* ptrInfo = &_info)
        fixed (RenderPass* ptrPass = &_handle)
        {
            r = device.VK.CreateRenderPass(device, ptrInfo, null, ptrPass);
        }

        r.Throw(device, () => "failed to create render pass");
    }

    protected unsafe override void OnGraphicsRelease()
    {
        if (_handle.Handle != 0)
        {
            Device.VK.DestroyRenderPass(Device, _handle, null);
            _handle = new RenderPass();

            // Free subpass info allocations.
            for (uint i = 0; i < _info.SubpassCount; i++)
            {
                EngineUtil.Free(ref _info.PSubpasses[i].PColorAttachments);
                EngineUtil.Free(ref _info.PSubpasses[i].PDepthStencilAttachment);
            }

            // Free render pass info allocations.
            EngineUtil.Free(ref _info.PAttachments);
            EngineUtil.Free(ref _info.PDependencies);
            EngineUtil.Free(ref _info.PAttachments);
        }
    }

    public override bool Equals(object obj)
    {
        if(obj is RenderPassVK pass)
            return Equals(pass);

        return false;
    }

    public bool Equals(RenderPassVK other)
    {
        if (_info.AttachmentCount == other._info.AttachmentCount)
        {
            int i = 0;
            for (; i < _info.AttachmentCount; i++)
            {
                if (!AttachmentsEqual(ref _info.PAttachments[i], ref other._info.PAttachments[i]))
                    return false;
            }
        }

        return true;
    }

    internal bool DoSurfacesMatch(DeviceVK device, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface)
    {
        AttachmentDescription descCompare = new AttachmentDescription();

        int surfaceCount = surfaces.Length + (depthSurface != null ? 1 : 0);
        if (surfaceCount == _info.AttachmentCount)
        {
            int i = 0;
            for (; i < surfaces.Length; i++)
            {
                GetAttachmentDesc(surfaces[i], out descCompare);
                if (!AttachmentsEqual(ref _info.PAttachments[i], ref descCompare))
                    return false;
            }

            // Compare the last attachment to the depth surface, if present.
            if (depthSurface != null)
            {
                GetAttachmentDesc(depthSurface, out descCompare);
                if (!AttachmentsEqual(ref _info.PAttachments[i], ref descCompare))
                    return false;
            }
        }

        return true;
    }


    private bool AttachmentsEqual(ref AttachmentDescription a, ref AttachmentDescription b)
    {
        return a.FinalLayout == b.FinalLayout &&
            a.Format == b.Format &&
            a.InitialLayout == b.InitialLayout &&
            a.LoadOp == b.LoadOp &&
            a.Samples == b.Samples &&
            a.StencilLoadOp == b.StencilLoadOp &&
            a.StencilStoreOp == b.StencilStoreOp &&
            a.StoreOp == b.StoreOp;
    }

    private void GetAttachmentDesc(IRenderSurfaceVK surface, out AttachmentDescription desc)
    {
        desc = new AttachmentDescription()
        {
            Format = surface.ResourceFormat.ToApi(),
            Samples = GetSampleFlags(surface.MultiSampleLevel),
            LoadOp = surface.ClearColor.HasValue ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
            StoreOp = AttachmentStoreOp.Store,
            InitialLayout = ImageLayout.Undefined, // TODO Track current layout of texture/surface and use it here
            FinalLayout = ImageLayout.Undefined,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
        };

        if (surface is ISwapChainSurface)
        {
            desc.FinalLayout = ImageLayout.PresentSrcKhr;
        }
        else
        {
            if (!surface.Flags.Has(GpuResourceFlags.DenyShaderAccess))
                desc.FinalLayout = ImageLayout.ColorAttachmentOptimal;
            else
                desc.FinalLayout = ImageLayout.ShaderReadOnlyOptimal;
        }
    }

    private void GetAttachmentDesc(DepthSurfaceVK surface, out AttachmentDescription desc)
    {
        desc = new AttachmentDescription()
        {
            Format = surface.ResourceFormat.ToApi(),
            Samples = GetSampleFlags(surface.MultiSampleLevel),
            LoadOp = surface.ClearValue.HasValue ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare, // TODO Set these based on depthSurface.DepthFormat - stencil format
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            // TODO Make use of DepthStencilReadOnlyOptimal when using read-only depth mode.
            // TODO if we don't need a stencil, try attaching with DepthAttachmentOptimal instead.
        };
    }

    private AttachmentReference GetAttachmentRef(ref AttachmentDescription desc, uint index)
    {
        /* For depth surface,s if the separateDepthStencilLayouts feature is not enabled, and attachment is not VK_ATTACHMENT_UNUSED, layout must not be:
         *  - VK_IMAGE_LAYOUT_DEPTH_ATTACHMENT_OPTIMAL, VK_IMAGE_LAYOUT_DEPTH_READ_ONLY_OPTIMAL, VK_IMAGE_LAYOUT_STENCIL_ATTACHMENT_OPTIMAL or 
         *  VK_IMAGE_LAYOUT_STENCIL_READ_ONLY_OPTIMAL*/

        ImageLayout layout = ImageLayout.ColorAttachmentOptimal;
        if (desc.FinalLayout == ImageLayout.DepthStencilAttachmentOptimal
            || desc.FinalLayout == ImageLayout.DepthStencilReadOnlyOptimal
            || desc.FinalLayout == ImageLayout.DepthAttachmentOptimal
            || desc.FinalLayout == ImageLayout.DepthReadOnlyOptimal)
        {
            layout = ImageLayout.DepthStencilAttachmentOptimal;
        }

        return new AttachmentReference(index, layout);
    }

    private SampleCountFlags GetSampleFlags(AntiAliasLevel aaLevel)
    {
        return aaLevel switch
        {
            AntiAliasLevel.X2 => SampleCountFlags.Count2Bit,
            AntiAliasLevel.X4 => SampleCountFlags.Count4Bit,
            AntiAliasLevel.X8 => SampleCountFlags.Count8Bit,
            AntiAliasLevel.X16 => SampleCountFlags.Count16Bit,
            _ => SampleCountFlags.Count1Bit,
        };
    }

    public static implicit operator RenderPass(RenderPassVK pass)
    {
        return pass._handle;
    }

    internal RenderPass Handle => _handle;

    internal ref RenderPassCreateInfo Info => ref _info;
}
