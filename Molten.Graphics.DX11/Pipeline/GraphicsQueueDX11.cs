using Newtonsoft.Json.Linq;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace Molten.Graphics.DX11
{
    /// <summary>Manages the pipeline of a either an immediate or deferred <see cref="GraphicsQueueDX11"/>.</summary>
    public unsafe partial class GraphicsQueueDX11 : GraphicsQueue
    {
        internal const uint D3D11_KEEP_UNORDERED_ACCESS_VIEWS = 0xffffffff;

        /// <summary>
        ///  If you set NumRTVs to D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL (0xffffffff), 
        ///  this method does not modify the currently bound render-target views (RTVs) and also does not modify depth-stencil view (DSV).
        /// </summary>
        internal const uint D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL = 0xffffffff;

        ShaderStageDX11[] _shaderStages;
        ShaderCSStage _cs;

        D3DPrimitiveTopology _boundTopology;
        VertexInputLayoutDX11 _inputLayout;
        List<VertexInputLayoutDX11> _cachedLayouts = new List<VertexInputLayoutDX11>();

        GraphicsDepthWritePermission _boundDepthMode = GraphicsDepthWritePermission.Enabled;

        ID3D11RenderTargetView1** _rtvs;
        ID3D11DepthStencilView* _dsv;

        BlendStateDX11 _stateBlend;
        RasterizerStateDX11 _stateRaster;
        DepthStateDX11 _stateDepth;
        GraphicsStateValueGroup<GraphicsResource> _omUAVs;
        CommandListDX11 _cmd;

        ID3D11DeviceContext4* _native;
        ID3DUserDefinedAnnotation* _debugAnnotation;

        internal GraphicsQueueDX11(DeviceDX11 device, ID3D11DeviceContext4* context) :
            base(device)
        {      
            DXDevice = device;
            _native = context;

            if (_native->GetType() == DeviceContextType.Immediate)
                Type = CommandQueueType.Immediate;
            else
                Type = CommandQueueType.Deferred;

            Guid debugGuid = ID3DUserDefinedAnnotation.Guid;
            void* ptrDebug = null;
            _native->QueryInterface(ref debugGuid, &ptrDebug);
            _debugAnnotation = (ID3DUserDefinedAnnotation*)ptrDebug;
            
            _cs = new ShaderCSStage(this);
            _shaderStages = new ShaderStageDX11[]
            {
                new ShaderVSStage(this),
                new ShaderHSStage(this),
                new ShaderDSStage(this),
                new ShaderGSStage(this),
                new ShaderPSStage(this)
            };

            uint maxRTs = Device.Capabilities.PixelShader.MaxOutputTargets;
            uint numRenderUAVs = Device.Capabilities.VertexShader.MaxUnorderedAccessSlots;
            _omUAVs = new GraphicsStateValueGroup<GraphicsResource>(numRenderUAVs);
            _rtvs = EngineUtil.AllocPtrArray<ID3D11RenderTargetView1>(maxRTs);
        }

        protected override void OnResetState()
        {
            // Bind default state to all shader stages 
            for (int i = 0; i < _shaderStages.Length; i++)
                _shaderStages[i].Bind(null);

            // Unbind all output surfaces
            _native->OMSetRenderTargets(0, null, _dsv);
        }

        public override void Begin(GraphicsCommandListFlags flags = GraphicsCommandListFlags.None)
        {
            base.Begin(flags);

            if (Type != CommandQueueType.Deferred)
                _cmd = null;
        }

        public override GraphicsCommandList End()
        {
            base.End();

            if (Type == CommandQueueType.Deferred)
            {
                ID3D11CommandList* ptrCmd = EngineUtil.Alloc<ID3D11CommandList>();

                _native->FinishCommandList(false, &ptrCmd);
                _cmd = new CommandListDX11(this, ptrCmd);
                Device.Frame.Track(_cmd);

                if (_cmd.Flags.Has(GraphicsCommandListFlags.CpuSyncable))
                    _cmd.Fence = new GraphicsOpenFence(Device);
            }

            return _cmd;
        }

        public override void Execute(GraphicsCommandList list)
        {
            if (!list.Flags.Has(GraphicsCommandListFlags.Deferred))
                throw new GraphicsCommandQueueException(this, "Cannot execute an immediate-mode command list. Use Submit() instead.");

            CommandListDX11 cmd = list as CommandListDX11;
            _native->ExecuteCommandList(cmd.Ptr, true);
        }

        public override void Sync(GraphicsCommandListFlags flags)
        {
            if (flags.Has(GraphicsCommandListFlags.Deferred))
                throw new GraphicsCommandQueueException(this, "Cannot submit deferred command lists to the immediate graphics queue");
        }

        protected override unsafe ResourceMap GetResourcePtr(GraphicsResource resource, uint subresource, GraphicsMapType mapType)
        {
            Map map = 0;
            GraphicsResourceFlags flags = resource.Flags;

            if (mapType == GraphicsMapType.Read)
            {
                if (flags.Has(GraphicsResourceFlags.CpuRead))
                {
                    map |= Map.Read;

                    // Only increment if we haven't already incremented during write flag check (above).
                    if (!flags.Has(GraphicsResourceFlags.CpuWrite))
                        Profiler.ResourceMapCalls++;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot map a resource for reading without CPU read access");
                }
            }
            else
            {
                if (flags.Has(GraphicsResourceFlags.CpuWrite))
                {
                    if (mapType == GraphicsMapType.Discard)
                    {
                        map = Map.WriteDiscard;
                    }
                    else
                    {
                        if (resource is GraphicsBuffer buffer 
                            && (buffer.BufferType == GraphicsBufferType.Vertex 
                            || buffer.BufferType == GraphicsBufferType.Index))
                        {
                            map = Map.WriteNoOverwrite;
                        }
                        else
                        {
                            if (resource.Flags.Has(GraphicsResourceFlags.CpuWrite) 
                                && !resource.Flags.Has(GraphicsResourceFlags.CpuRead) 
                                && !resource.Flags.Has(GraphicsResourceFlags.GpuWrite))
                                map = Map.WriteNoOverwrite;
                            else
                                map = Map.Write;
                        }
                    }

                    Profiler.ResourceMapCalls++;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot map a resource for writing without CPU write access");
                }
            }

            MappedSubresource resMap = new MappedSubresource();
            _native->Map((ResourceHandleDX11)resource.Handle, subresource, map, 0, &resMap);

            // Check for valid resource map handle.
            if (resMap.PData == null)
            {
                (Device as DeviceDX11).ProcessDebugLayerMessages();
                throw new InvalidOperationException($"Failed to map resource {resource.Name} for {mapType} access");
            }

            return new ResourceMap(resMap.PData, resMap.RowPitch, resMap.DepthPitch);
        }

        protected override void OnUnmapResource(GraphicsResource resource, uint subresource)
        {
            _native->Unmap((ResourceHandleDX11)resource.Handle, subresource);
        }

        public override unsafe void CopyResourceRegion(
            GraphicsResource source, uint srcSubresource, ResourceRegion* sourceRegion, 
            GraphicsResource dest, uint destSubresource, Vector3UI destStart)
        {
            Box* box = (Box*)sourceRegion;

            _native->CopySubresourceRegion((ResourceHandleDX11)dest.Handle, destSubresource, destStart.X, destStart.Y, destStart.Z, (ResourceHandleDX11)source.Handle, srcSubresource, box);
            Profiler.SubResourceCopyCalls++;
        }

        protected override unsafe void UpdateResource(GraphicsResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch)
        {
            Box* destBox = null;

            if (region != null)
            {
                ResourceRegion value = region.Value;
                destBox = (Box*)&value;
            }

            _native->UpdateSubresource((ResourceHandleDX11)resource.Handle, subresource, destBox, ptrData, rowPitch, slicePitch);
            Profiler.SubResourceUpdateCalls++;
        }

        protected override void CopyResource(GraphicsResource src, GraphicsResource dest)
        {
            src.Ensure(this);

            if(dest is BufferDX11 buffer && buffer.BufferType == GraphicsBufferType.Staging)
                dest.Ensure(this);

            _native->CopyResource((ResourceHandleDX11)dest.Handle, (ResourceHandleDX11)src.Handle);
            Profiler.ResourceCopyCalls++;
        }

        protected override GraphicsBindResult DoRenderPass(HlslPass hlslPass, QueueValidationMode mode, Action callback)
        {
            ShaderPassDX11 pass = hlslPass as ShaderPassDX11;
            D3DPrimitiveTopology passTopology = pass.Topology.ToApi();

            if (passTopology == D3DPrimitiveTopology.D3D11PrimitiveTopologyUndefined)
                return GraphicsBindResult.UndefinedTopology;

            // Clear output merger and rebind targets later.
            _native->OMSetRenderTargets(0, null, null);

            // Check topology
            if (_boundTopology != passTopology)
            {
                _boundTopology = passTopology;
                _native->IASetPrimitiveTopology(_boundTopology);
            }

            _omUAVs.Reset();

            Span<bool> stageChanged = stackalloc bool[_shaderStages.Length];
            for(int i = 0; i < _shaderStages.Length; i++)
            {
                ShaderComposition composition = pass[_shaderStages[i].Type];
                stageChanged[i] = _shaderStages[i].Bind(composition);

                // Set the output-merger UAVs needed by each render stage
                if (composition != null)
                {
                    for (int j = 0; j < composition.UnorderedAccessIds.Count; j++)
                    {
                        uint slotID = composition.UnorderedAccessIds[j];
                        _omUAVs[slotID] = composition.Pass.Parent.UAVs[slotID]?.Resource;
                    }
                }
            }

            bool vsChanged = stageChanged[0]; // Stage 0 is vertex buffer.
            bool ibChanged = State.IndexBuffer.Bind(this);
            bool vbChanged = State.VertexBuffers.Bind(this);

            bool omUavChanged = _omUAVs.Bind(this);
            if (omUavChanged)
            {
                // Set unordered access resources
                int count = _omUAVs.Length;
                ID3D11UnorderedAccessView** pUavs = stackalloc ID3D11UnorderedAccessView*[count];
                for (int i = 0; i < count; i++)
                {
                    if (_omUAVs.BoundValues[i] != null)
                        pUavs[i] = (ID3D11UnorderedAccessView*)_omUAVs.BoundValues[i].UAV;
                    else
                        pUavs = null;
                }

                _native->OMGetRenderTargetsAndUnorderedAccessViews(D3D11_KEEP_RENDER_TARGETS_AND_DEPTH_STENCIL, null, null, 0, (uint)count, pUavs);
            }

            if (ibChanged)
            {
                if (State.IndexBuffer.BoundValue != null)
                {
                    BufferDX11 buffer = State.IndexBuffer.BoundValue as BufferDX11;
                    uint byteOffset = 0; // TODO value.ByteOffset - May need again later for multi-part meshes.
                    _native->IASetIndexBuffer((ID3D11Buffer*)buffer.Handle, buffer.D3DFormat, byteOffset);
                }
                else
                {
                    _native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
                }
            }

            // Does the vertex input layout need updating?
            if (vbChanged || vsChanged)
            {
                if (vbChanged)
                    BindVertexBuffers();

                _inputLayout = GetInputLayout(pass);
                if (_inputLayout == null || _inputLayout.IsNullBuffer)
                    _native->IASetInputLayout(null);
                else
                    _native->IASetInputLayout(_inputLayout);
            }

            // Bind blend state
            if (_stateBlend != pass.BlendState)
            {
                _stateBlend = pass.BlendState;
                Color4 tmp = _stateBlend.BlendFactor;
                if (_stateBlend != null)
                    _native->OMSetBlendState(_stateBlend, (float*)&tmp, _stateBlend.BlendSampleMask);
                else
                    _native->OMSetBlendState(null, null, 0);
            }

            // Bind depth-stencil state.
            if (_stateDepth != pass.DepthState)
            {
                _stateDepth = pass.DepthState;
                if (_stateDepth != null)
                    _native->OMSetDepthStencilState(_stateDepth.NativePtr, _stateDepth.StencilReference);
                else
                    _native->OMSetDepthStencilState(null, 0);
            }

            if(_stateRaster != pass.RasterizerState)
            {
                _stateRaster = pass.RasterizerState;
                _native->RSSetState(_stateRaster); // A null value is fine here.
            }

            // Check if scissor rects need updating
            if (State.ScissorRects.IsDirty)
            {
                fixed (Rectangle* ptrRect = State.ScissorRects.Items)
                    _native->RSSetScissorRects((uint)State.ScissorRects.Length, (Box2D<int>*)ptrRect);

                State.ScissorRects.IsDirty = false;
            }

            // Check if viewports need updating.
            // TODO Consolidate - Molten viewports are identical in memory layout to DX11 viewports.
            if (State.Viewports.IsDirty)
            {
                fixed (ViewportF* ptrViewports = State.Viewports.Items)
                    _native->RSSetViewports((uint)State.Viewports.Length, (Silk.NET.Direct3D11.Viewport*)ptrViewports);

                State.Viewports.IsDirty = false;
            }

            GraphicsDepthWritePermission depthWriteMode = pass.WritePermission;
            bool surfaceChanged = State.Surfaces.Bind(this);
            bool depthChanged = State.DepthSurface.Bind(this) || (_boundDepthMode != depthWriteMode);

            if (surfaceChanged || depthChanged)
            {
                if (surfaceChanged)
                {
                    for (int i = 0; i < State.Surfaces.Length; i++)
                    {
                        if (State.Surfaces.BoundValues[i] != null)
                        {
                            SurfaceHandleDX11 rsHandle = State.Surfaces.BoundValues[i].Handle as SurfaceHandleDX11;
                            _rtvs[i] = rsHandle.RTV.Ptr;
                        }
                        else
                        {
                            _rtvs[i] = null;
                        }
                    }
                }

                if (depthChanged)
                {
                    if (State.DepthSurface.BoundValue != null && depthWriteMode != GraphicsDepthWritePermission.Disabled)
                    {
                        DepthSurfaceDX11 dss = State.DepthSurface.BoundValue as DepthSurfaceDX11;
                        if (depthWriteMode == GraphicsDepthWritePermission.ReadOnly)
                            _dsv = dss.ReadOnlyView;
                        else
                            _dsv = dss.DepthView;
                    }
                    else
                    {
                        _dsv = null;
                    }

                    _boundDepthMode = depthWriteMode;
                }
            }

            _native->OMSetRenderTargets((uint)State.Surfaces.Length, (ID3D11RenderTargetView**)_rtvs, _dsv);
            Profiler.BindSurfaceCalls++;

            GraphicsBindResult vResult = Validate(mode);

            if (vResult == GraphicsBindResult.Successful)
            {
                // Re-render the same pass for K iterations.
                for (int k = 0; k < pass.Iterations; k++)
                {
                    BeginEvent($"Iteration {k}");
                    callback();
                    Profiler.DrawCalls++;
                    EndEvent();
                }
            }

            // Validate pipeline state.
            return vResult;
        }

        protected override GraphicsBindResult DoComputePass(HlslPass hlslPass)
        {
            ShaderPassDX11 pass = hlslPass as ShaderPassDX11;
            _cs.Bind(pass[ShaderType.Compute]);

            Vector3UI groups = DrawInfo.Custom.ComputeGroups;

            if (groups.X == 0)
                groups.X = pass.ComputeGroups.X;

            if (groups.Y == 0)
                groups.Y = pass.ComputeGroups.Y;

            if (groups.Z == 0)
                groups.Z = pass.ComputeGroups.Z;

            DrawInfo.ComputeGroups = groups;

            GraphicsBindResult vResult = Validate(QueueValidationMode.Compute);
            if (vResult == GraphicsBindResult.Successful)
            {

                // Re-render the same pass for K iterations.
                for (int j = 0; j < pass.Iterations; j++)
                {
                    BeginEvent($"Iteration {j}");
                    _native->Dispatch(groups.X, groups.Y, groups.Z);
                    Profiler.DispatchCalls++;
                    EndEvent();
                }
            }
            return vResult;
        }

        private void BindVertexBuffers()
        {
            int count = State.VertexBuffers.Length;
            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[count];
            uint* pStrides = stackalloc uint[count];
            uint* pOffsets = stackalloc uint[count];
            GraphicsBuffer buffer = null;

            for (int i = 0; i < count; i++)
            {
                buffer = State.VertexBuffers.BoundValues[i];

                if (buffer != null)
                {
                    pBuffers[i] = ((ResourceHandleDX11<ID3D11Buffer>)buffer.Handle);
                    pStrides[i] = buffer.Stride;
                    pOffsets[i] = 0; // TODO buffer.ByteOffset; - May need again for multi-part meshes with sub-meshes within the same buffer.
                }
                else
                {
                    pBuffers[i] = null;
                    pStrides[i] = 0;
                    pOffsets[i] = 0;
                }
            }

            _native->IASetVertexBuffers(0, (uint)count, pBuffers, pStrides, pOffsets);
        }

        public override void BeginEvent(string label)
        {
            fixed(char* ptr = label)
                _debugAnnotation->BeginEvent(ptr);
        }

        public override void EndEvent()
        {
            _debugAnnotation->EndEvent();
        }

        public override void SetMarker(string label)
        {
            fixed (char* ptr = label)
                _debugAnnotation->SetMarker(ptr);
        }

        public override GraphicsBindResult Draw(HlslShader shader, uint vertexCount, uint vertexStartIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.Unindexed, () => _native->Draw(vertexCount, vertexStartIndex));
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawInstanced(HlslShader shader,
            uint vertexCountPerInstance,
            uint instanceCount,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.Instanced, () =>
            _native->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex));
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexed(HlslShader shader,
            uint indexCount,
            int vertexIndexOffset = 0,
            uint startIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.Indexed, () => _native->DrawIndexed(indexCount, startIndex, vertexIndexOffset));
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexedInstanced(HlslShader shader,
            uint indexCountPerInstance,
            uint instanceCount,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0)
        {
            return ApplyState(shader, QueueValidationMode.InstancedIndexed, () =>
                _native->DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex));
        }

        /// <summary>
        /// Dispatches a <see cref="HlslShader"/> as a compute shader. Any non-compute passes will be skipped.
        /// </summary>
        /// <param name="shader">The shader to be dispatched.</param>
        /// <param name="groups">The number of thread groups.</param>
        /// <returns></returns>
        public override GraphicsBindResult Dispatch(HlslShader shader, Vector3UI groups)
        {
            DrawInfo.Custom.ComputeGroups = groups;
            return ApplyState(shader, QueueValidationMode.Compute, null);
        }

        /// <summary>Retrieves or creates a usable input layout for the provided vertex buffers and sub-effect.</summary>
        /// <returns>An instance of InputLayout.</returns>
        private VertexInputLayoutDX11 GetInputLayout(HlslPass pass)
        {
            // Retrieve layout list or create new one if needed.
            foreach (VertexInputLayoutDX11 l in _cachedLayouts)
            {
                if (l.IsMatch(Device.Log, State.VertexBuffers))
                    return l;
            }

            ShaderComposition vs = pass[ShaderType.Vertex];
            VertexInputLayoutDX11 input = new VertexInputLayoutDX11(DXDevice, State.VertexBuffers, (ID3D10Blob*)pass.InputByteCode, vs.InputLayout);
            _cachedLayouts.Add(input);

            return input;
        }

        protected override GraphicsBindResult CheckInstancing()
        {
            if (_inputLayout != null && _inputLayout.IsInstanced)
                return GraphicsBindResult.Successful;
            else
                return GraphicsBindResult.NonInstancedVertexLayout;
        }

        /// <summary>Dispoes of the current <see cref="Graphics.GraphicsQueueDX11"/> instance.</summary>
        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _native);
            SilkUtil.ReleasePtr(ref _debugAnnotation);

            // Dispose context.
            if (Type != CommandQueueType.Immediate)
                DXDevice.RemoveDeferredContext(this);

            EngineUtil.FreePtrArray(ref _rtvs);
            _cmd.Dispose();
        }

        /// <summary>Gets the current <see cref="GraphicsQueueDX11"/> type. This value will not change during the context's life.</summary>
        internal CommandQueueType Type { get; private set; }

        internal DeviceDX11 DXDevice { get; private set; }

        protected override GraphicsCommandList Cmd
        {
            get => _cmd;
            set => _cmd = value as CommandListDX11;
        }

        internal ID3D11DeviceContext4* Ptr => _native;

        internal ID3DUserDefinedAnnotation* Debug => _debugAnnotation;
    }
}
