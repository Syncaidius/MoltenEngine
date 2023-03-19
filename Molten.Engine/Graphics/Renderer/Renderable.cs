namespace Molten.Graphics
{
    public abstract class Renderable
    {
        GraphicsResource[] _resources;

        protected Renderable(RenderService renderer)
        {
            Renderer = renderer;
            IsVisible = false;
            _resources = new GraphicsResource[0];
        }

        /// <summary>Applies a shader resource to the renderable at the specified slot.</summary>
        /// <param name="resource">The resource.</param>
        /// <param name="slot">The slot ID.</param>
        public void SetResource(IGraphicsResource resource, uint slot)
        {
            if (slot >= Renderer.Device.Adapter.Capabilities.PixelShader.MaxInResources)
                throw new IndexOutOfRangeException("The maximum slot number must be less than the maximum supported by the graphics device.");

            if (slot >= _resources.Length)
                EngineUtil.ArrayResize(ref _resources, slot + 1U);

            _resources[slot] = resource as GraphicsResource;
        }

        /// <summary>Gets the shader resource applied to the mesh at the specified slot.</summary>
        /// <param name="slot">The slot ID.</param>
        /// <returns>An <see cref="GraphicsResource"/> that was applied at the specified slot.</returns>
        public GraphicsResource GetResource(uint slot)
        {
            if (slot >= _resources.Length)
                return null;
            else
                return _resources[slot];
        }

        protected void ApplyResources(HlslShader shader)
        {
            // Set as many custom resources from the renderable as possible, or use the material's default when needed.
            for (uint i = 0; i < _resources.Length; i++)
                shader.Resources[i].Value = _resources[i] ?? shader.DefaultResources[i];

            for (uint i = (uint)_resources.Length; i < shader.Resources.Length; i++)
                shader.Resources[i].Value = shader.DefaultResources[i];
        }

        internal bool BatchRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, RenderDataBatch batch)
        {
            return OnBatchRender(cmd, renderer, camera, batch);
        }

        internal void Render(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            OnRender(cmd, renderer, camera, data);
        }

        protected virtual bool OnBatchRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, RenderDataBatch batch)
        {
            return false;
        }

        protected abstract void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data);

        /// <summary>Gets or sets whether or not the renderable should be drawn.</summary>
        public bool IsVisible { get; set; }

        internal RenderService Renderer { get; private set; }
    }
}
