using Silk.NET.Vulkan;
using System.Runtime.InteropServices;
using System.Text;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using VKViewport = Silk.NET.Vulkan.Viewport;

namespace Molten.Graphics.Vulkan;

internal unsafe class CommandListVK : GpuCommandList
{
    CommandPoolAllocation _allocation;
    CommandBuffer _handle;
    Vk _vk;
    DeviceVK _device;

    Stack<DebugUtilsLabelEXT> _eventLabelStack;
    IRenderSurfaceVK[] _applySurfaces;
    FrameBufferedArray<Rect2D> _applyScissors;
    FrameBufferedArray<ClearValue> _applyClearValues;
    FrameBufferedArray<VKViewport> _applyViewports;
    FrameBufferedArray<Buffer> _applyVertexBuffers;
    FrameBufferedArray<DescriptorSet> _applyDescSets;

    internal CommandListVK(CommandPoolAllocation allocation, CommandBuffer cmdBuffer) : 
        base(allocation.Pool.Queue.Device)
    {
        _allocation = allocation;
        _handle = cmdBuffer;
        _device = allocation.Pool.Queue.Device;
        _vk = allocation.Pool.Queue.VK;
        Semaphore = new SemaphoreVK(_device);
        Fence = new FenceVK(_device, FenceCreateFlags.None);

        uint maxSurfaces = (uint)State.Surfaces.Length;
        uint maxVBuffers = _device.Capabilities.VertexBuffers.MaxSlots;
        uint maxDescSets = 16U; // TODO: (uint)device.Capabilities.MaxBoundDescriptors;
        _applySurfaces = new IRenderSurfaceVK[maxSurfaces];
        _applyScissors = new FrameBufferedArray<Rect2D>(_device, maxSurfaces);
        _applyClearValues = new FrameBufferedArray<ClearValue>(_device, maxSurfaces);
        _applyViewports = new FrameBufferedArray<VKViewport>(_device, maxSurfaces);
        _applyVertexBuffers = new FrameBufferedArray<Buffer>(_device, maxVBuffers);
        _applyDescSets = new FrameBufferedArray<DescriptorSet>(_device, maxDescSets);

        _eventLabelStack = new Stack<DebugUtilsLabelEXT>();
    }

    public override void Execute(GpuCommandList cmd)
    {
        CommandListVK vkCmd = cmd as CommandListVK;
        if(vkCmd.Level == CommandBufferLevel.Primary)
            throw new GpuCommandListException(cmd, "Command lists can only execute secondary command lists. Primary command lists should be executed on a command queue.");


    }

    protected override void OnResetState()
    {
        throw new NotImplementedException();
    }

    protected override void OnGenerateMipmaps(GpuTexture texture)
    {
        throw new NotImplementedException();
    }

    public override void Free()
    {
        if (IsFree)
            return;

        IsFree = true;
        _allocation.Free(this);
    }

    public override unsafe void BeginEvent(string label)
    {
        RendererVK renderer = Device.Renderer as RendererVK;
        byte* ptrString = EngineUtil.StringToPtr(label, Encoding.UTF8);
        DebugUtilsLabelEXT lbl = new(pLabelName: ptrString);
        float* ptrColor = stackalloc float[] { 1f, 1f, 1f, 1f };

        NativeMemory.Copy(&ptrColor, lbl.Color, sizeof(float) * 4);
        renderer.DebugLayer.Module.CmdBeginDebugUtilsLabel(_handle, &lbl);
        _eventLabelStack.Push(lbl);
    }

    public unsafe override void EndEvent()
    {
        RendererVK renderer = Device.Renderer as RendererVK;
        DebugUtilsLabelEXT lbl = _eventLabelStack.Pop();

        renderer.DebugLayer.Module.CmdEndDebugUtilsLabel(_handle);

        EngineUtil.Free(ref lbl.PLabelName);
    }

