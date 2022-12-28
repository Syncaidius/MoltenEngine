namespace Molten.Graphics
{
    public abstract class Renderable : IRenderable
    {
        IShaderResource[] _resources;

        internal Renderable(DeviceDX11 device)
        {
            Device = device;
            IsVisible = false;
            _resources = new IShaderResource[0];
        }

        public void SetResource(IShaderResource resource, uint slot)
        {
            if (slot >= Device.Adapter.Capabilities.PixelShader.MaxInResources)
                throw new IndexOutOfRangeException("The maximum slot number must be less than the maximum supported by the graphics device.");

            if (slot >= _resources.Length)
                EngineUtil.ArrayResize(ref _resources, slot + 1U);

            _resources[slot] = resource;
        }

        public IShaderResource GetResource(uint slot)
        {
            if (slot >= _resources.Length)
                return null;
            else
                return _resources[slot];
        }

        protected void ApplyResources(Material material)
        {
            uint resCount = (uint)Math.Min(_resources.Length, material.Resources.Length);

            // Set as many custom resources from the renderable as possible, or use the material's default when needed.
            for (uint i = 0; i < _resources.Length; i++)
                material.Resources[i].Value = _resources[i] ?? material.DefaultResources[i];

            for (uint i = (uint)_resources.Length; i < material.Resources.Length; i++)
                material.Resources[i].Value = material.DefaultResources[i];
        }

        internal void Render(DeviceContext pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            pipe.State.DepthWriteOverride = data.DepthWriteOverride;
            OnRender(pipe, renderer, camera, data);
        }

        private protected abstract void OnRender(DeviceContext context, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data);

        /// <summary>Gets or sets whether or not the renderable should be drawn.</summary>
        public bool IsVisible { get; set; }

        internal DeviceDX11 Device { get; private set; }
    }
}
