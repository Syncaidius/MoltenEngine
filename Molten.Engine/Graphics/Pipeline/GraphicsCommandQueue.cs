namespace Molten.Graphics
{
    public abstract class GraphicsCommandQueue : EngineObject
    {
        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;
        List<GraphicsSlot> _slots;

        protected GraphicsCommandQueue(GraphicsDevice device)
        {
            Device = device;
            _slots = new List<GraphicsSlot>();
            _defaultProfiler = _profiler = new RenderProfiler();
        }

        public abstract GraphicsCommandList GetList(GraphicsCommandListType type);

        public abstract void Submit(bool waitForCompletion, Action CompletionCallback, params GraphicsCommandList[] cmd);

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

        public GraphicsSlotGroup<GraphicsBuffer> VertexBuffers { get; protected set; }

        public GraphicsSlot<GraphicsBuffer> IndexBuffer { get; protected set; }

        public GraphicsSlot<HlslShader> Shader { get; protected set; }

        public GraphicsSlotGroup<IRenderSurface2D> Surfaces { get; protected set; }
    }
}