    public unsafe override void SetMarker(string label)
    {
        RendererVK renderer = Device.Renderer as RendererVK;
        byte* ptrString = EngineUtil.StringToPtr(label, Encoding.UTF8);
        DebugUtilsLabelEXT lbl = new DebugUtilsLabelEXT(pLabelName: ptrString);
        float* ptrColor = stackalloc float[] { 1f, 1f, 1f, 1f };

        NativeMemory.Copy(&ptrColor, lbl.Color, sizeof(float) * 4);
        renderer.DebugLayer.Module.CmdBeginDebugUtilsLabel(_handle, &lbl);
        renderer.DebugLayer.Module.CmdEndDebugUtilsLabel(_handle);

        EngineUtil.Free(ref ptrString);
    }

    protected override unsafe GpuResourceMap GetResourcePtr(GpuResource resource, uint subresource, GpuMapType mapType)
    {
        ResourceHandleVK handle = (ResourceHandleVK)resource.Handle;
        if (mapType == GpuMapType.Discard)
            handle.Discard();

        GpuResourceMap map = new GpuResourceMap(null, (uint)resource.SizeInBytes, (uint)resource.SizeInBytes); // TODO Calculate correct RowPitch value when mapping textures
        Result r = _vk.MapMemory(_device, handle.Memory, 0, resource.SizeInBytes, 0, &map.Ptr);

        if (!r.Check(_device))
            return new GpuResourceMap();

        return map;
    }

    protected override unsafe void OnUnmapResource(GpuResource resource, uint subresource)
    {
        // TODO unmap isn't actually needed in certain circumstances. If the mapped memory will be re-populated every frame (e.g. uniform buffer)
        //      we can permenantly leave the memory mapped via vkMapMemory().
        // TODO optimize this accordingly to buffer type and usage.
        _vk.UnmapMemory(_device, (((ResourceHandleVK)resource.Handle).Memory));
    }

    protected override unsafe void UpdateResource(GpuResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch)
    {
        ResourceHandleVK handle = (ResourceHandleVK)resource.Handle;

        // Can we write directly to image memory?
        if (handle.Memory.Flags.Has(MemoryPropertyFlags.HostVisibleBit))
        {
            // TODO set the offset to match the provided region, writing row-by-row based on the rowPitch.

            using (GpuStream stream = MapResource(resource, subresource, 0, GpuMapType.Write))
                stream.WriteRange(ptrData, slicePitch);
        }
        else
        {
            // Use a staging buffer to transfer the data to the provided resource instead.
            using (GpuStream stream = MapResource(Device.Frame.StagingBuffer, 0, 0, GpuMapType.Write))
                stream.WriteRange(ptrData, slicePitch);

            CopyResource(Device.Frame.StagingBuffer, resource);
        }
    }

