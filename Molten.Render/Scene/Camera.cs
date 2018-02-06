using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A base class for custom camera implementations.</summary>
    public abstract class Camera
    {
        protected Matrix _view;
        protected Matrix _projection;
        protected Matrix _viewProjection;
        protected IRenderSurface _surface;
        protected float _nearClip = 0.1f;
        protected float _farClip = 100f;

        protected abstract void CalculateProjection();

        private void _surface_OnPostResize(ITexture texture)
        {
            CalculateProjection();
            _viewProjection = Matrix.Multiply(_view, _projection);
        }

        /// <summary>Gets or sets the camera's view matrix.</summary>
        public virtual Matrix View
        {
            get => _view;
            set
            {
                _view = value;
                _viewProjection = Matrix.Multiply(_view, _projection);
            }
        }

        /// <summary>Gets the camera's projection matrix.</summary>
        public Matrix Projection => _projection;

        /// <summary>Gets the camera's combined view-projection matrix. This is the result of the view matrix multiplied by the projection matrix.</summary>
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
                    CalculateProjection();
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
