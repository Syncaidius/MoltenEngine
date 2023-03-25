namespace Molten.Graphics
{
    public abstract class GraphicsCommandQueue : EngineObject
    {
        protected class BatchDrawInfo
        {
            public bool Began;

            public Vector3UI ComputeGroups;

            public CustomDrawInfo Custom { get; } = new CustomDrawInfo();

            public void Reset()
            {
                Began = false;
                Custom.Reset();
            }
        }

        /// <summary>
        /// A container for storing application data to share between completion callbacks of <see cref="HlslShader"/> passes.
        /// </summary>
        public class CustomDrawInfo
        {
            /// <summary>
            /// Custom compute dispatch group sizes. 
            /// <para>
            /// Any dimension that is 0 will default to the one provided by the shader's definition, if any.</para>
            /// </summary>
            public Vector3UI ComputeGroups;

            /// <summary>
            /// Gets a dictionary of custom values.
            /// </summary>
            public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

            public void Reset()
            {
                ComputeGroups = Vector3UI.Zero;
                Values.Clear();
            }
        }

        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;
        List<GraphicsSlot> _slots;

        protected GraphicsCommandQueue(GraphicsDevice device)
        {
            DrawInfo = new BatchDrawInfo();
            Device = device;
            _slots = new List<GraphicsSlot>();
            _defaultProfiler = _profiler = new RenderProfiler();
        }

        public unsafe GraphicsStream MapResource(GraphicsResource resource, uint subresource, uint streamOffset)
        {
            if (resource.Stream != null)
                throw new GraphicsResourceException(resource, $"Cannot map a resource that is already mapped. Dispose of the provided {nameof(GraphicsStream)} first");

            ResourceMap map = GetResourcePtr(resource, subresource, streamOffset);
            resource.Stream = new GraphicsStream(this, resource, ref map);
            resource.Stream.Position = streamOffset;
            return resource.Stream;
        }

        internal void UnmapResource(GraphicsResource resource)
        {
#if DEBUG
            if (resource.Stream == null)
                throw new GraphicsResourceException(resource, "$Cannot unmap a resource that has not been mapped yet. Call MapResource first");
#endif

            OnUnmapResource(resource, resource.Stream.SubResourceIndex);
            resource.Stream = null;
        }

        /// <summary>
        /// Invoked when a <see cref="GraphicsResource"/> is mapped for reading or writing by the CPU/system.
        /// </summary>
        /// <param name="resource">The <see cref="GraphicsResource"/></param>
        /// <param name="subresource">The sub-resource index. e.g. a texture mip-map level, or array slice.</param>
        /// <param name="streamOffset">An offset from the start of the sub-resource, in bytes.</param>
        /// <returns></returns>
        protected abstract ResourceMap GetResourcePtr(GraphicsResource resource, uint subresource, uint streamOffset);

        protected abstract void OnUnmapResource(GraphicsResource resource, uint subresource);

        protected internal abstract unsafe void UpdateResource(GraphicsResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch);

        protected internal abstract void CopyResource(GraphicsResource src, GraphicsResource dest);

        public abstract unsafe void CopyResourceRegion(GraphicsResource source, uint srcSubresource, ResourceRegion* sourceRegion,
            GraphicsResource dest, uint destSubresource, Vector3UI destStart);

        public void BeginDraw()
        {
#if DEBUG
            if (DrawInfo.Began)
                throw new GraphicsCommandQueueException(this, $"{nameof(GraphicsCommandQueue)}: EndDraw() must be called before the next BeginDraw() call.");
#endif

            DrawInfo.Began = true;
        }

        public void EndDraw()
        {
#if DEBUG
            if (!DrawInfo.Began)
                throw new GraphicsCommandQueueException(this, $"{nameof(GraphicsCommandQueue)}: BeginDraw() must be called before EndDraw().");
#endif

            DrawInfo.Reset();
        }

        /// <summary>Draw non-indexed, non-instanced primitives. 
        /// All queued compute shader dispatch requests are also processed</summary>
        /// <param name="shader">The <see cref="HlslShader"/> to apply when drawing.</param>
        /// <param name="vertexCount">The number of vertices to draw from the provided vertex buffer(s).</param>
        /// <param name="vertexStartIndex">The vertex to start drawing from.</param>
        public abstract GraphicsBindResult Draw(HlslShader shader, uint vertexCount, uint vertexStartIndex = 0);

        /// <summary>Draw instanced, unindexed primitives. </summary>
        /// <param name="shader">The <see cref="HlslShader"/> to apply when drawing.</param>
        /// <param name="vertexCountPerInstance">The expected number of vertices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="vertexStartIndex">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public abstract GraphicsBindResult DrawInstanced(HlslShader shader,
            uint vertexCountPerInstance,
            uint instanceCount,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0);

        /// <summary>Draw indexed, non-instanced primitives.</summary>
        /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
        /// <param name="vertexIndexOffset">A value added to each index before reading from the vertex buffer.</param>
        /// <param name="indexCount">The number of indices to be drawn.</param>
        /// <param name="startIndex">The index to start drawing from.</param>
        public abstract GraphicsBindResult DrawIndexed(HlslShader shader,
            uint indexCount,
            int vertexIndexOffset = 0,
            uint startIndex = 0);

        /// <summary>Draw indexed, instanced primitives.</summary>
        /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
        /// <param name="indexCountPerInstance">The expected number of indices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="vertexIndexOffset">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public abstract GraphicsBindResult DrawIndexedInstanced(HlslShader shader,
            uint indexCountPerInstance,
            uint instanceCount,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0);

        /// <summary>
        /// Dispatches a <see cref="HlslShader"/> as a compute shader. Any non-compute passes will be skipped.
        /// </summary>
        /// <param name="shader">The shader to be dispatched.</param>
        /// <param name="groups">The number of thread groups.</param>
        /// <returns></returns>
        public abstract GraphicsBindResult Dispatch(HlslShader shader, Vector3UI groups);

        public GraphicsSlot<T> RegisterSlot<T, B>(GraphicsBindTypeFlags bindType, string namePrefix, uint slotIndex)
