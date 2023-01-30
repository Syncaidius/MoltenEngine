namespace Molten.Graphics
{
    public abstract class Renderable : IRenderable
    {
        IShaderResource[] _resources;

        protected Renderable(GraphicsDevice device)
        {
            Device = device;
            IsVisible = false;
            _resources = new IShaderResource[0];
        }

        /// <summary>Applies a shader resource to the mesh at the specified slot.</summary>
        /// <param name="resource">The resource.</param>
        /// <param name="slot">The slot ID.</param>
        public void SetResource(IShaderResource resource, uint slot)
        {
            if (slot >= Device.Adapter.Capabilities.PixelShader.MaxInResources)
                throw new IndexOutOfRangeException("The maximum slot number must be less than the maximum supported by the graphics device.");

            if (slot >= _resources.Length)
                EngineUtil.ArrayResize(ref _resources, slot + 1U);

            _resources[slot] = resource;
        }

        /// <summary>Gets the shader resource applied to the mesh at the specified slot.</summary>
        /// <param name="slot">The slot ID.</param>
        /// <returns>An <see cref="IShaderResource"/> that was applied at the specified slot.</returns>
        public IShaderResource GetResource(uint slot)
        {
            if (slot >= _resources.Length)
                return null;
            else
                return _resources[slot];
        }

        protected void ApplyResources(Material material)
        {
            // Set as many custom resources from the renderable as possible, or use the material's default when needed.
            for (uint i = 0; i < _resources.Length; i++)
                material.Resources[i].Value = _resources[i] ?? material.DefaultResources[i];

            for (uint i = (uint)_resources.Length; i < material.Resources.Length; i++)
                material.Resources[i].Value = material.DefaultResources[i];
        }

        public void Render(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            cmd.DepthWriteOverride = data.DepthWriteOverride;
            OnRender(cmd, renderer, camera, data);
        }

        protected abstract void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data);

        /// <summary>Gets or sets whether or not the renderable should be drawn.</summary>
        public bool IsVisible { get; set; }

        internal GraphicsDevice Device { get; private set; }
    }
}
