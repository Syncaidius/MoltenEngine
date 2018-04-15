using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Camera2D : ICamera
    {
        public static readonly Matrix4F DefaultView = Matrix4F.Identity;

        protected Matrix4F _view;
        protected Matrix4F _projection;
        protected Matrix4F _viewProjection;
        protected IRenderSurface _surface;
        protected float _nearClip = 0.1f;
        protected float _farClip = 1000f;
        protected Vector3F _position;

        public Camera2D()
        {
            View = DefaultView;
            _nearClip = 0.0f;
            _farClip = 1.0f;
        }

        protected void CalculateProjection()
        {
            _projection = Matrix4F.OrthoOffCenterLH(0, _surface.Width, -_surface.Height, 0, _nearClip, _farClip);
            _viewProjection = Matrix4F.Multiply(View, _projection);
        }

        private void _surface_OnPostResize(ITexture texture)
        {
            CalculateProjection();
        }

        /// <summary>Gets or sets the camera's view matrix.</summary>
        public virtual Matrix4F View
        {
            get => _view;
            set
            {
                _view = value;
                _viewProjection = Matrix4F.Multiply(_view, _projection);
            }
        }

        /// <summary>Gets the camera's projection matrix. The projection is ignored during rendering if <see cref="OutputSurface"/> is not set.</summary>
        public Matrix4F Projection => _projection;

        /// <summary>Gets the camera's combined view-projection matrix. This is the result of the view matrix multiplied by the projection matrix.</summary>
        public Matrix4F ViewProjection => _viewProjection;

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

        /// <summary>Gets or sets the minimum draw dinstance. Also known as the near-clip plane. 
        /// Anything closer this value will not be drawn.</summary>
        public float MinimumDrawDistance
        {
            get => _nearClip;
            set
            {
                _nearClip = value;
                CalculateProjection();
            }
        }

        /// <summary>Gets or sets the maximum draw distance. Also known as the far-clip plane. 
        /// Anything further away than this value will not be drawn.</summary>
        public float MaximumDrawDistance
        {
            get => _farClip;
            set
            {
                _farClip = value;
                CalculateProjection();
            }
        }

        /// <summary>
        /// Gets or sets the field-of-view. Has no effect on a 2D camera.
        /// </summary>
        public float FieldOfView
        {
            get => 0;
            set { }
        }

        public Vector3F Position => _position;
    }
}