where T : class, IGraphicsObject
where B : GraphicsSlotBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlot(bindType, namePrefix, slotIndex, binder);
        }

        public GraphicsSlot<T> RegisterSlot<T>(GraphicsBindTypeFlags bindType, string namePrefix, uint slotIndex, GraphicsSlotBinder<T> binder)
            where T : class, IGraphicsObject
        {
            GraphicsSlot<T> slot = new GraphicsSlot<T>(this, binder, bindType, namePrefix, slotIndex);
            _slots.Add(slot);
            return slot;
        }

        public GraphicsSlotGroup<T> RegisterSlotGroup<T, B>(GraphicsBindTypeFlags bindType, string namePrefix, uint numSlots)
            where T : class, IGraphicsObject
            where B : GraphicsGroupBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlotGroup(bindType, namePrefix, numSlots, binder);
        }

        public GraphicsSlotGroup<T> RegisterSlotGroup<T>(GraphicsBindTypeFlags bindType, string namePrefix, uint numSlots, GraphicsGroupBinder<T> binder)
            where T : class, IGraphicsObject
        {
            GraphicsSlot<T>[] slots = new GraphicsSlot<T>[numSlots];
            GraphicsSlotGroup<T> grp = new GraphicsSlotGroup<T>(this, binder, slots, bindType, namePrefix);

            for (uint i = 0; i < numSlots; i++)
                slots[i] = new GraphicsSlot<T>(this, grp, bindType, namePrefix, i);

            _slots.AddRange(slots);

            return grp;
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        public void SetRenderSurfaces(params IRenderSurface2D[] surfaces)
        {
            if (surfaces == null)
                SetRenderSurfaces(null, 0);
            else
                SetRenderSurfaces(surfaces, (uint)surfaces.Length);
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">Array containing a list of render surfaces to be set.</param>
        /// <param name="count">The number of render surfaces to set.</param>
        public abstract void SetRenderSurfaces(IRenderSurface2D[] surfaces, uint count);

        /// <summary>Sets a render surface.</summary>
        /// <param name="surface">The surface to be set.</param>
        /// <param name="slot">The ID of the slot that the surface is to be bound to.</param>
        public abstract void SetRenderSurface(IRenderSurface2D surface, uint slot);

        /// <summary>
        /// Fills the provided array with a list of applied render surfaces.
        /// </summary>
        /// <param name="destinationArray">The array to fill with applied render surfaces.</param>
        public abstract void GetRenderSurfaces(IRenderSurface2D[] destinationArray);

        /// <summary>Returns the render surface that is bound to the requested slot ID. Returns null if the slot is empty.</summary>
        /// <param name="slot">The ID of the slot to retrieve a surface from.</param>
        /// <returns></returns>
        public abstract IRenderSurface2D GetRenderSurface(uint slot);

        /// <summary>
        /// Resets the render surfaces.
        /// </summary>
        public abstract void ResetRenderSurfaces();

        public abstract void SetScissorRectangle(Rectangle rect, int slot = 0);

        public abstract void SetScissorRectangles(params Rectangle[] rects);

        /// <summary>
        /// Applies the provided viewport value to the specified viewport slot.
        /// </summary>
        /// <param name="vp">The viewport value.</param>
        /// <param name="slot">The viewport slot.</param>
        public abstract void SetViewport(ViewportF vp, int slot);

        /// <summary>
        /// Applies the specified viewport to all viewport slots.
        /// </summary>
        /// <param name="vp">The viewport value.</param>
        public abstract void SetViewports(ViewportF vp);

        /// <summary>
        /// Sets the provided viewports on to their respective viewport slots. <para/>
        /// If less than the total number of viewport slots was provided, the remaining ones will be set to whatever the same value as the first viewport slot.
        /// </summary>
        /// <param name="viewports"></param>
        public abstract void SetViewports(ViewportF[] viewports);

        public abstract void GetViewports(ViewportF[] outArray);

        public abstract ViewportF GetViewport(int index);

        /// <summary>
        /// Starts a new event. Must be paired with a call to <see cref="EndEvent()"/> once finished. Events can aid debugging using the API's debugging toolset, if available.
        /// </summary>
        public abstract void BeginEvent(string label);

        /// <summary>
        /// Ends an event that was started with <see cref="BeginEvent(string)"/>. Events can aid debugging using the API's debugging toolset, if available.
        /// </summary>
        public abstract void EndEvent();

        /// <summary>
        /// Sets an API marker (if supported), to aid the use of the API's debugging toolset.
        /// </summary>
        public abstract void SetMarker(string label);

        protected BatchDrawInfo DrawInfo { get; }

        /// <summary>
        /// Gets the parent <see cref="GraphicsDevice"/> of the current <see cref="GraphicsCommandQueue"/>.
        /// </summary>
        public GraphicsDevice Device { get; }

        /// <summary>Gets the profiler bound to the current <see cref="GraphicsCommandQueue"/>. Contains statistics for this context alone.</summary>
        public RenderProfiler Profiler
        {
            get => _profiler;
            set => _profiler = value ?? _defaultProfiler;
        }

        /// <summary>
        /// Gets or sets the output depth surface.
        /// </summary>
        public GraphicsSlot<IDepthStencilSurface> DepthSurface { get; protected set; }

        public GraphicsSlotGroup<IVertexBuffer> VertexBuffers { get; protected set; }

        public GraphicsSlot<IIndexBuffer> IndexBuffer { get; protected set; }

        public GraphicsSlot<HlslShader> Shader { get; protected set; }

        public GraphicsSlotGroup<IRenderSurface2D> Surfaces { get; protected set; }
    }
}
