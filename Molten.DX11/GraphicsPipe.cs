using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Manages the pipeline of a either an immediate or deferred <see cref="DeviceContext"/>.</summary>
    internal class GraphicsPipe : EngineObject
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

        GraphicsDepthStage _depthStencil;
        GraphicsBlendStage _blendState;
        GraphicsRasterizerStage _rasterizer;

        ComputeInputStage _computeStage;
        PipelineInput _input;
        PipelineOutput _output;

        VertexInputLayout _curInputLayout;
        GraphicsValidationResult _drawResult;

        GraphicsDeviceDX11 _device;
        DeviceContext _context;
        PipeStateStack _stateStack;
        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;
        DrawInfo _drawInfo;

        internal void Initialize(Logger log, GraphicsDeviceDX11 device, DeviceContext context, int id)
        {
            ID = id;
            _context = context;
            _device = device;
            _defaultProfiler = _profiler = new RenderProfiler();
            _drawInfo = new DrawInfo();
            Log = log;

            if (Context.TypeInfo == DeviceContextType.Immediate)
                Type = GraphicsContextType.Immediate;
            else
                Type = GraphicsContextType.Deferred;

            _context.ClearState();

            _stateStack = new PipeStateStack(this);
            _computeStage = new ComputeInputStage(this);
            _input = new PipelineInput(this);
            _output = new PipelineOutput(this);

            _depthStencil = new GraphicsDepthStage(this);
            _blendState = new GraphicsBlendStage(this);
            _rasterizer = new GraphicsRasterizerStage(this);

            // Apply the surface of the graphics device's output initialally.
            SetRenderSurfaces(null);
            _output.DepthWritePermission = GraphicsDepthWritePermission.Enabled;
        }

        /// <summary>Dispatches a compute effect to the GPU.</summary>
        public void Dispatch(ComputeTask task, int x, int y, int z)
        {
            _computeStage.Shader = task;

            // TODO fix null UAV on depth texture (possibly issue with TextureBase).

            _output.Refresh(); // TODO Why is this here??? Compute shaders don't output to surfaces/RTs?
            _computeStage.Dispatch(x, y, z);
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        /// <param name="count">The number of render surfaces to set.</param>
        public void SetRenderSurfaces(RenderSurface[] surfaces, int count)
        {
            _output.SetRenderSurfaces(surfaces, count);
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        public void SetRenderSurfaces(params RenderSurface[] surfaces)
        {
            if (surfaces == null)
                _output.SetRenderSurfaces(null, 0);
            else
                _output.SetRenderSurfaces(surfaces, surfaces.Length);
        }

        /// <summary>Sets a render surface.</summary>
        /// <param name="surface">The surface to be set.</param>
        /// <param name="slot">The ID of the slot that the surface is to be bound to.</param>
        public void SetRenderSurface(RenderSurface surface, int slot)
        {
            _output.SetRenderSurface(surface, slot);
        }

        /// <summary>Fills an array with the current configuration of render surfaces.</summary>
        /// <param name="destinationArray"></param>
        public void GetRenderSurfaces(RenderSurface[] destinationArray)
        {
            _output.GetRenderSurfaces(destinationArray);
        }

        /// <summary>Returns the render surface that is bound to the requested slot ID. Returns null if the slot is empty.</summary>
        /// <param name="slot">The ID of the slot to retrieve a surface from.</param>
        /// <returns></returns>
        public RenderSurface GetRenderSurface(int slot)
        {
            return _output.GetRenderSurface(slot);
        }

        /// <summary>Resets a render surface slot.</summary>
        /// <param name="resetMode">The type of reset to perform.</param>
        /// <param name="slot">The ID of the slot to reset.</param>
        public void UnsetRenderSurface(int slot)
        {
            _output.ResetRenderSurface(slot);
        }

        public void UnsetRenderSurfaces()
        {
            _output.ResetRenderSurfaces();
        }

        public int PushState()
        {
            return _stateStack.Push();
        }

        public void PopState()
        {
            _stateStack.Pop();
        }

        private GraphicsValidationResult ApplyState(Material material, MaterialPass pass,
            GraphicsValidationMode mode,
            PrimitiveTopology topology)
        {
            if (topology == PrimitiveTopology.Undefined)
                return GraphicsValidationResult.UndefinedTopology;

            GraphicsValidationResult result = GraphicsValidationResult.Successful;

            _input.Material = material;
            _input.Refresh(pass, _drawInfo.Conditions, topology);
            _output.DepthWritePermission = DepthWriteOverride != GraphicsDepthWritePermission.Enabled ? DepthWriteOverride : pass.DepthState[_drawInfo.Conditions].WritePermission;
            _output.Refresh();

            _blendState.Current = pass.BlendState[_drawInfo.Conditions];
            _rasterizer.Current = pass.RasterizerState[_drawInfo.Conditions];
            _depthStencil.Current = pass.DepthState[_drawInfo.Conditions];

            // Apply render targets and states.
            _depthStencil.Refresh();
            _blendState.Refresh();
            _rasterizer.Refresh();

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

        /// <summary>Draw non-indexed, non-instanced primitives. All queued compute shader dispatch requests are also processed</summary>
        /// <param name="vertexCount">The number of vertices to draw from the provided vertex buffer(s).</param>
        /// <param name="vertexStartIndex">The vertex to start drawing from.</param>
        /// <param name="topology">The primitive topolog to use when drawing with a NULL vertex buffer. Vertex buffers always override this when applied.</param>
        public void Draw(Material material, int vertexCount, PrimitiveTopology topology, int vertexStartIndex = 0)
        {
#if DEBUG
            if (!_drawInfo.Began)
                throw new GraphicsContextException("GraphicsPipe: BeginDraw() must be called before calling a Draw___() method.");
#endif

            // Re-render the same material for I iterations.
            for (int i = 0; i < material.Iterations; i++)
            {
                for (int j = 0; j < material.PassCount; j++)
                {
                    MaterialPass pass = material.Passes[j];
                    _drawResult = ApplyState(material, pass, GraphicsValidationMode.Unindexed, topology);

                    if (_drawResult == GraphicsValidationResult.Successful)
                    {
                        // Re-render the same pass for K iterations.
                        for (int k = 0; k < pass.Iterations; k++)
                        {
                            _context.Draw(vertexCount, vertexStartIndex);
                            Profiler.Current.DrawCalls++;
                        }
                    }
                    else
                    {
                        _device.Log.WriteWarning($"Draw() call failed with result: {_drawResult} -- Iteration: M{i}/{material.Iterations}P{j}/{material.PassCount} -- " +
                            $"Material: {material.Name} -- Topology: {topology} -- VertexCount: { vertexCount}");
                        break;
                    }
                }
            }
        }

        /// <summary>Draw instanced, unindexed primitives. </summary>
        /// <param name="vertexCountPerInstance">The expected number of vertices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="topology">The expected topology of the indexed vertex data.</param>
        /// <param name="vertexStartIndex">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public void DrawInstanced(Material material,
            int vertexCountPerInstance,
            int instanceCount,
            PrimitiveTopology topology,
            int vertexStartIndex = 0,
            int instanceStartIndex = 0)
        {
            if (!_drawInfo.Began)
                throw new GraphicsContextException("GraphicsPipe: BeginDraw() must be called before calling a Draw___() method.");

            // Re-render the same material for I iterations.
            for (int i = 0; i < material.Iterations; i++)
            {
                for (int j = 0; j < material.PassCount; j++)
                {
                    MaterialPass pass = material.Passes[j];
                    _drawResult = ApplyState(material, pass, GraphicsValidationMode.Instanced, topology);
                    if (_drawResult == GraphicsValidationResult.Successful)
                    {
                        // Re-render the same pass for K iterations.
                        for (int k = 0; k < pass.Iterations; k++)
                        {
                            _context.DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex);
                            Profiler.Current.DrawCalls++;
                        }
                    }
                    else
                    {
                        _device.Log.WriteWarning($"DrawInstanced() call failed with result: {_drawResult} -- Iteration: M{i}/{material.Iterations}P{j}/{material.PassCount} -- Material: {material.Name} -- " +
                            $"Topology: {topology} -- VertexCount: { vertexCountPerInstance} -- Instances: {instanceCount}");
                        break;
                    }
                }
            }
        }

        /// <summary>Draw indexed, non-instanced primitives.</summary>
        /// <param name="vertexCount">The number of indices to draw from the provided vertex/index buffers.</param>
        /// <param name="vertexIndexOffset">A value added to each index before reading from the vertex buffer.</param>
        /// <param name="indexCount">The number of indices to be drawn.</param>
        /// <param name="startIndex">The index to start drawing from.</param>
        /// <param name="topology">The toplogy to apply when drawing with a NULL vertex buffer. Vertex buffers always override this when applied.</param>
        public void DrawIndexed(Material material,
            int indexCount,
            PrimitiveTopology topology,
            int vertexIndexOffset = 0,
            int startIndex = 0)
        {
            if (!_drawInfo.Began)
                throw new GraphicsContextException("GraphicsPipe: BeginDraw() must be called before calling a Draw___() method.");

            // Re-render the same material for I iterations.
            for (int i = 0; i < material.Iterations; i++)
            {
                for (int j = 0; j < material.PassCount; j++)
                {
                    MaterialPass pass = material.Passes[j];
                    _drawResult = ApplyState(material, pass, GraphicsValidationMode.Indexed, topology);
                    if (_drawResult == GraphicsValidationResult.Successful)
                    {
                        // Re-render the same pass for K iterations.
                        for (int k = 0; k < pass.Iterations; k++)
                        {
                            _context.DrawIndexed(indexCount, startIndex, vertexIndexOffset);
                            Profiler.Current.DrawCalls++;
                        }
                    }
                    else
                    {
                        _device.Log.WriteWarning($"DrawIndexed() call failed with result: {_drawResult} -- Iteration: M{i}/{material.Iterations}P{j}/{material.PassCount}" +
                            $" -- Material: {material.Name} -- Topology: {topology} -- indexCount: { indexCount}");
                        break;
                    }
                }
            }
        }

        /// <summary>Draw indexed, instanced primitives.</summary>
        /// <param name="indexCountPerInstance">The expected number of indices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="topology">The expected topology of the indexed vertex data.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="vertexIndexOffset">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public void DrawIndexedInstanced(Material material,
            int indexCountPerInstance,
            int instanceCount,
            PrimitiveTopology topology,
            StateConditions conditions,
            int startIndex = 0,
            int vertexIndexOffset = 0,
            int instanceStartIndex = 0)
        {
            if (!_drawInfo.Began)
                throw new GraphicsContextException("GraphicsPipe: BeginDraw() must be called before calling a Draw___() method.");

            // Re-render the same material for I iterations.
            for (int i = 0; i < material.Iterations; i++)
            {
                for (int j = 0; j < material.PassCount; j++)
                {
                    MaterialPass pass = material.Passes[j];
                    _drawResult = ApplyState(material, pass, GraphicsValidationMode.InstancedIndexed, topology);
                    if (_drawResult == GraphicsValidationResult.Successful)
                    {
                        // Re-render the same pass for K iterations.
                        for (int k = 0; k < pass.Iterations; k++)
                        {
                            _context.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex);
                            Profiler.Current.DrawCalls++;
                        }
                    }
                }
            }
        }

        /// <summary>Sets a vertex buffer to a particular input slot.</summary>
        /// <param name="segment">The buffer.</param>
        /// <param name="slot">The input slot ID.</param>
        /// <param name="byteOffset">The number of bytes to offset the starting point within the buffer.</param>
        internal void SetVertexSegment(BufferSegment segment, int slot)
        {
            _input.SetVertexSegment(segment, slot);
        }

        /// <summary>Sets a list of vertex buffers to input slots.</summary>
        /// <param name="segments">The buffers to set.</param>
        /// <param name="startIndex">The index within the buffer list/array to start setting.</param>
        /// <param name="firstSlot">The input slot to start setting.</param>
        /// <param name="byteOffsets">A list of byte offsets. each entry/element corresponds to the buffers in the buffer array.</param>
        internal void SetVertexSegments(BufferSegment[] segments)
        {
            _input.SetVertexSegments(segments, 0, 0, segments.Length);
        }

        /// <summary>Sets a list of vertex buffers to input slots.</summary>
        /// <param name="segments">The buffers to set.</param>
        /// <param name="startIndex">The index within the buffer list/array to start setting.</param>
        /// <param name="firstSlot">The input slot to start setting.</param>
        /// <param name="byteOffsets">A list of byte offsets. each entry/element corresponds to the buffers in the buffer array.</param>
        internal void SetVertexSegments(BufferSegment[] segments, int count, int firstSlot, int startIndex)
        {
            _input.SetVertexSegments(segments, firstSlot, startIndex, count);
        }

        /// <summary>Sets a index buffer.</summary>
        /// <param name="segment">The buffer segment to set.</param>
        /// <param name="byteOffset">The number of bytes to offset the starting point within the buffer.</param>
        internal void SetIndexSegment(BufferSegment segment)
        {
            _input.SetIndexSegment(segment);
        }

        /// <summary>Copyies a list of vertex <see cref="BufferSegment"/> that are set on the current <see cref="GraphicsPipe"/>. Any empty slots will be null.</summary>
        /// <param name="destination"></param>
        internal void GetVertexSegments(BufferSegment[] destination)
        {
            _input.GetVertexSegments(destination);
        }

        /// <summary>Returns the current index <see cref="BufferSegment"/>, or null if not set.</summary>
        /// <returns></returns>
        internal BufferSegment GetIndexSegment()
        {
            return _input.GetIndexSegment();
        }

        /// <summary>Clears the first render target that is set on the device.</summary>
        /// <param name="color"></param>
        /// <param name="slot"></param>
        public void Clear(Color color, int slot = 0)
        {
            _output.Clear(color, slot);
        }

        /// <summary>Dispoes of the current <see cref="GraphicsPipe"/> instance.</summary>
        protected override void OnDispose()
        {
            DisposeObject(ref _output);
            DisposeObject(ref _input);
            DisposeObject(ref _computeStage);

            // Dispose of graphical stages
            DisposeObject(ref _depthStencil);
            DisposeObject(ref _blendState);
            DisposeObject(ref _rasterizer);

            DisposeObject(ref _curInputLayout);

            // Dispose context.
            if (Type != GraphicsContextType.Immediate)
            {
                DisposeObject(ref _context);
                _device.RemoveDeferredPipe(this);
            }

            base.OnDispose();
        }

        /// <summary>Gets the current <see cref="GraphicsPipe"/> type. This value will not change during the context's life.</summary>
        public GraphicsContextType Type { get; private set; }

        internal GraphicsDeviceDX11 Device => _device;

        internal DeviceContext Context => _context;

        internal Logger Log { get; private set; }

        /// <summary>Gets the profiler bound to the current <see cref="GraphicsPipe"/>. Contains statistics for this pipe alone.</summary>
        internal RenderProfiler Profiler
        {
            get => _profiler;
            set => _profiler = value ?? _defaultProfiler;
        }

        internal int ID { get; private set; }

        /// <summary>Gets the depth stencil component of the graphics device.</summary>
        internal GraphicsDepthStage DepthStencil
        {
            get { return _depthStencil; }
        }

        /// <summary>Gets the blend state component of the graphics device.</summary>
        internal GraphicsBlendStage BlendState
        {
            get { return _blendState; }
        }

        /// <summary>Gets the rasterizer component of the graphics device.</summary>
        internal GraphicsRasterizerStage Rasterizer
        {
            get { return _rasterizer; }
        }

        /// <summary>Gets the result flags of the last draw call.</summary>
        internal GraphicsValidationResult DrawResult { get { return _drawResult; } }

        /// <summary>Gets the pipeline input.</summary>
        internal PipelineInput Input { get { return _input; } }

        /// <summary>Gets the pipeline output.</summary>
        internal PipelineOutput Output { get { return _output; } }

        /// <summary>
        /// Gets or sets the output depth surface.
        /// </summary>
        internal DepthStencilSurface DepthSurface
        {
            get => _output.DepthSurface;
            set => _output.DepthSurface = value;
        }

        internal GraphicsDepthWritePermission DepthWriteOverride { get; set; } = GraphicsDepthWritePermission.Enabled;

        internal GraphicsDepthWritePermission DepthWritePermission => _output.DepthWritePermission;
    }
}
