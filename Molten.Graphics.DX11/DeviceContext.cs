using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;
using Molten.IO;

namespace Molten.Graphics
{
    internal delegate void PipeDrawCallback(MaterialPass pass);
    internal delegate void PipeDrawFailCallback(MaterialPass pass, uint iteration, uint passNumber, GraphicsValidationResult result);

    /// <summary>Manages the pipeline of a either an immediate or deferred <see cref="DeviceContext"/>.</summary>
    public unsafe partial class DeviceContext : EngineObject
    {
        class DrawInfo
        {
            public bool Began;
            public StateConditions Conditions;

            public void Reset()
            {
                Began = false;
                Conditions = StateConditions.None;
            }
        }

        InputAssemblerStage _input;
        ShaderComputeStage _compute;

        Device _device;
        ID3D11DeviceContext* _context;
        PipeStateStack _stateStack;
        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;
        DrawInfo _drawInfo;

        internal void Initialize(Logger log, Device device, ID3D11DeviceContext* context)
        {
            _context = context;
            _device = device;
            _defaultProfiler = _profiler = new RenderProfiler();
            _drawInfo = new DrawInfo();
            Log = log;

            if (_context->GetType() == DeviceContextType.DeviceContextImmediate)
                Type = GraphicsContextType.Immediate;
            else
                Type = GraphicsContextType.Deferred;

            _context->ClearState();

            _stateStack = new PipeStateStack(this);
            _compute = new ShaderComputeStage(this);
            _input = new InputAssemblerStage(this);
            Output = new OutputMergerStage(this);

            DepthStencil = new GraphicsDepthStage(this);
            BlendState = new GraphicsBlendStage(this);
            Rasterizer = new GraphicsRasterizerStage(this);

            // Apply the surface of the graphics device's output initialally.
            SetRenderSurfaces(null);
            Output.DepthWritePermission = GraphicsDepthWritePermission.Enabled;
        }

        /// <summary>
        /// Maps a resource on the current <see cref="Graphics.DeviceContext"/>.
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
        /// Maps a resource on the current <see cref="Graphics.DeviceContext"/> and provides a <see cref="RawStream"/> to aid read-write operations.
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

            bool canWrite = mapType != Map.MapRead;
            bool canRead = mapType == Map.MapRead || mapType == Map.MapReadWrite;
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

