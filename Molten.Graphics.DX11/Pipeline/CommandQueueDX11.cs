using Molten.IO;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace Molten.Graphics
{
    internal delegate void CmdQueueDrawCallback();
    internal delegate void CmdQueueDrawFailCallback(MaterialPassDX11 pass, uint passNumber, GraphicsBindResult result);

    /// <summary>Manages the pipeline of a either an immediate or deferred <see cref="CommandQueueDX11"/>.</summary>
    public unsafe partial class CommandQueueDX11 : GraphicsCommandQueue
    {
        ID3D11DeviceContext4* _context;

        Rectangle[] _scissorRects;
        bool _scissorRectsDirty;

        ViewportF[] _viewports;
        bool _viewportsDirty;
        ViewportF[] _nullViewport;
        ID3DUserDefinedAnnotation* _debugAnnotation;

        ShaderStageDX11[] _shaderStages;
        ShaderCSStage _cs;
         GraphicsSlot<ComputeTask> _compute;

        D3DPrimitiveTopology _boundTopology;
        GraphicsSlot<VertexInputLayout> _vertexLayout;
        List<VertexInputLayout> _cachedLayouts = new List<VertexInputLayout>();

        GraphicsDepthWritePermission _boundDepthMode = GraphicsDepthWritePermission.Enabled;

        internal ID3D11RenderTargetView1** RTVs;
        uint _numRTVs;
        internal ID3D11DepthStencilView* DSV;

        GraphicsSlot<BlendStateDX11> _stateBlend;
        GraphicsSlot<RasterizerStateDX11> _stateRaster;
        GraphicsSlot<DepthStateDX11> _stateDepth;

        internal CommandQueueDX11(DeviceDX11 device, ID3D11DeviceContext4* context) :
            base(device)
        {
            _context = context;
            DXDevice = device;

            if (_context->GetType() == DeviceContextType.Immediate)
                Type = GraphicsContextType.Immediate;
            else
                Type = GraphicsContextType.Deferred;

            _nullViewport = new ViewportF[1];

            Guid debugGuid = ID3DUserDefinedAnnotation.Guid;
            void* ptrDebug = null;
            _context->QueryInterface(ref debugGuid, &ptrDebug);
            _debugAnnotation = (ID3DUserDefinedAnnotation*)ptrDebug;

            uint maxRTs = Device.Adapter.Capabilities.PixelShader.MaxOutResources;
            _scissorRects = new Rectangle[maxRTs];
            _viewports = new ViewportF[maxRTs];

            uint maxVBuffers = Device.Adapter.Capabilities.VertexBuffers.MaxSlots;
            VertexBuffers = RegisterSlotGroup<IGraphicsBufferSegment, VertexBufferGroupBinder>(GraphicsBindTypeFlags.Input, "V-Buffer", maxVBuffers);
            IndexBuffer = RegisterSlot<IGraphicsBufferSegment, IndexBufferBinder>(GraphicsBindTypeFlags.Input, "I-Buffer", 0);
            _vertexLayout = RegisterSlot<VertexInputLayout, InputLayoutBinder>(GraphicsBindTypeFlags.Input, "Vertex Input Layout", 0);
            Material = RegisterSlot<Material, MaterialBinder>(GraphicsBindTypeFlags.Input, "Material", 0);
            _compute = RegisterSlot<ComputeTask, ComputeTaskBinder>(GraphicsBindTypeFlags.Input, "Compute Task", 0);

            _shaderStages = new ShaderStageDX11[]
            {
                new ShaderVSStage(this),
                new ShaderHSStage(this),
                new ShaderDSStage(this),
                new ShaderGSStage(this),
                new ShaderPSStage(this)
            };

            _cs = new ShaderCSStage(this);

            _stateBlend = RegisterSlot<BlendStateDX11, BlendBinder>(GraphicsBindTypeFlags.Input, "Blend State", 0);
            _stateDepth = RegisterSlot<DepthStateDX11, DepthStencilBinder>(GraphicsBindTypeFlags.Input, "Depth-Stencil State", 0);
            _stateRaster = RegisterSlot<RasterizerStateDX11, RasterizerBinder>(GraphicsBindTypeFlags.Input, "Rasterizer State", 0);

            RTVs = EngineUtil.AllocPtrArray<ID3D11RenderTargetView1>(maxRTs);

            Surfaces = RegisterSlotGroup<IRenderSurface2D, SurfaceGroupBinder>(GraphicsBindTypeFlags.Output, "Render Surface", maxRTs);
            DepthSurface = RegisterSlot<IDepthStencilSurface, DepthSurfaceBinder>(GraphicsBindTypeFlags.Output, "Depth Surface", 0);

            // Apply the surface of the graphics device's output initialally.
            SetRenderSurfaces(null);
        }

        internal void Clear()
        {
            Native->ClearState();
        }

        /// <summary>
        /// Maps a resource on the current <see cref="CommandQueueDX11"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <param name="subresource"></param>
        /// <param name="mapType"></param>
        /// <param name="mapFlags"></param>
        /// <returns></returns>
        internal MappedSubresource MapResource<T>(T* resource, uint subresource, Map mapType, MapFlag mapFlags)
            where T : unmanaged
        {
            MappedSubresource mapping = new MappedSubresource();
            Native->Map((ID3D11Resource*)resource, subresource, mapType, (uint)mapFlags, ref mapping);

            return mapping;
        }

        /// <summary>
        /// Maps a resource on the current <see cref="CommandQueueDX11"/> and provides a <see cref="RawStream"/> to aid read-write operations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <param name="subresource"></param>
        /// <param name="mapType"></param>
        /// <param name="mapFlags"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal MappedSubresource MapResource<T>(T* resource, uint subresource, Map mapType, MapFlag mapFlags, out RawStream stream)
            where T: unmanaged
        {
            MappedSubresource mapping = new MappedSubresource();
            Native->Map((ID3D11Resource*)resource, subresource, mapType, (uint)mapFlags, ref mapping);

            bool canWrite = mapType != Map.Read;
            bool canRead = mapType == Map.Read || mapType == Map.ReadWrite;
            stream = new RawStream(mapping.PData, uint.MaxValue, canRead, canWrite);

            return mapping;
        }

        internal void UnmapResource<T>(T* resource, uint subresource)
            where T : unmanaged
        {
            Native->Unmap((ID3D11Resource*)resource, subresource);
        }

        internal void CopyResourceRegion(
            ID3D11Resource* source, uint srcSubresource, ref Box sourceRegion, 
            ID3D11Resource* dest, uint destSubresource, Vector3UI destStart)
        {
            Native->CopySubresourceRegion(dest, destSubresource, destStart.X, destStart.Y, destStart.Z,
                source, srcSubresource, ref sourceRegion);

            Profiler.Current.CopySubresourceCount++;
        }

        internal void CopyResourceRegion(
            ID3D11Resource* source, uint srcSubresource, Box* sourceRegion,
            ID3D11Resource* dest, uint destSubresource, Vector3UI destStart)
        {
            Native->CopySubresourceRegion(dest, destSubresource, destStart.X, destStart.Y, destStart.Z,
                source, srcSubresource, sourceRegion);

            Profiler.Current.CopySubresourceCount++;
        }

        internal void UpdateResource(ID3D11Resource* resource, uint subresource, 
            Box* region, void* ptrData, uint rowPitch, uint slicePitch)
        {
            Native->UpdateSubresource(resource, subresource, region, ptrData, rowPitch, slicePitch);
            Profiler.Current.UpdateSubresourceCount++;
        }

        private GraphicsBindResult ApplyState(MaterialPassDX11 pass,
            QueueValidationMode mode)
        {
            if (pass.Topology == D3DPrimitiveTopology.D3D11PrimitiveTopologyUndefined)
                return GraphicsBindResult.UndefinedTopology;

            bool matChanged = Material.Bind();

            // Check topology
            if (_boundTopology != pass.Topology)
            {
                _boundTopology = pass.Topology;
                Native->IASetPrimitiveTopology(_boundTopology);
            }

            Span<bool> stageChanged = stackalloc bool[_shaderStages.Length];

            for(int i = 0; i < _shaderStages.Length; i++)
            {
                ShaderComposition comp = pass[_shaderStages[i].Type];
                _shaderStages[i].Shader.Value = comp;
                stageChanged[i] = _shaderStages[i].Bind();
            }

            bool vsChanged = stageChanged[0]; // Stage 0 is vertex buffer.
            bool ibChanged = IndexBuffer.Bind();
            bool vbChanged = VertexBuffers.BindAll();

            // Check index buffer
            if (ibChanged)
            {
                BufferSegment ib = IndexBuffer.BoundValue as BufferSegment;

                if (ib != null)
                    Native->IASetIndexBuffer(ib, ib.DataFormat, ib.ByteOffset);
                else
                    Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
            }

            // Does the vertex input layout need updating?
            if (vbChanged || vsChanged)
            {
                _vertexLayout.Value = GetInputLayout(pass);
                _vertexLayout.Bind();
            }

            GraphicsDepthWritePermission depthWriteMode = pass.WritePermission;
            _stateBlend.Value = pass.BlendState;
            _stateDepth.Value = pass.DepthState;
            _stateRaster.Value = pass.RasterizerState;

            bool bStateChanged = _stateBlend.Bind();
            bool rStateChanged = _stateDepth.Bind();
            bool dStateChanged = _stateRaster.Bind();

            // Check if scissor rects need updating
            if (_scissorRectsDirty)
            {
                fixed (Rectangle* ptrRect = _scissorRects)
                    Native->RSSetScissorRects((uint)_scissorRects.Length, (Rectangle<int>*)ptrRect);

                _scissorRectsDirty = false;
            }

            // Check if viewports need updating.
            // TODO Consolidate - Molten viewports are identical in memory layout to DX11 viewports.
            if (_viewportsDirty)
            {
                fixed (ViewportF* ptrViewports = _viewports)
                    Native->RSSetViewports((uint)_viewports.Length, (Silk.NET.Direct3D11.Viewport*)ptrViewports);

                _viewportsDirty = false;
            }

            bool surfaceChanged = Surfaces.BindAll();
            bool depthChanged = DepthSurface.Bind() || (_boundDepthMode != depthWriteMode);

            if (surfaceChanged || depthChanged)
            {
                if (surfaceChanged)
                {
                    _numRTVs = 0;

                    for (uint i = 0; i < Surfaces.SlotCount; i++)
                    {
                        if (Surfaces[i].BoundValue != null)
                        {
                            _numRTVs = (i + 1);
                            RTVs[i] = (Surfaces[i].BoundValue as RenderSurface2D).RTV.Ptr;
                        }
                        else
                        {
                            RTVs[i] = null;
                        }
                    }
                }

                if (depthChanged)
                {
                    if (DepthSurface.BoundValue != null && depthWriteMode != GraphicsDepthWritePermission.Disabled)
                    {
                        DepthStencilSurface dss = DepthSurface.BoundValue as DepthStencilSurface;
                        if (depthWriteMode == GraphicsDepthWritePermission.ReadOnly)
                            DSV = dss.ReadOnlyView;
                        else
                            DSV = dss.DepthView;
                    }
                    else
                    {
                        DSV = null;
                    }

                    _boundDepthMode = depthWriteMode;
                }

                Native->OMSetRenderTargets(_numRTVs, (ID3D11RenderTargetView**)RTVs, DSV);
                Profiler.Current.SurfaceBindings++;
            }

            // Validate pipeline state.
            return Validate(mode);
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

        private GraphicsBindResult DrawCommon(Material mat, QueueValidationMode mode, 
            CmdQueueDrawCallback drawCallback, CmdQueueDrawFailCallback failCallback)
        {
            GraphicsBindResult vResult = GraphicsBindResult.Successful;

            if (!DrawInfo.Began)
                throw new GraphicsCommandQueueException(this, $"{nameof(CommandQueueDX11)}: BeginDraw() must be called before calling {nameof(Draw)}()");

            Material.Value = mat;

            // Re-render the same material for mat.Iterations.
            BeginEvent($"Draw '{mode}' Call");
            for (uint j = 0; j < mat.Passes.Length; j++)
            {
                BeginEvent($"Pass {j}");
                MaterialPassDX11 pass = mat.Passes[j] as MaterialPassDX11;
                vResult = ApplyState(pass, mode);

                if (vResult == GraphicsBindResult.Successful)
                {
                    // Re-render the same pass for K iterations.
                    for (int k = 0; k < pass.Iterations; k++)
                    {
                        BeginEvent($"Iteration {k}");
                        drawCallback();
                        (Device as DeviceDX11).ProcessDebugLayerMessages();
                        Profiler.Current.DrawCalls++;
                        EndEvent();
                    }

                    EndEvent();
                }
                else
                {
                    EndEvent();
                    failCallback(pass, j, vResult);
                    (Device as DeviceDX11).ProcessDebugLayerMessages();
                    break;
                }
            }
            EndEvent();

            return vResult;
        }

        public override GraphicsBindResult Draw(Material material, uint vertexCount, uint vertexStartIndex = 0)
        {
            return DrawCommon(material, QueueValidationMode.Unindexed, () => _context->Draw(vertexCount, vertexStartIndex),
            (pass, passNumber, vResult) =>
            {
                Device.Log.Warning($"Draw() call failed with result: {vResult} -- " + 
                    $"Iteration: P{passNumber}/{material.Passes.Length} -- " +
                    $"Material: {material.Name} -- Topology: {pass.Topology} -- VertexCount: { vertexCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawInstanced(Material material,
            uint vertexCountPerInstance,
            uint instanceCount,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material, QueueValidationMode.Instanced, () => 
            _context->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex),
            (pass, passNum, vResult) =>
            {
                Device.Log.Warning($"DrawInstanced() call failed with result: {vResult} -- " + 
                        $"Pass {passNum}/{material.Passes.Length} -- Material: {material.Name} -- " +
                        $"Topology: {pass.Topology} -- VertexCount: { vertexCountPerInstance} -- Instances: {instanceCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexed(Material material,
            uint indexCount,
            int vertexIndexOffset = 0,
            uint startIndex = 0)
        {
            return DrawCommon(material, QueueValidationMode.Indexed, () => _context->DrawIndexed(indexCount, startIndex, vertexIndexOffset),
            (pass, passNum, vResult) =>
            {
                Device.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Pass {passNum}/{material.Passes.Length} -- Material: {material.Name} -- Topology: {pass.Topology} -- indexCount: { indexCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexedInstanced(Material material,
            uint indexCountPerInstance,
            uint instanceCount,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material, QueueValidationMode.InstancedIndexed, () =>
            {
                _context->DrawIndexedInstanced(indexCountPerInstance, instanceCount,
                    startIndex, vertexIndexOffset, instanceStartIndex);
            },
            (pass, passNum, vResult) =>
            {
                Device.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Pass {passNum}/{material.Passes.Length} -- Material: {material.Name} -- Topology: {pass.Topology} -- Indices-per-instance: { indexCountPerInstance}");
            });
        }

        /// <inheritdoc/>
        public override void Dispatch(ComputeTask task, uint groupsX, uint groupsY, uint groupsZ)
        {
            bool csChanged = Bind(task);

            if (_cs.Shader.BoundValue == null)
            {
                return;
            }
            else
            {
                ComputeCapabilities comCap = Device.Adapter.Capabilities.Compute;

                if (groupsZ > comCap.MaxGroupCountZ)
                {
                    Device.Log.Error($"Unable to dispatch compute shader. Z dimension ({groupsZ}) is greater than supported ({comCap.MaxGroupCountZ}).");
                    return;
                }
                else if (groupsX > comCap.MaxGroupCountX)
                {
                    Device.Log.Error($"Unable to dispatch compute shader. X dimension ({groupsX}) is greater than supported ({comCap.MaxGroupCountX}).");
                    return;
                }
                else if (groupsY > comCap.MaxGroupCountY)
                {
                    Device.Log.Error($"Unable to dispatch compute shader. Y dimension ({groupsY}) is greater than supported ({comCap.MaxGroupCountY}).");
                    return;
                }

                // TODO have this processed during the presentation call of each graphics context.
                // 
                Native->Dispatch(groupsX, groupsY, groupsZ);
            }
        }

        public bool Bind(ComputeTask task)
        {
            _compute.Value = task;
            _compute.Bind();
            _cs.Shader.Value = _compute.BoundValue.Composition;

            bool csChanged = _cs.Bind();

            if (_cs.Shader.BoundValue != null)
            {
                // Apply unordered acces views to slots
                for (int i = 0; i < _cs.Shader.BoundValue.UnorderedAccessIds.Count; i++)
                {
                    uint slotID = _cs.Shader.BoundValue.UnorderedAccessIds[i];
                    _cs.UAVs[slotID].Value = _compute.BoundValue.UAVs[slotID]?.UnorderedResource as GraphicsResourceDX11;
                }
            }

            return csChanged;
        }

        public override void SetRenderSurfaces(IRenderSurface2D[] surfaces, uint count)
        {
            if (surfaces != null)
            {
                for (uint i = 0; i < count; i++)
                    Surfaces[i].Value = surfaces[i] as RenderSurface2D;
            }
            else
            {
                count = 0;
            }

            // Set the remaining surfaces to null.
            for (uint i = count; i < Surfaces.SlotCount; i++)
                Surfaces[i].Value = null;
        }

        public override void SetRenderSurface(IRenderSurface2D surface, uint slot)
        {
            Surfaces[slot].Value = surface as RenderSurface2D;
        }

        public override void GetRenderSurfaces(IRenderSurface2D[] destinationArray)
        {
            if (destinationArray.Length < Surfaces.SlotCount)
                throw new InvalidOperationException($"The destination array is too small ({destinationArray.Length}). A minimum size of {Surfaces.SlotCount} is needed.");

            for (uint i = 0; i < Surfaces.SlotCount; i++)
                destinationArray[i] = Surfaces[i].Value;
        }

        
        public override IRenderSurface2D GetRenderSurface(uint slot)
        {
            return Surfaces[slot].Value;
        }
 
        public override void ResetRenderSurfaces()
        {
            for (uint i = 0; i < Surfaces.SlotCount; i++)
                Surfaces[i].Value = null;
        }

        public override void SetScissorRectangle(Rectangle rect, int slot = 0)
        {
            _scissorRects[slot] = rect;
            _scissorRectsDirty = true;
        }

        public override void SetScissorRectangles(params Rectangle[] rects)
        {
            for (int i = 0; i < rects.Length; i++)
                _scissorRects[i] = rects[i];

            // Reset any remaining scissor rectangles to whatever the first is.
            for (int i = rects.Length; i < _scissorRects.Length; i++)
                _scissorRects[i] = _scissorRects[0];

            _scissorRectsDirty = true;
        }

        public override void SetViewport(ViewportF vp, int slot)
        {
            _viewports[slot] = vp;
        }

        public override void SetViewports(ViewportF vp)
        {
            for (int i = 0; i < _viewports.Length; i++)
                _viewports[i] = vp;

            _viewportsDirty = true;
        }

        public override void SetViewports(ViewportF[] viewports)
        {
            if (viewports == null)
            {
                IRenderSurface2D surface = null;
                IRenderSurface2D surfaceZero = GetRenderSurface(0);

                for (uint i = 0; i < _viewports.Length; i++)
                {
                    surface = GetRenderSurface(i);
                    _viewports[i] = surface != null ? surface.Viewport : surfaceZero.Viewport;
                }
            }
            else
            {
                for (int i = 0; i < viewports.Length; i++)
                    _viewports[i] = viewports[i];

                // Set remaining unset ones to whatever the first is.
                for (int i = viewports.Length; i < _viewports.Length; i++)
                    _viewports[i] = _viewports[0];
            }

            _viewportsDirty = true;
        }

        public override void GetViewports(ViewportF[] outArray)
        {
            Array.Copy(_viewports, outArray, _viewports.Length);
        }

        public override ViewportF GetViewport(int index)
        {
            return _viewports[index];
        }

        /// <summary>Retrieves or creates a usable input layout for the provided vertex buffers and sub-effect.</summary>
        /// <returns>An instance of InputLayout.</returns>
        private VertexInputLayout GetInputLayout(MaterialPass pass)
        {
            // Retrieve layout list or create new one if needed.
            foreach (VertexInputLayout l in _cachedLayouts)
            {
                if (l.IsMatch(Device.Log, VertexBuffers))
                    return l;
            }

            ShaderComposition vs = pass[ShaderType.Vertex];
            VertexInputLayout input = new VertexInputLayout(DXDevice, VertexBuffers, (ID3D10Blob*)pass.InputByteCode, vs.InputStructure);
            _cachedLayouts.Add(input);

            return input;
        }

        private GraphicsBindResult Validate(QueueValidationMode mode)
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            result |= CheckMaterial();

            // Validate and update mode-specific data if needed.
            switch (mode)
            {
                case QueueValidationMode.Indexed:
                    result |= CheckVertexSegment();
                    result |= CheckIndexSegment();
                    break;

                case QueueValidationMode.Instanced:
                    result |= CheckVertexSegment();
                    result |= CheckInstancing();
                    break;

                case QueueValidationMode.InstancedIndexed:
                    result |= CheckVertexSegment();
                    result |= CheckIndexSegment();
                    result |= CheckInstancing();
                    break;
            }


            return result;
        }

        /// <summary>Validate vertex buffer and vertex shader.</summary>
        /// <param name="vbChanged">Has the vertex buffer changed.</param>
        /// <param name="veChanged">Has the vertex effect changed.</param>
        /// <returns></returns>
        private GraphicsBindResult CheckMaterial()
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            if (Material == null)
                result |= GraphicsBindResult.MissingMaterial;

            return result;
        }

        private GraphicsBindResult CheckVertexSegment()
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            if (VertexBuffers[0] == null)
                result |= GraphicsBindResult.MissingVertexSegment;

            return result;
        }

        private GraphicsBindResult CheckIndexSegment()
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            // If the index buffer is null, this method will always fail because 
            // it assumes it is only being called during an indexed draw call.
            if (IndexBuffer.BoundValue == null)
                result |= GraphicsBindResult.MissingIndexSegment;

            return result;
        }

        private GraphicsBindResult CheckInstancing()
        {
            if (_vertexLayout.BoundValue != null && _vertexLayout.BoundValue.IsInstanced)
                return GraphicsBindResult.Successful;
            else
                return GraphicsBindResult.NonInstancedVertexLayout;
        }

        /// <summary>Dispoes of the current <see cref="Graphics.CommandQueueDX11"/> instance.</summary>
        protected override void OnDispose()
        {
            EngineUtil.FreePtrArray(ref RTVs);
            SilkUtil.ReleasePtr(ref _debugAnnotation);

            // Dispose context.
            if (Type != GraphicsContextType.Immediate)
            {
                SilkUtil.ReleasePtr(ref _context);
                DXDevice.RemoveDeferredContext(this);
            }
        }

        /// <summary>Gets the current <see cref="CommandQueueDX11"/> type. This value will not change during the context's life.</summary>
        public GraphicsContextType Type { get; private set; }

        internal DeviceDX11 DXDevice { get; private set; }

        internal ID3D11DeviceContext4* Native => _context;
    }
}
