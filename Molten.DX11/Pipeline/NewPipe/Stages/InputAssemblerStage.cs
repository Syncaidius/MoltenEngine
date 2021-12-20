using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class InputAssemblerStage : PipeStage
    {
        VertexTopology _boundTopology;
        PipeSlot<VertexInputLayout> _vertexLayout;
        List<VertexInputLayout> _cachedLayouts = new List<VertexInputLayout>();

        ShaderVertexStage _vs;
        ShaderGeometryStage _gs;
        ShaderHullStage _hs;
        ShaderDomainStage _ds;
        ShaderPixelStage _ps;

        public InputAssemblerStage(PipeDX11 pipe, PipeStageType stageType) : 
            base(pipe, stageType)
        {
            _vs = new ShaderVertexStage(pipe);
            _gs = new ShaderGeometryStage(pipe);
            _hs = new ShaderHullStage(pipe);
            _ds = new ShaderDomainStage(pipe);
            _ps = new ShaderPixelStage(pipe);

            uint maxVBuffers = pipe.Device.Features.MaxVertexBufferSlots;
            VertexBuffers = DefineSlotGroup<BufferSegment>(maxVBuffers, PipeBindTypeFlags.Input, "V-Buffer");
            IndexBuffer = DefineSlot<BufferSegment>(0, PipeBindTypeFlags.Input, "I-Buffer");
            _vertexLayout = DefineSlot<VertexInputLayout>(0, PipeBindTypeFlags.Input, "Vertex Input Layout");
            Material = DefineSlot<Material>(0, PipeBindTypeFlags.Input, "Material");
        }

        internal bool Bind(MaterialPass pass, StateConditions conditions, VertexTopology topology)
        {
            // Check topology
            if (_boundTopology != topology)
            {
                _boundTopology = topology;
                Pipe.Context->IASetPrimitiveTopology(_boundTopology.ToApi());
            }

            _vs.Shader.Value = pass.VertexShader;
            _gs.Shader.Value = pass.GeometryShader;
            _hs.Shader.Value = pass.HullShader;
            _ds.Shader.Value = pass.DomainShader;
            _ps.Shader.Value = pass.PixelShader;

            bool vsChanged = _vs.Bind();
            bool gsChanged = false;
            bool hsChanged = false;
            bool dsChanged = false;
            bool psChanged = false;

            bool ibChanged = IndexBuffer.Bind();
            bool vbChanged = VertexBuffers.BindAll();

            // Check index buffer
            if (ibChanged)
            {
                BufferSegment ib = IndexBuffer.BoundValue;
                if (ib != null)
                    Pipe.Context->IASetIndexBuffer(ib.Buffer, ib.DataFormat, ib.ByteOffset);
                else
                    Pipe.Context->IASetIndexBuffer(null, Format.FormatUnknown, 0);
            }

            // Does the vertex input layout need updating?
            if (vbChanged|| vsChanged)
            {
                BindVertexBuffers(VertexBuffers);
                _vertexLayout.Value = GetInputLayout();
                _vertexLayout.Bind();
                Pipe.Context->IASetInputLayout(_vertexLayout.BoundValue);
            }

            return vsChanged || gsChanged || hsChanged || 
                dsChanged || psChanged || ibChanged || vbChanged;
        }

        private void BindVertexBuffers(PipeSlotGroup<BufferSegment> grp)
        {
            int iNumChanged = (int)grp.NumSlotsChanged;

            ID3D11Buffer** pBuffers = stackalloc ID3D11Buffer*[iNumChanged];
            uint* pStrides = stackalloc uint[iNumChanged];
            uint* pOffsets = stackalloc uint[iNumChanged];
            uint p = 0;
            BufferSegment seg = null;
            
            for (uint i = grp.FirstChanged; i <= grp.LastChanged; i++)
            {
                seg = grp[i].BoundValue;

                if (seg != null)
                {
                    pBuffers[p] = seg.Buffer.ResourcePtr;
                    pStrides[p] = seg.Stride;
                    pOffsets[p] = seg.ByteOffset;
                }
                else
                {
                    pBuffers[p] = null;
                    pStrides[p] = 0;
                    pOffsets[p] = 0;
                }

                p++;
            }

            Pipe.Context->IASetVertexBuffers(grp.FirstChanged, grp.NumSlotsChanged, pBuffers, pStrides, pOffsets);
        }


        /// <summary>Retrieves or creates a usable input layout for the provided vertex buffers and sub-effect.</summary>
        /// <returns>An instance of InputLayout.</returns>
        private VertexInputLayout GetInputLayout()
        {
            // Retrieve layout list or create new one if needed.
            foreach (VertexInputLayout l in _cachedLayouts)
            {
                if (l.IsMatch(Device.Log, VertexBuffers))
                    return l;
            }

            VertexInputLayout input = new VertexInputLayout(Device, VertexBuffers,
                Material.BoundValue.InputStructureByteCode,
                Material.BoundValue.InputStructure);
            _cachedLayouts.Add(input);

            return input;
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            _vs.Dispose();
            _gs.Dispose();
            _hs.Dispose();
            _ds.Dispose();
            _ps.Dispose();
        }

        public PipeSlotGroup<BufferSegment> VertexBuffers { get; }

        public PipeSlot<BufferSegment> IndexBuffer { get;}

        public PipeSlot<Material> Material { get; }
    }
}