        /// <summary>Dispatches a compute effect to the GPU.</summary>
        public void Dispatch(ComputeTask task, uint x, uint y, uint z)
        {
            _compute.Task.Value = task;

            // TODO fix null UAV on depth texture (possibly issue with TextureBase).

            Output.Refresh(); // Refresh any outputs that may be used by the compute task (Render targets!).
            _compute.Dispatch(x, y, z);
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        /// <param name="count">The number of render surfaces to set.</param>
        public void SetRenderSurfaces(RenderSurface[] surfaces, uint count)
        {
            Output.SetRenderSurfaces(surfaces, count);
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        public void SetRenderSurfaces(params RenderSurface[] surfaces)
        {
            if (surfaces == null)
                Output.SetRenderSurfaces(null, 0);
            else
                Output.SetRenderSurfaces(surfaces, (uint)surfaces.Length);
        }

        /// <summary>Sets a render surface.</summary>
        /// <param name="surface">The surface to be set.</param>
        /// <param name="slot">The ID of the slot that the surface is to be bound to.</param>
        public void SetRenderSurface(RenderSurface surface, uint slot)
        {
            Output.SetRenderSurface(surface, slot);
        }

        /// <summary>Fills an array with the current configuration of render surfaces.</summary>
        /// <param name="destinationArray"></param>
        public void GetRenderSurfaces(RenderSurface[] destinationArray)
        {
            Output.GetRenderSurfaces(destinationArray);
        }

        /// <summary>Returns the render surface that is bound to the requested slot ID. Returns null if the slot is empty.</summary>
        /// <param name="slot">The ID of the slot to retrieve a surface from.</param>
        /// <returns></returns>
        public RenderSurface GetRenderSurface(uint slot)
        {
            return Output.GetRenderSurface(slot);
        }

        /// <summary>Resets a render surface slot.</summary>
        /// <param name="resetMode">The type of reset to perform.</param>
        /// <param name="slot">The ID of the slot to reset.</param>
        public void UnsetRenderSurface(uint slot)
        {
            Output.SetRenderSurface(null, slot);
        }

        public void UnsetRenderSurfaces()
        {
            Output.ResetRenderSurfaces();
        }

        public int PushState()
        {
            return _stateStack.Push();
        }

        public void PopState()
        {
            _stateStack.Pop();
        }

        private GraphicsValidationResult ApplyState(MaterialPass pass,
            GraphicsValidationMode mode,
            VertexTopology topology)
        {
            if (topology == VertexTopology.Undefined)
                return GraphicsValidationResult.UndefinedTopology;

            GraphicsValidationResult result = GraphicsValidationResult.Successful;

            _input.Bind(pass, _drawInfo.Conditions, topology);

            Output.DepthWritePermission = DepthWriteOverride != GraphicsDepthWritePermission.Enabled ? 
                DepthWriteOverride : pass.DepthState[_drawInfo.Conditions].WritePermission;
            Output.Refresh();

            BlendState.State.Value = pass.BlendState[_drawInfo.Conditions];
            Rasterizer.State.Value = pass.RasterizerState[_drawInfo.Conditions];
            DepthStencil.State.Value = pass.DepthState[_drawInfo.Conditions];

            // Apply render targets and states.
            DepthStencil.Bind();
            BlendState.Bind();
            Rasterizer.Bind();

            // Validate all pipeline components.
            result |= _input.Validate(mode);

            return result;
        }

        internal void BeginDraw(StateConditions conditions)
        {
#if DEBUG
            if (_drawInfo.Began)
                throw new GraphicsContextException("GraphicsPipe: EndDraw() must be called before the next BeginDraw() call.");
#endif

            _drawInfo.Began = true;
            _drawInfo.Conditions = conditions;
        }

        internal void EndDraw()
        {
#if DEBUG
            if (!_drawInfo.Began)
                throw new GraphicsContextException("GraphicsPipe: BeginDraw() must be called before EndDraw().");
#endif

            _drawInfo.Reset();
        }

        private GraphicsValidationResult DrawCommon(Material mat, GraphicsValidationMode mode, VertexTopology topology, 
            PipeDrawCallback drawCallback, PipeDrawFailCallback failCallback)
        {
            GraphicsValidationResult vResult = GraphicsValidationResult.Successful;

            if (!_drawInfo.Began)
                throw new GraphicsContextException($"GraphicsPipe: BeginDraw() must be called before calling {nameof(Draw)}()");

            _input.Material.Value = mat;

            // Re-render the same material for mat.Iterations.
            for (uint i = 0; i < mat.Iterations; i++)
            {
                for (uint j = 0; j < mat.PassCount; j++)
                {
                    MaterialPass pass = mat.Passes[j];
                    vResult = ApplyState(pass, mode, topology);

                    if (vResult == GraphicsValidationResult.Successful)
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

        /// <summary>Draw non-indexed, non-instanced primitives. 
        /// All queued compute shader dispatch requests are also processed</summary>
        /// <param name="vertexCount">The number of vertices to draw from the provided vertex buffer(s).</param>
        /// <param name="vertexStartIndex">The vertex to start drawing from.</param>
        /// <param name="topology">The primitive topology to use when drawing with a NULL vertex buffer. 
        /// Vertex buffers always override this when applied.</param>
        public GraphicsValidationResult Draw(Material material, uint vertexCount, VertexTopology topology, uint vertexStartIndex = 0)
        {
            return DrawCommon(material, GraphicsValidationMode.Unindexed, topology, (pass) =>
            {
                _context->Draw(vertexCount, vertexStartIndex);
            },
            (pass, iteration, passNumber, vResult) =>
            {
                _device.Log.WriteWarning($"Draw() call failed with result: {vResult} -- " + 
                    $"Iteration: M{iteration}/{material.Iterations}P{passNumber}/{material.PassCount} -- " +
                    $"Material: {material.Name} -- Topology: {topology} -- VertexCount: { vertexCount}");
            });
        }

        /// <summary>Draw instanced, unindexed primitives. </summary>
        /// <param name="vertexCountPerInstance">The expected number of vertices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="topology">The expected topology of the indexed vertex data.</param>
        /// <param name="vertexStartIndex">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public GraphicsValidationResult DrawInstanced(Material material,
            uint vertexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material, GraphicsValidationMode.Instanced, topology, (pass) =>
            {
                _context->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex);
            },
            (pass, iteration, passNum, vResult) =>
            {
                _device.Log.WriteWarning($"DrawInstanced() call failed with result: {vResult} -- " + 
                        $"Iteration: M{iteration}/{material.Iterations}P{passNum}/{material.PassCount} -- Material: {material.Name} -- " +
                        $"Topology: {topology} -- VertexCount: { vertexCountPerInstance} -- Instances: {instanceCount}");
            });
        }

        /// <summary>Draw indexed, non-instanced primitives.</summary>
        /// <param name="vertexCount">The number of indices to draw from the provided vertex/index buffers.</param>
        /// <param name="vertexIndexOffset">A value added to each index before reading from the vertex buffer.</param>
        /// <param name="indexCount">The number of indices to be drawn.</param>
        /// <param name="startIndex">The index to start drawing from.</param>
        /// <param name="topology">The toplogy to apply when drawing with a NULL vertex buffer. Vertex buffers always override this when applied.</param>
        public GraphicsValidationResult DrawIndexed(Material material,
            uint indexCount,
            VertexTopology topology,
            int vertexIndexOffset = 0,
            uint startIndex = 0)
        {
            return DrawCommon(material, GraphicsValidationMode.Indexed, topology, (pass) =>
            {
                _context->DrawIndexed(indexCount, startIndex, vertexIndexOffset);
            },
            (pass, it, passNum, vResult) =>
            {
                _device.Log.WriteWarning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Iteration: M{it}/{material.Iterations}P{passNum}/{material.PassCount}" +
                    $" -- Material: {material.Name} -- Topology: {topology} -- indexCount: { indexCount}");
            });
        }

        /// <summary>Draw indexed, instanced primitives.</summary>
        /// <param name="indexCountPerInstance">The expected number of indices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="topology">The expected topology of the indexed vertex data.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="vertexIndexOffset">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public GraphicsValidationResult DrawIndexedInstanced(Material material,
            uint indexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            StateConditions conditions,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material, GraphicsValidationMode.Indexed, topology, (pass) =>
            {
                _context->DrawIndexedInstanced(indexCountPerInstance, instanceCount,
                    startIndex, vertexIndexOffset, instanceStartIndex);
            },
            (pass, it, passNum, vResult) =>
            {
                _device.Log.WriteWarning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Iteration: M{it}/{material.Iterations}P{passNum}/{material.PassCount}" +
                    $" -- Material: {material.Name} -- Topology: {topology} -- Indices-per-instance: { indexCountPerInstance}");
            });

