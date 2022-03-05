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
        VertexTopology _boundTopology;
        ContextSlot<VertexInputLayout> _vertexLayout;
        List<VertexInputLayout> _cachedLayouts = new List<VertexInputLayout>();

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
            VertexBuffers = RegisterSlotGroup<BufferSegment, VertexBufferGroupBinder>(PipeBindTypeFlags.Input, "V-Buffer", maxVBuffers);
            IndexBuffer = RegisterSlot<BufferSegment, IndexBufferBinder>(PipeBindTypeFlags.Input, "I-Buffer", 0);
            _vertexLayout = RegisterSlot<VertexInputLayout, InputLayoutBinder>(PipeBindTypeFlags.Input, "Vertex Input Layout", 0);
            Material = RegisterSlot<Material, MaterialBinder>(PipeBindTypeFlags.Input, "Material", 0);
            Compute = RegisterSlot<ComputeTask, ComputeTaskBinder>(PipeBindTypeFlags.Input, "Compute Task", 0);

            VS = new ShaderVSStage(this);
            GS = new ShaderGSStage(this);
            HS = new ShaderHSStage(this);
            DS = new ShaderDSStage(this);
            PS = new ShaderPSStage(this);
            CS = new ShaderCSStage(this);

            BlendState = RegisterSlot<GraphicsBlendState, BlendBinder>(PipeBindTypeFlags.Output, "Blend State", 0);
            DepthState = RegisterSlot<GraphicsDepthState, DepthStencilBinder>(PipeBindTypeFlags.Output, "Depth-Stencil State", 0);
            RasterizerState = RegisterSlot<GraphicsRasterizerState, RasterizerBinder>(PipeBindTypeFlags.Output, "Rasterizer State", 0);
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

            BlendState.Value = pass.BlendState[conditions];
            RasterizerState.Value = pass.RasterizerState[conditions];
            DepthState.Value = pass.DepthState[conditions];

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

            return matChanged || vsChanged || gsChanged || hsChanged ||
                dsChanged || psChanged || ibChanged || vbChanged;
        }

        public bool BindCompute()
        {
            Compute.Bind();
            CS.Shader.Value = Compute.BoundValue.Composition;

            bool csChanged = CS.Bind();

            if (CS.Shader.BoundValue != null)
            {
                // Apply unordered acces views to slots
                for (int i = 0; i < CS.Shader.BoundValue.UnorderedAccessIds.Count; i++)
                {
                    uint slotID = CS.Shader.BoundValue.UnorderedAccessIds[i];
                    CS.UAVs[slotID].Value = Compute.BoundValue.UAVs[slotID]?.UnorderedResource;
                }
            }

            return csChanged;
        }


        public void SetScissorRectangle(Rectangle rect, int slot = 0)
        {
            _scissorRects[slot] = rect;
            _scissorRectsDirty = true;
        }

        public void SetScissorRectangles(Rectangle[] rects)
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
                RenderSurface surfaceZero = Context.Output.GetRenderSurface(0);

                for (uint i = 0; i < _viewports.Length; i++)
                {
                    surface = Context.Output.GetRenderSurface(i);
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

        internal ContextSlot<T> RegisterSlot<T, B>(PipeBindTypeFlags bindType, string namePrefix, uint slotIndex) 
            where T : PipeBindable
            where B : ContextSlotBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlot(bindType, namePrefix, slotIndex, binder);
        }

        internal ContextSlot<T> RegisterSlot<T>(PipeBindTypeFlags bindType, string namePrefix, uint slotIndex, ContextSlotBinder<T> binder)
            where T : PipeBindable
        {
            ContextSlot<T> slot = new ContextSlot<T>(this, binder, bindType, namePrefix, slotIndex);
            _slots.Add(slot);
            return slot;
        }

        internal ContextSlotGroup<T> RegisterSlotGroup<T, B>(PipeBindTypeFlags bindType, string namePrefix, uint numSlots)
            where T : PipeBindable
            where B : ContextGroupBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlotGroup(bindType, namePrefix, numSlots, binder);
        }

        internal ContextSlotGroup<T> RegisterSlotGroup<T>(PipeBindTypeFlags bindType, string namePrefix, uint numSlots, ContextGroupBinder<T> binder)
            where T : PipeBindable
        {
            ContextSlot<T>[] slots = new ContextSlot<T>[numSlots];
            ContextSlotGroup<T> grp = new ContextSlotGroup<T>(this, binder, slots, bindType, namePrefix);

            for (uint i = 0; i < numSlots; i++)
                slots[i] = new ContextSlot<T>(this, grp, bindType, namePrefix, i);

            _slots.AddRange(slots);

            return grp;
        }

        protected override void OnDispose() { }

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

        public ContextSlot<ComputeTask> Compute { get; }

        internal ContextSlot<GraphicsBlendState> BlendState { get; }

        internal ContextSlot<GraphicsRasterizerState> RasterizerState { get; }

        internal ContextSlot<GraphicsDepthState> DepthState { get; }

        /// <summary>Gets the number of applied viewports.</summary>
        public int ViewportCount => _viewports.Length;
    }
}
