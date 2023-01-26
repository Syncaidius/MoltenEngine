using Microsoft.VisualBasic.Devices;
using Molten.IO;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace Molten.Graphics
{
    internal delegate void ContextDrawCallback(MaterialPass pass);
    internal delegate void ContextDrawFailCallback(MaterialPass pass, uint iteration, uint passNumber, GraphicsBindResult result);

    /// <summary>Manages the pipeline of a either an immediate or deferred <see cref="CommandQueueDX11"/>.</summary>
    public unsafe partial class CommandQueueDX11 : GraphicsCommandQueue
    {
        ID3D11DeviceContext4* _context;
        ContextStateStack _stateStack;

        Rectangle[] _scissorRects;
        bool _scissorRectsDirty;

        Silk.NET.Direct3D11.Viewport[] _apiViewports;
        ViewportF[] _viewports;
        bool _viewportsDirty;
        ViewportF[] _nullViewport;

        List<GraphicsSlot> _slots;
        GraphicsSlot<ComputeTask> _compute;

        VertexTopology _boundTopology;
        GraphicsSlot<VertexInputLayout> _vertexLayout;
        List<VertexInputLayout> _cachedLayouts = new List<VertexInputLayout>();

        GraphicsDepthWritePermission _boundDepthMode = GraphicsDepthWritePermission.Enabled;

        internal ID3D11RenderTargetView1** RTVs;
        uint _numRTVs;
        internal ID3D11DepthStencilView* DSV;

        internal CommandQueueDX11(DeviceDX11 device, ID3D11DeviceContext4* context) :
            base(device)
        {
            _context = context;
            DXDevice = device;

            if (_context->GetType() == DeviceContextType.Immediate)
                Type = GraphicsContextType.Immediate;
            else
                Type = GraphicsContextType.Deferred;

            _slots = new List<GraphicsSlot>();
            _nullViewport = new ViewportF[1];

            uint maxRTs = DXDevice.Adapter.Capabilities.PixelShader.MaxOutResources;
            _scissorRects = new Rectangle[maxRTs];
            _viewports = new ViewportF[maxRTs];
            _apiViewports = new Silk.NET.Direct3D11.Viewport[maxRTs];

            uint maxVBuffers = DXDevice.Adapter.Capabilities.VertexBuffers.MaxSlots;
            VertexBuffers = RegisterSlotGroup<BufferSegment, VertexBufferGroupBinder>(GraphicsBindTypeFlags.Input, "V-Buffer", maxVBuffers);
            IndexBuffer = RegisterSlot<BufferSegment, IndexBufferBinder>(GraphicsBindTypeFlags.Input, "I-Buffer", 0);
            _vertexLayout = RegisterSlot<VertexInputLayout, InputLayoutBinder>(GraphicsBindTypeFlags.Input, "Vertex Input Layout", 0);
            Material = RegisterSlot<Material, MaterialBinder>(GraphicsBindTypeFlags.Input, "Material", 0);
            _compute = RegisterSlot<ComputeTask, ComputeTaskBinder>(GraphicsBindTypeFlags.Input, "Compute Task", 0);

            VS = new ShaderVSStage(this);
            GS = new ShaderGSStage(this);
            HS = new ShaderHSStage(this);
            DS = new ShaderDSStage(this);
            PS = new ShaderPSStage(this);
            CS = new ShaderCSStage(this);

            Blend = RegisterSlot<BlendStateDX11, BlendBinder>(GraphicsBindTypeFlags.Output, "Blend State", 0);
            Depth = RegisterSlot<DepthStateDX11, DepthStencilBinder>(GraphicsBindTypeFlags.Output, "Depth-Stencil State", 0);
            Rasterizer = RegisterSlot<RasterizerStateDX11, RasterizerBinder>(GraphicsBindTypeFlags.Output, "Rasterizer State", 0);

            RTVs = EngineUtil.AllocPtrArray<ID3D11RenderTargetView1>(maxRTs);

            Surfaces = RegisterSlotGroup<RenderSurface2D, SurfaceGroupBinder>(GraphicsBindTypeFlags.Output, "Render Surface", maxRTs);
            DepthSurface = RegisterSlot<DepthStencilSurface, DepthSurfaceBinder>(GraphicsBindTypeFlags.Output, "Depth Surface", 0);

            _stateStack = new ContextStateStack(this);

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

        public int PushState()
        {
            return _stateStack.Push();
        }

        public void PopState()
        {
            _stateStack.Pop();
        }

        private GraphicsBindResult ApplyState(MaterialPass pass,
            GraphicsValidationMode mode,
            VertexTopology topology)
        {
            if (topology == VertexTopology.Undefined)
                return GraphicsBindResult.UndefinedTopology;

            Bind(pass, DrawInfo.Conditions, topology);

            // Validate all pipeline components.
            GraphicsBindResult result = Validate(mode);

            return result;
        }

        private GraphicsBindResult DrawCommon(Material mat, GraphicsValidationMode mode, VertexTopology topology, 
            ContextDrawCallback drawCallback, ContextDrawFailCallback failCallback)
        {
            GraphicsBindResult vResult = GraphicsBindResult.Successful;

            if (!DrawInfo.Began)
                throw new GraphicsCommandQueueException(this, $"{nameof(CommandQueueDX11)}: BeginDraw() must be called before calling {nameof(Draw)}()");

            Material.Value = mat;

            // Re-render the same material for mat.Iterations.
            for (uint i = 0; i < mat.Iterations; i++)
            {
                for (uint j = 0; j < mat.PassCount; j++)
                {
                    MaterialPass pass = mat.Passes[j];
                    vResult = ApplyState(pass, mode, topology);

                    if (vResult == GraphicsBindResult.Successful)
                    {
                        // Re-render the same pass for K iterations.
                        for (int k = 0; k < pass.Iterations; k++)
                        {
                            drawCallback(pass);
                            Profiler.Current.DrawCalls++;
                        }
                    }
                    else
                    {
                        failCallback(pass, i, j, vResult);
                        break;
                    }
                }
            }

            return vResult;
        }

        public override GraphicsBindResult Draw(IMaterial material, uint vertexCount, VertexTopology topology, uint vertexStartIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.Unindexed, topology, (pass) =>
            {
                _context->Draw(vertexCount, vertexStartIndex);
            },
            (pass, iteration, passNumber, vResult) =>
            {
                DXDevice.Log.Warning($"Draw() call failed with result: {vResult} -- " + 
                    $"Iteration: M{iteration}/{material.Iterations}P{passNumber}/{material.PassCount} -- " +
                    $"Material: {material.Name} -- Topology: {topology} -- VertexCount: { vertexCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawInstanced(IMaterial material,
            uint vertexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.Instanced, topology, (pass) =>
            {
                _context->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex);
            },
            (pass, iteration, passNum, vResult) =>
            {
                DXDevice.Log.Warning($"DrawInstanced() call failed with result: {vResult} -- " + 
                        $"Iteration: M{iteration}/{material.Iterations}P{passNum}/{material.PassCount} -- Material: {material.Name} -- " +
                        $"Topology: {topology} -- VertexCount: { vertexCountPerInstance} -- Instances: {instanceCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexed(IMaterial material,
            uint indexCount,
            VertexTopology topology,
            int vertexIndexOffset = 0,
            uint startIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.Indexed, topology, (pass) =>
            {
                _context->DrawIndexed(indexCount, startIndex, vertexIndexOffset);
            },
            (pass, it, passNum, vResult) =>
            {
                DXDevice.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Iteration: M{it}/{material.Iterations}P{passNum}/{material.PassCount}" +
                    $" -- Material: {material.Name} -- Topology: {topology} -- indexCount: { indexCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexedInstanced(IMaterial material,
            uint indexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.InstancedIndexed, topology, (pass) =>
            {
                _context->DrawIndexedInstanced(indexCountPerInstance, instanceCount,
                    startIndex, vertexIndexOffset, instanceStartIndex);
            },
            (pass, it, passNum, vResult) =>
            {
                DXDevice.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Iteration: M{it}/{material.Iterations}P{passNum}/{material.PassCount}" +
                    $" -- Material: {material.Name} -- Topology: {topology} -- Indices-per-instance: { indexCountPerInstance}");
            });
        }

        /// <inheritdoc/>
        public override void Dispatch(IComputeTask task, uint groupsX, uint groupsY, uint groupsZ)
        {
            bool csChanged = Bind(task as ComputeTask);

            if (CS.Shader.BoundValue == null)
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

        internal bool Bind(MaterialPass pass, StateConditions conditions, VertexTopology topology)
        {
            bool matChanged = Material.Bind();

            // Check topology
            if (_boundTopology != topology)
            {
                _boundTopology = topology;
                Native->IASetPrimitiveTopology(_boundTopology.ToApi());
            }

            VS.Shader.Value = pass.VS;
            GS.Shader.Value = pass.GS;
            HS.Shader.Value = pass.HS;
            DS.Shader.Value = pass.DS;
            PS.Shader.Value = pass.PS;

            bool vsChanged = VS.Bind();
            bool gsChanged = GS.Bind();
            bool hsChanged = HS.Bind();
            bool dsChanged = DS.Bind();
            bool psChanged = PS.Bind();

            bool ibChanged = IndexBuffer.Bind();
            bool vbChanged = VertexBuffers.BindAll();

            // Check index buffer
            if (ibChanged)
            {
                BufferSegment ib = IndexBuffer.BoundValue;
                if (ib != null)
                    Native->IASetIndexBuffer(ib.Buffer, ib.DataFormat, ib.ByteOffset);
                else
                    Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
            }

            // Does the vertex input layout need updating?
            if (vbChanged || vsChanged)
            {
                _vertexLayout.Value = GetInputLayout();
                _vertexLayout.Bind();
            }

            Blend.Value = pass.BlendState[conditions];
            Rasterizer.Value = pass.RasterizerState[conditions];
            Depth.Value = pass.DepthState[conditions];

            bool bStateChanged = Blend.Bind();
            bool rStateChanged = Rasterizer.Bind();
            bool dStateChanged = Depth.Bind();

            // Check if scissor rects need updating
            if (_scissorRectsDirty)
            {
                fixed (Rectangle* ptrRect = _scissorRects)
                    Native->RSSetScissorRects((uint)_scissorRects.Length, (Rectangle<int>*)ptrRect);

                _scissorRectsDirty = false;
            }

            // Check if viewports need updating.
            if (_viewportsDirty)
            {
                for (int i = 0; i < _viewports.Length; i++)
                    _apiViewports[i] = _viewports[i].ToApi();

                Native->RSSetViewports((uint)_viewports.Length, ref _apiViewports[0]);
                _viewportsDirty = false;
            }

            GraphicsDepthWritePermission depthWriteMode = DepthWriteOverride != GraphicsDepthWritePermission.Enabled ?
                DepthWriteOverride : pass.DepthState[conditions].WritePermission;

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
                            RTVs[i] = Surfaces[i].BoundValue.RTV.Ptr;
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
                        if (depthWriteMode == GraphicsDepthWritePermission.ReadOnly)
                            DSV = DepthSurface.BoundValue.ReadOnlyView;
                        else
                            DSV = DepthSurface.BoundValue.DepthView;
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

            return bStateChanged || rStateChanged || dStateChanged ||
                matChanged || vsChanged || gsChanged || hsChanged ||
                dsChanged || psChanged || ibChanged || vbChanged;
        }

        public bool Bind(ComputeTask task)
        {
            _compute.Value = task;
            _compute.Bind();
            CS.Shader.Value = _compute.BoundValue.Composition;

            bool csChanged = CS.Bind();

            if (CS.Shader.BoundValue != null)
            {
                // Apply unordered acces views to slots
                for (int i = 0; i < CS.Shader.BoundValue.UnorderedAccessIds.Count; i++)
                {
                    uint slotID = CS.Shader.BoundValue.UnorderedAccessIds[i];
                    CS.UAVs[slotID].Value = _compute.BoundValue.UAVs[slotID]?.UnorderedResource;
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
        private VertexInputLayout GetInputLayout()
        {
            // Retrieve layout list or create new one if needed.
            foreach (VertexInputLayout l in _cachedLayouts)
            {
                if (l.IsMatch(DXDevice.Log, VertexBuffers))
                    return l;
            }

            Material mat = Material.BoundValue;
            VertexInputLayout input = new VertexInputLayout(DXDevice, VertexBuffers, (ID3D10Blob*)mat.InputStructureByteCode, mat.InputStructure);
            _cachedLayouts.Add(input);

            return input;
        }

        private GraphicsBindResult Validate(GraphicsValidationMode mode)
        {
            GraphicsBindResult result = GraphicsBindResult.Successful;

            result |= CheckMaterial();

            // Validate and update mode-specific data if needed.
            switch (mode)
            {
                case GraphicsValidationMode.Indexed:
                    result |= CheckVertexSegment();
                    result |= CheckIndexSegment();
                    break;

                case GraphicsValidationMode.Instanced:
                    result |= CheckVertexSegment();
                    result |= CheckInstancing();
                    break;

                case GraphicsValidationMode.InstancedIndexed:
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

        public GraphicsSlot<T> RegisterSlot<T, B>(GraphicsBindTypeFlags bindType, string namePrefix, uint slotIndex)
    where T : GraphicsObject
    where B : GraphicsSlotBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlot(bindType, namePrefix, slotIndex, binder);
        }

        public GraphicsSlot<T> RegisterSlot<T>(GraphicsBindTypeFlags bindType, string namePrefix, uint slotIndex, GraphicsSlotBinder<T> binder)
            where T : GraphicsObject
        {
            GraphicsSlot<T> slot = new GraphicsSlot<T>(this, binder, bindType, namePrefix, slotIndex);
            _slots.Add(slot);
            return slot;
        }

        public GraphicsSlotGroup<T> RegisterSlotGroup<T, B>(GraphicsBindTypeFlags bindType, string namePrefix, uint numSlots)
            where T : GraphicsObject
            where B : GraphicsGroupBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlotGroup(bindType, namePrefix, numSlots, binder);
        }

        internal GraphicsSlotGroup<T> RegisterSlotGroup<T>(GraphicsBindTypeFlags bindType, string namePrefix, uint numSlots, GraphicsGroupBinder<T> binder)
            where T : GraphicsObject
        {
            GraphicsSlot<T>[] slots = new GraphicsSlot<T>[numSlots];
            GraphicsSlotGroup<T> grp = new GraphicsSlotGroup<T>(this, binder, slots, bindType, namePrefix);

            for (uint i = 0; i < numSlots; i++)
                slots[i] = new GraphicsSlot<T>(this, grp, bindType, namePrefix, i);

            _slots.AddRange(slots);

            return grp;
        }

        /// <summary>Dispoes of the current <see cref="Graphics.CommandQueueDX11"/> instance.</summary>
        protected override void OnDispose()
        {
            EngineUtil.FreePtrArray(ref RTVs);

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

        internal ShaderVSStage VS { get; }
        internal ShaderGSStage GS { get; }
        internal ShaderHSStage HS { get; }
        internal ShaderDSStage DS { get; }
        internal ShaderPSStage PS { get; }
        internal ShaderCSStage CS { get; }

        public GraphicsSlotGroup<BufferSegment> VertexBuffers { get; }

        public GraphicsSlot<BufferSegment> IndexBuffer { get; }

        public GraphicsSlot<Material> Material { get; }

        internal GraphicsSlot<BlendStateDX11> Blend { get; }

        internal GraphicsSlot<RasterizerStateDX11> Rasterizer { get; }

        internal GraphicsSlot<DepthStateDX11> Depth { get; }

        /// <summary>Gets the number of applied viewports.</summary>
        public int ViewportCount => _viewports.Length;

        public GraphicsSlotGroup<RenderSurface2D> Surfaces { get; }

        /// <summary>
        /// Gets or sets the output depth surface.
        /// </summary>
        public GraphicsSlot<DepthStencilSurface> DepthSurface { get; }
    }
}
