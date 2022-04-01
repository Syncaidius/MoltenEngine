using Molten.IO;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal delegate void ContextDrawCallback(MaterialPass pass);
    internal delegate void ContextDrawFailCallback(MaterialPass pass, uint iteration, uint passNumber, GraphicsBindResult result);

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

        ID3D11DeviceContext* _context;
        ContextStateStack _stateStack;
        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;
        DrawInfo _drawInfo;

        internal DeviceContext()
        {
            _drawInfo = new DrawInfo();
            _defaultProfiler = _profiler = new RenderProfiler();
        }

        internal void Initialize(Logger log, Device device, ID3D11DeviceContext* context)
        {
            _context = context;
            Device = device;
            Log = log;

            if (_context->GetType() == DeviceContextType.DeviceContextImmediate)
                Type = GraphicsContextType.Immediate;
            else
                Type = GraphicsContextType.Deferred;

            State = new DeviceContextState(this);

            _stateStack = new ContextStateStack(this);

            // Apply the surface of the graphics device's output initialally.
            State.SetRenderSurfaces(null);
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

            State.Bind(pass, _drawInfo.Conditions, topology);

            // Validate all pipeline components.
            GraphicsBindResult result = State.Validate(mode);

            return result;
        }

        internal void BeginDraw(StateConditions conditions)
        {
#if DEBUG
            if (_drawInfo.Began)
                throw new GraphicsContextException($"{nameof(DeviceContext)}: EndDraw() must be called before the next BeginDraw() call.");
#endif

            _drawInfo.Began = true;
            _drawInfo.Conditions = conditions;
        }

        internal void EndDraw()
        {
#if DEBUG
            if (!_drawInfo.Began)
                throw new GraphicsContextException($"{nameof(DeviceContext)}: BeginDraw() must be called before EndDraw().");
#endif

            _drawInfo.Reset();
        }

        private GraphicsBindResult DrawCommon(Material mat, GraphicsValidationMode mode, VertexTopology topology, 
            ContextDrawCallback drawCallback, ContextDrawFailCallback failCallback)
        {
            GraphicsBindResult vResult = GraphicsBindResult.Successful;

            if (!_drawInfo.Began)
                throw new GraphicsContextException($"{nameof(DeviceContext)}: BeginDraw() must be called before calling {nameof(Draw)}()");

            State.Material.Value = mat;

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

        /// <summary>Draw non-indexed, non-instanced primitives. 
        /// All queued compute shader dispatch requests are also processed</summary>
        /// <param name="vertexCount">The number of vertices to draw from the provided vertex buffer(s).</param>
        /// <param name="vertexStartIndex">The vertex to start drawing from.</param>
        /// <param name="topology">The primitive topology to use when drawing with a NULL vertex buffer. 
        /// Vertex buffers always override this when applied.</param>
        public GraphicsBindResult Draw(Material material, uint vertexCount, VertexTopology topology, uint vertexStartIndex = 0)
        {
            return DrawCommon(material, GraphicsValidationMode.Unindexed, topology, (pass) =>
            {
                _context->Draw(vertexCount, vertexStartIndex);
            },
            (pass, iteration, passNumber, vResult) =>
            {
                Device.Log.Warning($"Draw() call failed with result: {vResult} -- " + 
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
        public GraphicsBindResult DrawInstanced(Material material,
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
                Device.Log.Warning($"DrawInstanced() call failed with result: {vResult} -- " + 
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
        public GraphicsBindResult DrawIndexed(Material material,
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
                Device.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
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
        public GraphicsBindResult DrawIndexedInstanced(Material material,
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
                Device.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Iteration: M{it}/{material.Iterations}P{passNum}/{material.PassCount}" +
                    $" -- Material: {material.Name} -- Topology: {topology} -- Indices-per-instance: { indexCountPerInstance}");
            });
        }

        internal void Dispatch(ComputeTask task, uint groupsX, uint groupsY, uint groupsZ)
        {
            bool csChanged = State.Bind(task);

            if (State.CS.Shader.BoundValue == null)
            {
                return;
            }
            else
            {
                // Ensure dispatch is within supported range.
                int maxZ = Device.Features.Compute.MaxDispatchZDimension;
                int maxXY = Device.Features.Compute.MaxDispatchXYDimension;

                if (groupsZ > maxZ)
                {
#if DEBUG
                    Log.Write("Unable to dispatch compute shader. Z dimension (" + groupsZ + ") is greater than supported (" + maxZ + ").");
#endif
                    return;
                }
                else if (groupsX > maxXY)
                {
#if DEBUG
                    Log.Write("Unable to dispatch compute shader. X dimension (" + groupsX + ") is greater than supported (" + maxXY + ").");
#endif
                    return;
                }
                else if (groupsY > maxXY)
                {
#if DEBUG
                    Log.Write("Unable to dispatch compute shader. Y dimension (" + groupsY + ") is greater than supported (" + maxXY + ").");
#endif
                    return;
                }

                // TODO have this processed during the presentation call of each graphics context.

               
                Native->Dispatch(groupsX, groupsY, groupsZ);
            }
        }

        /// <summary>Dispoes of the current <see cref="Graphics.DeviceContext"/> instance.</summary>
        protected override void OnDispose()
        {
            State.Dispose();

            // Dispose context.
            if (Type != GraphicsContextType.Immediate)
            {
                SilkUtil.ReleasePtr(ref _context);
                Device.RemoveDeferredContext(this);
            }
        }

        /// <summary>Gets the current <see cref="Graphics.DeviceContext"/> type. This value will not change during the context's life.</summary>
        public GraphicsContextType Type { get; private set; }

        internal Device Device { get; private set; }

        internal ID3D11DeviceContext* Native => _context;

        internal Logger Log { get; private set; }

        /// <summary>Gets the profiler bound to the current <see cref="Graphics.DeviceContext"/>. Contains statistics for this context alone.</summary>
        public RenderProfiler Profiler
        {
            get => _profiler;
            set => _profiler = value ?? _defaultProfiler;
        }

        /// <summary>
        /// Gets the state of the current <see cref="DeviceContext"/>.
        /// </summary>
        internal DeviceContextState State { get; private set; }

    }
}
