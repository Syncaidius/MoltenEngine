using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class Camera
    {
        protected Matrix _projection;
        protected Matrix _viewProjection;
        protected IRenderSurface _surface;
        protected float _nearClip = 0.1f;
        protected float _farClip = 100f;

        protected abstract void CalculateViewProjection();

        private void _surface_OnPostResize(ITexture texture)
        {
            CalculateViewProjection();
        }

        /// <summary>[TEMP]Gets or sets the camera's view matrix.</summary>
        public Matrix View;

        public Matrix Projection => _projection;

        public Matrix ViewProjection => _viewProjection;

        /// <summary>Gets or sets the <see cref="IRenderSurface"/> that the camera's view should be rendered out to.</summary>
        public IRenderSurface OutputSurface
        {
            get => _surface;
            set
            {
                if (_surface != value)
                {
                    if (_surface != null)
                        _surface.OnPostResize -= _surface_OnPostResize;

                    if (value != null)
                        value.OnPostResize += _surface_OnPostResize;

                    _surface = value;
                    CalculateViewProjection();
                }
            }
        }

        public IDepthSurface OutputDepthSurface { get; set; }

        /// <summary>Gets or sets the minimum draw dinstance. Also known as the near-clip plane. 
        /// Anything closer this value will not be drawn.</summary>
        public float MinimumDrawDistance
        {
            get => _nearClip;
            set => _nearClip = value;
        }

        /// <summary>Gets or sets the maximum draw distance. Also known as the far-clip plane. 
        /// Anything further away than this value will not be drawn.</summary>
        public float MaximumDrawDistance
        {
            get => _farClip;
            set => _farClip = value;
        }
    }
}
