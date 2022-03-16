using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents the complete state of a <see cref="DeviceContext"/>.
    /// </summary>
    internal unsafe class DeviceContextState : EngineObject
    {
        Rectangle<int>[] _apiScissorRects;
        Rectangle[] _scissorRects;
        bool _scissorRectsDirty;

        Silk.NET.Direct3D11.Viewport[] _apiViewports;
        ViewportF[] _viewports;
        bool _viewportsDirty;
        ViewportF[] _nullViewport;

        List<ContextSlot> _slots;
        ContextSlot<ComputeTask> _compute;

        VertexTopology _boundTopology;
        ContextSlot<VertexInputLayout> _vertexLayout;
        List<VertexInputLayout> _cachedLayouts = new List<VertexInputLayout>();

        GraphicsDepthWritePermission _boundDepthMode = GraphicsDepthWritePermission.Enabled;

        internal ID3D11RenderTargetView** RTVs;
        uint _numRTVs;
        internal ID3D11DepthStencilView* DSV;

        internal DeviceContextState(DeviceContext context)
        {
            Context = context;
            _slots = new List<ContextSlot>();
            AllSlots = _slots.AsReadOnly();
            _nullViewport = new ViewportF[1];

            uint maxRTs = context.Device.Features.SimultaneousRenderSurfaces;
            _scissorRects = new Rectangle[maxRTs];
            _viewports = new ViewportF[maxRTs];
            _apiScissorRects = new Rectangle<int>[maxRTs];
            _apiViewports = new Silk.NET.Direct3D11.Viewport[maxRTs];

            uint maxVBuffers = Context.Device.Features.MaxVertexBufferSlots;
            VertexBuffers = RegisterSlotGroup<BufferSegment, VertexBufferGroupBinder>(ContextBindTypeFlags.Input, "V-Buffer", maxVBuffers);
            IndexBuffer = RegisterSlot<BufferSegment, IndexBufferBinder>(ContextBindTypeFlags.Input, "I-Buffer", 0);
            _vertexLayout = RegisterSlot<VertexInputLayout, InputLayoutBinder>(ContextBindTypeFlags.Input, "Vertex Input Layout", 0);
            Material = RegisterSlot<Material, MaterialBinder>(ContextBindTypeFlags.Input, "Material", 0);
            _compute = RegisterSlot<ComputeTask, ComputeTaskBinder>(ContextBindTypeFlags.Input, "Compute Task", 0);

            VS = new ShaderVSStage(this);
            GS = new ShaderGSStage(this);
            HS = new ShaderHSStage(this);
            DS = new ShaderDSStage(this);
            PS = new ShaderPSStage(this);
            CS = new ShaderCSStage(this);

            Blend = RegisterSlot<GraphicsBlendState, BlendBinder>(ContextBindTypeFlags.Output, "Blend State", 0);
            Depth = RegisterSlot<GraphicsDepthState, DepthStencilBinder>(ContextBindTypeFlags.Output, "Depth-Stencil State", 0);
            Rasterizer = RegisterSlot<GraphicsRasterizerState, RasterizerBinder>(ContextBindTypeFlags.Output, "Rasterizer State", 0);

            RTVs = EngineUtil.AllocPtrArray<ID3D11RenderTargetView>(maxRTs);

            Surfaces = RegisterSlotGroup<RenderSurface, SurfaceGroupBinder>(ContextBindTypeFlags.Output, "Render Surface", maxRTs);
            DepthSurface = RegisterSlot<DepthStencilSurface, DepthSurfaceBinder>(ContextBindTypeFlags.Output, "Depth Surface", 0);
        }

        internal void Clear()
        {
            Context.Native->ClearState();
        }

        internal bool Bind(MaterialPass pass, StateConditions conditions, VertexTopology topology)
        {
            bool matChanged = Material.Bind();

            // Check topology
            if (_boundTopology != topology)
            {
                _boundTopology = topology;
                Context.Native->IASetPrimitiveTopology(_boundTopology.ToApi());
            }

            VS.Shader.Value = pass.VertexShader;
            GS.Shader.Value = pass.GeometryShader;
            HS.Shader.Value = pass.HullShader;
            DS.Shader.Value = pass.DomainShader;
            PS.Shader.Value = pass.PixelShader;

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
                    Context.Native->IASetIndexBuffer(ib.Buffer, ib.DataFormat, ib.ByteOffset);
                else
                    Context.Native->IASetIndexBuffer(null, Format.FormatUnknown, 0);
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
                for (int i = 0; i < _scissorRects.Length; i++)
                    _apiScissorRects[i] = _scissorRects[i].ToApi();

                fixed (Rectangle<int>* ptrRect = _apiScissorRects)
                    Context.Native->RSSetScissorRects((uint)_apiScissorRects.Length, ptrRect);

                _scissorRectsDirty = false;
            }

            // Check if viewports need updating.
            if (_viewportsDirty)
            {
                for (int i = 0; i < _viewports.Length; i++)
                    _apiViewports[i] = _viewports[i].ToApi();

                Context.Native->RSSetViewports((uint)_viewports.Length, ref _apiViewports[0]);
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
                            _numRTVs = (i+1);
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

                Context.Native->OMSetRenderTargets(_numRTVs, RTVs, DSV);
                Context.Profiler.Current.SurfaceBindings++;
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

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        public void SetRenderSurfaces(params RenderSurface[] surfaces)
        {
            if (surfaces == null)
                SetRenderSurfaces(null, 0);
            else
                SetRenderSurfaces(surfaces, (uint)surfaces.Length);
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        /// <param name="count">The number of render surfaces to set.</param>
        public void SetRenderSurfaces(RenderSurface[] surfaces, uint count)
        {
            if (surfaces != null)
            {
                for (uint i = 0; i < count; i++)
                    Surfaces[i].Value = surfaces[i];
            }
            else
            {
                count = 0;
            }

            // Set the remaining surfaces to null.
            for (uint i = count; i < Surfaces.SlotCount; i++)
                Surfaces[i].Value = null;
        }

        /// <summary>Sets a render surface.</summary>
        /// <param name="surface">The surface to be set.</param>
        /// <param name="slot">The ID of the slot that the surface is to be bound to.</param>
        public void SetRenderSurface(RenderSurface surface, uint slot)
        {
            Surfaces[slot].Value = surface;
        }

        /// <summary>
        /// Fills the provided array with a list of applied render surfaces.
        /// </summary>
        /// <param name="destinationArray">The array to fill with applied render surfaces.</param>
        public void GetRenderSurfaces(RenderSurface[] destinationArray)
        {
            if (destinationArray.Length < Surfaces.SlotCount)
                throw new InvalidOperationException($"The destination array is too small ({destinationArray.Length}). A minimum size of {Surfaces.SlotCount} is needed.");
            for (uint i = 0; i < Surfaces.SlotCount; i++)
                destinationArray[i] = Surfaces[i].Value;
        }

        /// <summary>Returns the render surface that is bound to the requested slot ID. Returns null if the slot is empty.</summary>
        /// <param name="slot">The ID of the slot to retrieve a surface from.</param>
        /// <returns></returns>
        public RenderSurface GetRenderSurface(uint slot)
        {
            return Surfaces[slot].Value;
        }

        /// <summary>
        /// Resets the render surfaces.
        /// </summary>
        /// <param name="resetMode">The reset mode.</param>
        /// <param name="outputOnFirst">If true and the reset mode is OutputSurface, it will only be applied to the first slot (0)..</param>
        public void ResetRenderSurfaces()
        {
            for (uint i = 0; i < Surfaces.SlotCount; i++)
                Surfaces[i].Value = null;
        }

        public void SetScissorRectangle(Rectangle rect, int slot = 0)
        {
            _scissorRects[slot] = rect;
            _scissorRectsDirty = true;
        }

        public void SetScissorRectangles(params Rectangle[] rects)
        {
            for (int i = 0; i < rects.Length; i++)
                _scissorRects[i] = rects[i];

            // Reset any remaining scissor rectangles to whatever the first is.
            for (int i = rects.Length; i < _scissorRects.Length; i++)
                _scissorRects[i] = _scissorRects[0];

            _scissorRectsDirty = true;
        }

        /// <summary>
        /// Applies the provided viewport value to the specified viewport slot.
        /// </summary>
        /// <param name="vp">The viewport value.</param>
        public void SetViewport(ViewportF vp, int slot)
        {
            _viewports[slot] = vp;
        }

        /// <summary>
        /// Applies the specified viewport to all viewport slots.
        /// </summary>
        /// <param name="vp">The viewport value.</param>
        public void SetViewports(ViewportF vp)
        {
            for (int i = 0; i < _viewports.Length; i++)
                _viewports[i] = vp;

            _viewportsDirty = true;
        }

        /// <summary>
        /// Sets the provided viewports on to their respective viewport slots. <para/>
        /// If less than the total number of viewport slots was provided, the remaining ones will be set to whatever the same value as the first viewport slot.
        /// </summary>
        /// <param name="viewports"></param>
        public void SetViewports(ViewportF[] viewports)
        {
            if (viewports == null)
            {
                RenderSurface surface = null;
                RenderSurface surfaceZero = GetRenderSurface(0);

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

        public void GetViewports(ViewportF[] outArray)
        {
            Array.Copy(_viewports, outArray, _viewports.Length);
        }

        public ViewportF GetViewport(int index)
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
                if (l.IsMatch(Context.Log, VertexBuffers))
                    return l;
            }

            Material mat = Material.BoundValue;
            VertexInputLayout input = new VertexInputLayout(Context.Device, VertexBuffers, mat.InputStructureByteCode, mat.InputStructure);
            _cachedLayouts.Add(input);

            return input;
        }

        internal GraphicsBindResult Validate(GraphicsValidationMode mode)
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

        internal ContextSlot<T> RegisterSlot<T, B>(ContextBindTypeFlags bindType, string namePrefix, uint slotIndex) 
            where T : ContextBindable
            where B : ContextSlotBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlot(bindType, namePrefix, slotIndex, binder);
        }

        internal ContextSlot<T> RegisterSlot<T>(ContextBindTypeFlags bindType, string namePrefix, uint slotIndex, ContextSlotBinder<T> binder)
            where T : ContextBindable
        {
            ContextSlot<T> slot = new ContextSlot<T>(this, binder, bindType, namePrefix, slotIndex);
            _slots.Add(slot);
            return slot;
        }

        internal ContextSlotGroup<T> RegisterSlotGroup<T, B>(ContextBindTypeFlags bindType, string namePrefix, uint numSlots)
            where T : ContextBindable
            where B : ContextGroupBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlotGroup(bindType, namePrefix, numSlots, binder);
        }

        internal ContextSlotGroup<T> RegisterSlotGroup<T>(ContextBindTypeFlags bindType, string namePrefix, uint numSlots, ContextGroupBinder<T> binder)
            where T : ContextBindable
        {
            ContextSlot<T>[] slots = new ContextSlot<T>[numSlots];
            ContextSlotGroup<T> grp = new ContextSlotGroup<T>(this, binder, slots, bindType, namePrefix);

            for (uint i = 0; i < numSlots; i++)
                slots[i] = new ContextSlot<T>(this, grp, bindType, namePrefix, i);

            _slots.AddRange(slots);

            return grp;
        }

        protected override void OnDispose()
        {
            EngineUtil.FreePtrArray(ref RTVs);
        }

        internal DeviceContext Context { get; }

        internal IReadOnlyList<ContextSlot> AllSlots { get; }

        internal ShaderVSStage VS { get; }
        internal ShaderGSStage GS { get; }
        internal ShaderHSStage HS { get; }
        internal ShaderDSStage DS { get; }
        internal ShaderPSStage PS { get; }
        internal ShaderCSStage CS { get; }

        public ContextSlotGroup<BufferSegment> VertexBuffers { get; }

        public ContextSlot<BufferSegment> IndexBuffer { get; }

        public ContextSlot<Material> Material { get; }

        internal ContextSlot<GraphicsBlendState> Blend { get; }

        internal ContextSlot<GraphicsRasterizerState> Rasterizer { get; }

        internal ContextSlot<GraphicsDepthState> Depth { get; }

        /// <summary>Gets the number of applied viewports.</summary>
        public int ViewportCount => _viewports.Length;

        internal GraphicsDepthWritePermission DepthWriteOverride { get; set; } = GraphicsDepthWritePermission.Enabled;

        public ContextSlotGroup<RenderSurface> Surfaces { get; }


        /// <summary>
        /// Gets or sets the output depth surface.
        /// </summary>
        public ContextSlot<DepthStencilSurface> DepthSurface { get; }
    }
}
