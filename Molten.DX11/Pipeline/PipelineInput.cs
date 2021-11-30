using Molten.Graphics.Shaders;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class PipelineInput : PipelineComponent<DeviceDX11, PipeDX11>
    {
        MaterialInputStage _materialStage;

        VertexBufferBinder _vbBinder;
        PipelineBindSlot<BufferSegment, DeviceDX11, PipeDX11> _slotIndexBuffer;

        BufferSegment _indexSegment;
        VertexInputLayout _vertexLayout;

        VertexTopology _prevTopology = VertexTopology.Undefined;
        List<VertexInputLayout> _cachedLayouts = new List<VertexInputLayout>();

        public PipelineInput(PipeDX11 pipe) : base(pipe)
        {
            _materialStage = new MaterialInputStage(pipe);
            _vbBinder = new VertexBufferBinder(pipe);

            int iSlots = Device.Features.MaxIndexBufferSlots;
            _slotIndexBuffer = new PipelineBindSlot<BufferSegment, DeviceDX11, PipeDX11>(this, 0);
            _slotIndexBuffer.OnObjectForcedUnbind += _slotIndexBuffer_OnBoundObjectDisposed;
        }

        private void _slotIndexBuffer_OnBoundObjectDisposed(PipelineBindSlot<DeviceDX11, PipeDX11> slot, PipelineDisposableObject obj)
        {
            Pipe.Context->IASetIndexBuffer(null, Format.FormatUnknown, 0);
        }

        internal unsafe void Refresh(MaterialPass pass, StateConditions conditions, VertexTopology topology)
        {
            // Update shader pipeline stages
            _materialStage.Refresh(pass, conditions);

            // Update vertex buffers
            bool anyVbChanged = false;
            BufferSegment vb = null;
            for (int i = 0; i < _vertexSlotCount; i++)
            {
                vb = _vertexSegments[i];
                bool vbChanged = _slotVertexBuffers[i].Bind(Pipe, vb, PipelineBindType.Input);

                // Check for change
                if (vbChanged)
                {
                    anyVbChanged = true;

                    if (vb == null)
                    {
                        _vertexBindings[i] = _nullVertexBuffer;
                        _inputAssembler.SetVertexBuffers(i, _nullVertexBuffer);
                    }
                    else
                    {
                        _vertexBindings[i] = vb.Binding;
                        _inputAssembler.SetVertexBuffers(i, vb.Binding);
                    }
                }
                else if(vb != null && _vertexBindings[i].Offset != vb.Binding.Offset)
                {
                    _vertexBindings[i] = vb.Binding;
                    _inputAssembler.SetVertexBuffers(i, vb.Binding);
                }
            }

            if (_prevTopology != topology)
            {
                Pipe.Context->IASetPrimitiveTopology((D3DPrimitiveTopology)topology);
                _prevTopology = topology;
            }

            // Update index buffers
            bool ibChanged = _slotIndexBuffer.Bind(Pipe, _indexSegment, PipelineBindType.Input);
            if (ibChanged)
            {
                if (_indexSegment != null)
                {
                    _inputAssembler.SetIndexBuffer(
                        _indexSegment.Buffer,
                        _indexSegment.DataFormat,
                        _indexSegment.ByteOffset);
                }
                else
                {
                    _inputAssembler.SetIndexBuffer(null, Format.FormatUnknown, 0);
                }
            }

            // Check if a change of layout is required
            if (anyVbChanged || _materialStage.HasMaterialChanged)
            {
                _vertexLayout = GetInputLayout();
                Pipe.Context->IASetInputLayout(_vertexLayout.Native);
            }
        }

        /// <summary>Retrieves or creates a usable input layout for the provided vertex buffers and sub-effect.</summary>
        /// <returns>An instance of InputLayout.</returns>
        private VertexInputLayout GetInputLayout()
        {
            if (_materialStage.BoundShader == null)
                return null;

            // Retrieve layout list or create new one if needed.
            foreach(VertexInputLayout l in _cachedLayouts)
            {
                bool match = l.IsMatch(Device.Log, _slotVertexBuffers, _materialStage.BoundShader.InputStructure, _vertexSlotCount);
                if (match)
                    return l;
            }

            // A new layout is required
            VertexInputLayout input = new VertexInputLayout(Device, 
                _slotVertexBuffers, 
                _materialStage.BoundShader.InputStructureByteCode,
                _materialStage.BoundShader.InputStructure);
            _cachedLayouts.Add(input);

            return input;
        }

        internal GraphicsValidationResult Validate(GraphicsValidationMode mode)
        {
            GraphicsValidationResult result = GraphicsValidationResult.Successful;

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

            result |= CheckMaterial();
            return result;
        }


        /// <summary>Validate vertex buffer and vertex shader.</summary>
        /// <param name="vbChanged">Has the vertex buffer changed.</param>
        /// <param name="veChanged">Has the vertex effect changed.</param>
        /// <returns></returns>
        private GraphicsValidationResult CheckMaterial()
        {
            GraphicsValidationResult result = GraphicsValidationResult.Successful;

            if (_materialStage.IsValid == false)
            {
                result |= GraphicsValidationResult.InvalidMaterial;
            }
            else
            {
                if (_materialStage.BoundShader == null)
                    result |= GraphicsValidationResult.MissingMaterial;
            }

            return result;
        }

        private GraphicsValidationResult CheckVertexSegment()
        {
            GraphicsValidationResult result = GraphicsValidationResult.Successful;

            if (_slotVertexBuffers[0].Object == null)
                result |= GraphicsValidationResult.MissingVertexSegment;

            return result;
        }

        private GraphicsValidationResult CheckIndexSegment()
        {
            GraphicsValidationResult result = GraphicsValidationResult.Successful;

            // If the index buffer is null, this method will always fail because 
            // it assumes it is only being called during an indexed draw call.
            if (_slotIndexBuffer.Object == null)
                result |= GraphicsValidationResult.MissingIndexSegment;

            return result;
        }

        private GraphicsValidationResult CheckInstancing()
        {
            if (_vertexLayout != null && _vertexLayout.IsInstanced)
                return GraphicsValidationResult.Successful;
            else
                return GraphicsValidationResult.NonInstancedVertexLayout;
        }

        internal void SetVertexSegment(BufferSegment seg, int slot)
        {
            if (seg != null)
            {
                if ((seg.Buffer.Description.BindFlags & BindFlag.BindVertexBuffer) != BindFlag.BindVertexBuffer)
                    throw new InvalidOperationException("The provided buffer segment is not part of a vertex buffer.");
            }

            EnsureVertexSlots(slot + 1);
            _vertexSegments[slot] = seg;
        }

        internal void SetVertexSegments(BufferSegment[] segments, int firstSlot, int startIndex, int segmentCount)
        {
            int end = startIndex + segmentCount;
            int slotID = firstSlot;
            EnsureVertexSlots(end);

            for (int i = startIndex; i < end; i++)
            {
                if (segments[i] != null)
                {
                    if ((segments[i].Buffer.Description.BindFlags & BindFlag.BindVertexBuffer) != BindFlag.BindVertexBuffer)
                        throw new InvalidOperationException($"The provided buffer segment at index {i} is not part of a vertex buffer.");
                }

                _vertexSegments[slotID] = segments[i];
                slotID++;
            }
        }

        internal BufferSegment GetVertexSegment(int slot)
        {
            return _vertexSegments[slot];
        }

        internal void SetIndexSegment(BufferSegment seg)
        {
            if (seg != null)
            {
                if ((seg.Buffer.Description.BindFlags & BindFlag.BindIndexBuffer) != BindFlag.BindIndexBuffer)
                    throw new InvalidOperationException("The provided buffer segment is not part of an index buffer.");
            }

            _indexSegment = seg;
        }

        internal void GetVertexSegments(BufferSegment[] destination)
        {
            for (int i = 0; i < _vertexSlotCount; i++)
                destination[i] = _vertexSegments[i];
        }

        internal BufferSegment GetIndexSegment()
        {
            return _indexSegment;
        }

        private void EnsureVertexSlots(int count)
        {
            _vertexSlotCount = _vertexSlotCount < count ? count : _vertexSlotCount;
        }

        protected override void OnDispose()
        {
            DisposeObject(ref _materialStage);

            // Dispose input layouts.
            for (int i = 0; i < _cachedLayouts.Count; i++)
                _cachedLayouts[i].Dispose();

            _cachedLayouts.Clear();

            base.OnDispose();
        }

        internal override bool IsValid { get { return true; } }

        internal Material Material
        {
            get => _materialStage.Shader;
            set => _materialStage.Shader = value;
        }
    }
}
