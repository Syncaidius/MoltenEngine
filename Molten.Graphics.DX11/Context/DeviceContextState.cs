using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
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
        List<ContextSlot> _slots;
        VertexTopology _boundTopology;
        ContextSlot<VertexInputLayout> _vertexLayout;
        List<VertexInputLayout> _cachedLayouts = new List<VertexInputLayout>();

        internal DeviceContextState(DeviceContext context)
        {
            Context = context;
            _slots = new List<ContextSlot>();
            AllSlots = _slots.AsReadOnly();

            uint maxVBuffers = Context.Device.Features.MaxVertexBufferSlots;
            VertexBuffers = RegisterSlotGroup<BufferSegment, VertexBufferGroupBinder>(PipeBindTypeFlags.Input, "V-Buffer", maxVBuffers);
            IndexBuffer = RegisterSlot<BufferSegment, IndexBufferBinder>(PipeBindTypeFlags.Input, "I-Buffer", 0);
            _vertexLayout = RegisterSlot<VertexInputLayout, InputLayoutBinder>(PipeBindTypeFlags.Input, "Vertex Input Layout", 0);
            Material = RegisterSlot<Material, MaterialBinder>(PipeBindTypeFlags.Input, "Material", 0);

            VS = new ShaderVSStage(this);
            GS = new ShaderGSStage(this);
            HS = new ShaderHSStage(this);
            DS = new ShaderDSStage(this);
            PS = new ShaderPSStage(this);
            CS = new ShaderCSStage(this);
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

            return matChanged || vsChanged || gsChanged || hsChanged ||
                dsChanged || psChanged || ibChanged || vbChanged;
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

        internal ContextShaderStage<ID3D11VertexShader> VS { get; }
        internal ContextShaderStage<ID3D11GeometryShader> GS { get; }
        internal ContextShaderStage<ID3D11HullShader> HS { get; }
        internal ContextShaderStage<ID3D11DomainShader> DS { get; }
        internal ContextShaderStage<ID3D11PixelShader> PS { get; }
        internal ContextShaderStage<ID3D11ComputeShader> CS { get; }

        public ContextSlotGroup<BufferSegment> VertexBuffers { get; }

        public ContextSlot<BufferSegment> IndexBuffer { get; }

        public ContextSlot<Material> Material { get; }
    }
}
