using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class Renderable : IRenderable
    {
        IShaderResource[] _resources;

        internal Renderable(GraphicsDeviceDX11 device)
        {
            Device = device;
            IsVisible = false;
            _resources = new IShaderResource[0];
        }

        public void SetResource(IShaderResource resource, int slot)
        {
            if (slot >= Device.Features.MaxInputResourceSlots)
                throw new IndexOutOfRangeException("The maximum slot number must be less than the maximum supported by the graphics device.");

            if (slot >= _resources.Length)
                Array.Resize(ref _resources, slot + 1);

            _resources[slot] = resource;
        }

        public IShaderResource GetResource(int slot)
        {
            if (slot >= _resources.Length)
                return null;
            else
                return _resources[slot];
        }

        protected void ApplyResources(Material material)
        {
            int resCount = Math.Min(_resources.Length, material.Resources.Length);

            // Set as many custom resources from the renderable as possible, or use the material's default when needed.
            for (int i = 0; i < _resources.Length; i++)
                material.Resources[i].Value = _resources[i] ?? material.DefaultResources[i];

            for (int i = _resources.Length; i < material.Resources.Length; i++)
                material.Resources[i].Value = material.DefaultResources[i];
        }

        internal void Render(GraphicsPipe pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            pipe.DepthWriteOverride = data.DepthWriteOverride;
            OnRender(pipe, renderer, camera, data);
        }

        private protected abstract void OnRender(GraphicsPipe pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data);

        /// <summary>Gets or sets whether or not the renderable should be drawn.</summary>
        public bool IsVisible { get; set; }

        internal GraphicsDeviceDX11 Device { get; private set; }
    }
}