            if (!_drawInfo.Began)
                throw new GraphicsContextException($"GraphicsPipe: BeginDraw() must be called before calling {nameof(DrawIndexedInstanced)}()");
        }

        /// <summary>Sets a vertex buffer to a particular input slot.</summary>
        /// <param name="segment">The buffer.</param>
        /// <param name="slot">The input slot ID.</param>
        /// <param name="byteOffset">The number of bytes to offset the starting point within the buffer.</param>
        internal void SetVertexSegment(BufferSegment segment, uint slot)
        {
            _input.VertexBuffers[slot].Value = segment;
        }

        /// <summary>Sets a list of vertex buffers to input slots.</summary>
        /// <param name="segments">The buffers to set.</param>
        /// <param name="startIndex">The index within the buffer list/array to start setting.</param>
        /// <param name="firstSlot">The input slot to start setting.</param>
        /// <param name="byteOffsets">A list of byte offsets. each entry/element corresponds to the buffers in the buffer array.</param>
        internal void SetVertexSegments(BufferSegment[] segments)
        {
            SetVertexSegments(segments, segments.Length, 0, 0);
        }

        /// <summary>Sets a list of vertex buffers to input slots.</summary>
        /// <param name="segments">The buffers to set.</param>
        /// <param name="startIndex">The index within 'segments' array to start setting.</param>
        /// <param name="firstSlot">The input slot to start setting.</param>
        /// <param name="byteOffsets">A list of byte offsets. each entry/element corresponds to the buffers in the buffer array.</param>
        internal void SetVertexSegments(BufferSegment[] segments, int count, uint firstSlot, uint startIndex)
        {
            uint end = startIndex + (uint)count;
            uint slotID = firstSlot;

            for (uint i = startIndex; i < end; i++)
            {
                if (segments[i] != null)
                {
                    if (((BindFlag)segments[i].Buffer.Description.BindFlags & BindFlag.BindVertexBuffer) != BindFlag.BindVertexBuffer)
                        throw new InvalidOperationException($"The provided buffer segment at index {i} is not part of a vertex buffer.");
                }

                _input.VertexBuffers[slotID++].Value = segments[i];
            }
        }

        /// <summary>Sets a index buffer.</summary>
        /// <param name="segment">The buffer segment to set.</param>
        /// <param name="byteOffset">The number of bytes to offset the starting point within the buffer.</param>
        internal void SetIndexSegment(BufferSegment segment)
        {
            _input.IndexBuffer.Value = segment;
        }

        /// <summary>Copyies a list of vertex <see cref="BufferSegment"/> that are set on the current <see cref="Graphics.DeviceContext"/>. Any empty slots will be null.</summary>
        /// <param name="destination"></param>
        internal void GetVertexSegments(BufferSegment[] destination)
        {
            int needed = _input.VertexBuffers.SlotCount;
            if (destination.Length < needed)
                throw new InvalidOperationException($"The destination array is too small. Needs {needed} but has {destination.Length} capacity.");

            for (uint i = 0; i < needed; i++)
                destination[i] = _input.VertexBuffers[i].Value;
        }

        /// <summary>Returns the current index <see cref="BufferSegment"/>, or null if not set.</summary>
        /// <returns></returns>
        internal BufferSegment GetIndexSegment()
        {
            return _input.IndexBuffer.Value;
        }

        /// <summary>Clears the first render target that is set on the device.</summary>
        /// <param name="color"></param>
        /// <param name="slot"></param>
        public void Clear(Color color, uint slot = 0)
        {
            Output.Clear(color, slot);
        }

        /// <summary>Dispoes of the current <see cref="Graphics.DeviceContext"/> instance.</summary>
        protected override void OnDispose()
        {
            Output.Dispose();
            _input.Dispose();
            _compute.Dispose();

            DepthStencil.Dispose();
            BlendState.Dispose();
            Rasterizer.Dispose();

            // Dispose context.
            if (Type != GraphicsContextType.Immediate)
            {
                SilkUtil.ReleasePtr(ref _context);
                _device.RemoveDeferredPipe(this);
            }
        }

        /// <summary>Gets the current <see cref="Graphics.DeviceContext"/> type. This value will not change during the context's life.</summary>
        public GraphicsContextType Type { get; private set; }

        internal Device Device => _device;

        internal ID3D11DeviceContext* Native => _context;

        internal Logger Log { get; private set; }

        /// <summary>Gets the profiler bound to the current <see cref="Graphics.DeviceContext"/>. Contains statistics for this pipe alone.</summary>
        public RenderProfiler Profiler
        {
            get => _profiler;
            set => _profiler = value ?? _defaultProfiler;
        }

        /// <summary>Gets the depth stencil component of the graphics device.</summary>
        internal GraphicsDepthStage DepthStencil { get; private set; }

        /// <summary>Gets the blend state component of the graphics device.</summary>
        internal GraphicsBlendStage BlendState { get; private set; }

        /// <summary>Gets the rasterizer component of the graphics device.</summary>
        internal GraphicsRasterizerStage Rasterizer { get; private set; }

        /// <summary>Gets the output merger state of the current <see cref="Graphics.DeviceContext"/>.</summary>
        internal OutputMergerStage Output { get; private set; }

        internal GraphicsDepthWritePermission DepthWriteOverride { get; set; } = GraphicsDepthWritePermission.Enabled;

        internal GraphicsDepthWritePermission DepthWritePermission => Output.DepthWritePermission;
    }
}