    protected override unsafe void CopyResource(GpuResource src, GpuResource dest)
    {
        if (src is BufferVK srcBuffer)
        {
            Buffer srcHandle = *srcBuffer.Handle.NativePtr;

            if (dest is BufferVK dstBuffer)
            {
                Buffer dstHandle = *dstBuffer.Handle.NativePtr;
                Span<BufferCopy> copy = [
                    new BufferCopy(0, 0, src.SizeInBytes)
                ];

                _vk.CmdCopyBuffer(_handle, srcHandle, dstHandle, copy);
            }
            else if (dest is TextureVK dstTex)
            {
                Image dstHandle = *dstTex.Handle.NativePtr;
                Offset3D offset = new Offset3D(0, 0, 0);
                Extent3D extent = new Extent3D(dstTex.Width, dstTex.Height, dstTex.Depth);

                // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.

                ImageSubresourceLayers layers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                Span<BufferImageCopy> regions = [
                    new BufferImageCopy(0, 0, (uint)src.SizeInBytes, layers, offset, extent)
                ];

                _vk.CmdCopyBufferToImage(_handle, srcHandle, dstHandle, ImageLayout.TransferDstOptimal, regions);
            }
        }
        else if (src is TextureVK srcTex)
        {
            Image srcHandle = *srcTex.Handle.NativePtr;
            Offset3D srcOffset = new Offset3D(0, 0, 0);

            // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.
            ImageSubresourceLayers srcLayers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
            Extent3D srcExtent = new Extent3D(srcTex.Width, srcTex.Height, srcTex.Depth);

            if (dest is BufferVK dstBuffer)
            {
                Buffer dstHandle = *dstBuffer.Handle.NativePtr;
                Span<BufferImageCopy> regions = [
                    new BufferImageCopy(0, 0, (uint)src.SizeInBytes, srcLayers, srcOffset, srcExtent)
                ];

                _vk.CmdCopyImageToBuffer(_handle, srcHandle, ImageLayout.TransferSrcOptimal, dstHandle, regions);
            }
            else if (dest is TextureVK dstTex)
            {
                Image dstHandle = *dstTex.Handle.NativePtr;
                Offset3D destOffset = new Offset3D(0, 0, 0);

                // TODO set the image aspect flags based on texture type. e.g. is DepthTextureVK or standard TextureVK/surface.

                ImageSubresourceLayers destLayers = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1);
                Span<ImageCopy> regions = [
                    new ImageCopy(srcLayers, srcOffset, destLayers, destOffset, srcExtent),
                ];

                _vk.CmdCopyImage(_handle, srcHandle, ImageLayout.TransferSrcOptimal, dstHandle, ImageLayout.TransferDstOptimal, regions);
            }
        }
        else
        {
            throw new InvalidOperationException($"Unsupported copy resource type '{src.GetType().Name}'");
        }
    }

    public override unsafe void CopyResourceRegion(GpuResource source, uint srcSubresource, ResourceRegion? sourceRegion, GpuResource dest, uint destSubresource, Vector3UI destStart)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// Queues a texture memory barrier.
    /// </summary>
    /// <param name="srcFlags">The source stage flags.</param>
    /// <param name="destFlags">The destination stage flags.</param>
    /// <param name="barrier">A pointer to one or more image memory barriers.</param>
    /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
    internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, ImageMemoryBarrier* barrier, uint barrierCount = 1)
    {
        _vk.CmdPipelineBarrier(_handle, srcFlags, destFlags, DependencyFlags.None, 0, null, 0, null, barrierCount, barrier);
    }

    /// <summary>
    /// Queues a buffer memory barrier.
    /// </summary>
    /// <param name="srcFlags">The source stage flags.</param>
    /// <param name="destFlags">The destination stage flags.</param>
    /// <param name="barrier">A pointer to one or more buffer memory barriers.</param>
    /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
    internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, BufferMemoryBarrier* barrier, uint barrierCount = 1)
    {
        _vk.CmdPipelineBarrier(_handle, srcFlags, destFlags, DependencyFlags.None, 0, null, barrierCount, barrier, 0, null);
    }

    /// <summary>
    /// Queues a global memory barrier command.
    /// </summary>
    /// <param name="srcFlags">The source stage flags.</param>
    /// <param name="destFlags">The destination stage flags.</param>
    /// <param name="barrier">A pointer to one or more global memory barriers.</param>
    /// <param name="barrierCount">The number of memory barriers in the <paramref name="barrier"/> parameter.</param>
    internal unsafe void MemoryBarrier(PipelineStageFlags srcFlags, PipelineStageFlags destFlags, MemoryBarrier* barrier, uint barrierCount = 1)
    {
        _vk.CmdPipelineBarrier(_handle, srcFlags, destFlags, DependencyFlags.None, barrierCount, barrier, 0, null, 0, null);
    }

    internal unsafe void ClearImage(Image image, ImageLayout layout, Color color, ImageSubresourceRange* pRanges, uint numRanges)
    {
        _vk.CmdClearColorImage(_handle, image, layout, *(ClearColorValue*)&color, numRanges, pRanges);
    }

    internal unsafe void ClearDepthImage(Image image, ImageLayout layout, float depthValue, uint stencilValue, ImageSubresourceRange* pRanges, uint numRanges)
    {
        ClearDepthStencilValue values = new ClearDepthStencilValue(depthValue, stencilValue);
        _vk.CmdClearDepthStencilImage(_handle, image, layout, &values, numRanges, pRanges);
    }

    protected override unsafe GpuBindResult DoRenderPass(ShaderPass hlslPass, QueueValidationMode mode, Action callback)
    {
        ShaderPassVK pass = hlslPass as ShaderPassVK;
        GpuBindResult vResult = Validate(mode);

        if (vResult != GpuBindResult.Successful)
            return vResult;

        DepthSurfaceVK depthSurface = State.DepthSurface.Value as DepthSurfaceVK;

        // Gather all surfaces and scissor rectangles.
        uint maxSurfaceCount = (uint)_applySurfaces.Length;
        uint surfaceID = 0;
        for (; surfaceID < maxSurfaceCount; surfaceID++)
        {
            IRenderSurfaceVK surface = State.Surfaces[surfaceID] as IRenderSurfaceVK;
            _applySurfaces[surfaceID] = surface;

            // Get scissor rectangles.
            Rectangle r = State.ScissorRects[surfaceID];
            _applyScissors[surfaceID] = new Rect2D()
            {
                Offset = new Offset2D(r.X, r.Y),
                Extent = new Extent2D((uint)r.Width, (uint)r.Height)
            };

            ViewportF vp = State.Viewports[surfaceID];
            _applyViewports[surfaceID] = new VKViewport()
            {
                X = vp.X,
                Y = vp.Y,
                Width = vp.Width,
                Height = vp.Height,
                MinDepth = vp.MinDepth,
                MaxDepth = vp.MaxDepth,
            };

            // Get clear color
            _applyClearValues[surfaceID] = new ClearValue();
            if (surface.ClearColor.HasValue)
            {
                Color4 color = surface.ClearColor.Value;
                _applyClearValues[surfaceID].Color = new ClearColorValue() // TODO handle formats that are not RGBA layout (e.g. BGRA)
                {
                    Float32_0 = color.R,
                    Float32_1 = color.G,
                    Float32_2 = color.B,
                    Float32_3 = color.A,
                };
            }
        }

        // Get depth surface clear value, if any.
        if (depthSurface != null)
        {
            _applyClearValues[surfaceID] = new ClearValue();
            if (depthSurface.ClearValue.HasValue)
                _applyClearValues[surfaceID].DepthStencil = depthSurface.ClearValue.Value;
        }

        // Re-render the same pass for K iterations.
        for (int k = 0; k < pass.Iterations; k++)
        {
            BeginEvent($"Iteration {k}");

            Buffer iBuffer = new Buffer();
            IndexType iType = IndexType.NoneKhr;
            if (State.IndexBuffer.BoundValue != null)
            {
                BufferVK vkIndexBuffer = State.IndexBuffer.BoundValue as BufferVK;
                iBuffer = *vkIndexBuffer.Handle.NativePtr;
                switch (vkIndexBuffer.ResourceFormat)
                {
                    case GpuResourceFormat.R16_UInt:
                        iType = IndexType.Uint16;
                        break;
                    case GpuResourceFormat.R32_UInt:
                        iType = IndexType.Uint32;
                        break;

                    case GpuResourceFormat.R8_UInt:
                        iType = IndexType.Uint8Ext;
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported index format '{vkIndexBuffer.ResourceFormat}'");
                }
            }

            PipelineStateVK pipeState = pass.State.GetState(_applySurfaces, depthSurface);
            FrameBufferVK frameBuffer = _device.GetFrameBuffer(pipeState.RenderPass, _applySurfaces, depthSurface);

            RenderPassBeginInfo rpInfo = new RenderPassBeginInfo()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = pipeState.RenderPass,
                Framebuffer = frameBuffer,
                RenderArea = _applyScissors[0],         // TODO Use a rectangle that contains all of the provided scissor rectangles
                PClearValues = _applyClearValues,       // TODO Gather clear values of all bound surfaces (value[0].DepthStencil is always for the depth-stencil clear value)
                ClearValueCount = maxSurfaceCount,      // TODO Gather clear values of all bound surfaces
                PNext = null
            };

            _device.VK.CmdBeginRenderPass(_handle, rpInfo, SubpassContents.Inline);
            _device.VK.CmdBindPipeline(_handle, PipelineBindPoint.Graphics, pipeState);

            _device.VK.CmdSetViewport(_handle, 0, maxSurfaceCount, _applyViewports); // TODO collect viewport handles and set them all at once.
            _device.VK.CmdSetScissor(_handle, 0, maxSurfaceCount, _applyScissors);

            _device.VK.CmdBindDescriptorSets(_handle, PipelineBindPoint.Graphics, pipeState.Layout, 0, 1, _applyDescSets, 0, null);
            _device.VK.CmdBindVertexBuffers(_handle, 0, 1, null, null);
            _device.VK.CmdBindIndexBuffer(_handle, iBuffer, 0, iType);

            callback.Invoke();
            _device.VK.CmdEndRenderPass(_handle);

            Profiler.DrawCalls++;
            EndEvent();
        }

        return GpuBindResult.Successful;
    }

    protected override GpuBindResult DoComputePass(ShaderPass hlslPass)
    {
        ShaderPassVK pass = hlslPass as ShaderPassVK;
        Vector3UI groups = DrawInfo.Custom.ComputeGroups;

        if (groups.X == 0)
            groups.X = pass.ComputeGroups.X;

        if (groups.Y == 0)
            groups.Y = pass.ComputeGroups.Y;

        if (groups.Z == 0)
            groups.Z = pass.ComputeGroups.Z;

        DrawInfo.ComputeGroups = groups;

        GpuBindResult vResult = Validate(QueueValidationMode.Compute);
        if (vResult != GpuBindResult.Successful)
            return vResult;

        // Re-render the same pass for K iterations.
        // TODO Use sub-passes?
        for (int j = 0; j < pass.Iterations; j++)
        {
            BeginEvent($"Iteration {j}");
            PipelineStateVK pipeState = pass.State.GetState(null, null);
            uint descriptorSetCount = 0;

            _vk.CmdBindPipeline(_handle, PipelineBindPoint.Compute, pipeState);
            //_vk.CmdBindDescriptorSets(_cmd, PipelineBindPoint.Compute, pipelineLayout, 0, descriptorSetCount, pDescriptorSets, 0, null);
            _vk.CmdDispatch(_handle, groups.X, groups.Y, groups.Z);

            Profiler.DispatchCalls++;
            EndEvent();
        }

        pass.InvokeCompleted(DrawInfo.Custom);
        return GpuBindResult.Successful;
    }

    public override GpuBindResult Draw(Shader shader, uint vertexCount, uint vertexStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Unindexed, () =>
            _vk.CmdDraw(_handle, vertexCount, 1, vertexStartIndex, 0));
    }

    public override GpuBindResult DrawInstanced(Shader shader, uint vertexCountPerInstance, uint instanceCount, uint vertexStartIndex = 0, uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Instanced, () =>
            _vk.CmdDraw(_handle, vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex));
    }

    public override GpuBindResult DrawIndexed(Shader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Indexed, () =>
            _vk.CmdDrawIndexed(_handle, indexCount, 1, startIndex, vertexIndexOffset, 0));
    }

    public override GpuBindResult DrawIndexedInstanced(Shader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.InstancedIndexed, () =>
            _vk.CmdDrawIndexed(_handle, indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex));
    }

    public override GpuBindResult Dispatch(Shader shader, Vector3UI groups)
    {
        return ApplyState(shader, QueueValidationMode.Compute, null);
    }

    protected override GpuBindResult CheckInstancing()
    {
        throw new NotImplementedException();
    }

    protected override void OnGraphicsRelease()
    {
        _applyScissors.Dispose();
        _applyViewports.Dispose();
        _applyClearValues.Dispose();
        _applyVertexBuffers.Dispose();

        Semaphore.Dispose();
        Fence.Dispose();
    }

    public static implicit operator CommandBuffer(CommandListVK list)
    {
        return list._handle;
    }

    internal bool IsFree { get; set; }

    internal CommandBuffer Ptr => _handle;

    internal CommandBufferLevel Level => _allocation.Level;

    internal SemaphoreVK Semaphore { get; }

    internal FenceVK Fence { get; set; }
}
