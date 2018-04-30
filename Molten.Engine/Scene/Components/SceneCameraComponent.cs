using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>An implementation of <see cref="Camera"/> which provides a left-handed projection matrix based on it's <see cref="Camera.OutputSurface"/>.</summary>
    public class SceneCameraComponent : SceneComponent, ICamera
    {
        Matrix4F _view;
        Matrix4F _projection;
        Matrix4F _viewProjection;
        IRenderSurface _surface;
        float _nearClip;
        float _farClip;
        float _fov;

        public SceneCameraComponent()
        {
            _nearClip = 0.1f;
            _farClip = 1000.0f;
            _view = Matrix4F.Identity;
            _fov = (float)Math.PI / 4.0f;
        }

        protected override void OnInitialize(SceneObject obj)
        {
            base.OnInitialize(obj);
            CalculateView();
        }

        private void _surface_OnPostResize(ITexture texture)
        {
            CalculateProjection();
            _viewProjection = Matrix4F.Multiply(_view, _projection);
        }

        private void CalculateProjection()
        {
            float width = 10;
            float height = 10;

            if (_surface != null)
            {
                width = _surface.Width;
                height = _surface.Height;
            }

            _projection = Matrix4F.PerspectiveFovLH(_fov, width / height, _nearClip, _farClip);
        }

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            CalculateProjection();
            CalculateView();
        }

        private void CalculateView()
        {
            _view = Matrix4F.Invert(Object.Transform.Global);
            _viewProjection = Matrix4F.Multiply(_view, _projection);
        }

        /// <summary>Converts the provided screen position to a globalized 3D world position.</summary>
        /// <param name="location">The screen position.</param>
        /// <returns></returns>
        public Vector3F ConvertScreenToWorld(Vector2F location)
        {
            Vector4F result = Vector2F.Transform(location, Object.Transform.Global);
            return new Vector3F(result.X, result.Y, result.Z);
        }

        public Vector2F ConvertWorldToScreen(Vector3F position)
        {
            Vector4F result = Vector3F.Transform(position, _view);
            return new Vector2F(result.X, result.Y);
        }

        public Matrix4F View => _view;

        public Matrix4F Projection => _projection;

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

        /// <summary>Gets or sets the camera's depth surface.</summary>
        public IDepthSurface OutputDepthSurface { get; set; }

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
            set =>_farClip = value;
        }

        /// <summary>
        /// Gets or sets the camera's field-of-view (FoV), in radians.
        /// </summary>
        public float FieldOfView
        {
            get => _fov;
            set => _fov = value;
        }

        public Vector3F Position => Object.Transform.GlobalPosition;
    }
}
