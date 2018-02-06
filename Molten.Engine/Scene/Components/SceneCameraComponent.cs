using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>An implementation of <see cref="Camera"/> which provides a left-handed projection matrix based on it's <see cref="Camera.OutputSurface"/>.</summary>
    public class SceneCameraComponent : SceneComponent, ICamera
    {
        //public static readonly Matrix DefaultView = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);

        Vector3 _position;
        Matrix _view;
        Matrix _projection;
        Matrix _viewProjection;
        IRenderSurface _surface;
        float _nearClip = 0.1f;
        float _farClip = 100f;

        public SceneCameraComponent()
        {
            _nearClip = 0.1f;
            _farClip = 100.0f;
            _view = Matrix.Identity;
        }

        protected override void OnInitialize(SceneObject obj)
        {
            base.OnInitialize(obj);
            CalculateView();
        }

        private void _surface_OnPostResize(ITexture texture)
        {
            CalculateProjection();
            _viewProjection = Matrix.Multiply(_view, _projection);
        }

        private void CalculateProjection()
        {
            _projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, _surface.Width / (float)_surface.Height, _nearClip, _farClip);
        }

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            CalculateView();
        }

        private void CalculateView()
        {
            _view = Matrix.Invert(Object.Transform.GlobalTransform);

            Vector3 scale;
            Quaternion rot;
            Object.Transform.GlobalTransform.Decompose(out scale, out rot, out _position);
        }

        //public void SetView(Vector3 position, Vector3 lookAtPosition, Vector3 upAxis)
        //{
        //    _view = Matrix.LookAtLH(position, lookAtPosition, upAxis);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="position">The camera's position.</param>
        ///// <param name="xRotation">The Z-axis rotation, in degrees.</param>
        ///// <param name="yRotation">The Z-axis rotation, in degrees.</param>
        ///// <param name="zRotation">The Z-axis rotation, in degrees.</param>
        ///// <param name="upAxis">The up axis.</param>
        //public void SetView(Vector3 position, float xRotation, float yRotation, float zRotation, Vector3 upAxis, Vector3 forwardAxis)
        //{
        //    SetView(position, new Vector3(xRotation, yRotation, zRotation), upAxis, forwardAxis);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="position">The camera's position.</param>
        ///// <param name="rotation">The XYZ rotation, in degrees.</param>
        ///// <param name="upAxis">The up axis.</param>
        //public void SetView(Vector3 position, Vector3 rotation, Vector3 upAxis, Vector3 forwardAxis)
        //{
        //    Vector3 leftAxis = -Vector3.Cross(forwardAxis, upAxis);
        //    Quaternion qRot = Quaternion.RotationAxis(leftAxis, MathHelper.DegreesToRadians(rotation.X)) *
        //        Quaternion.RotationAxis(upAxis, MathHelper.DegreesToRadians(rotation.Y)) *
        //        Quaternion.RotationAxis(forwardAxis, MathHelper.DegreesToRadians(rotation.Z));

        //    SetView(position, qRot);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="position">The camera's position.</param>
        ///// <param name="rotation">The rotation quaternion.</param>
        ///// <param name="upAxis">The up axis.</param>
        //public void SetView(Vector3 position, Quaternion rotation)
        //{
        //    _view = Matrix.FromQuaternion(rotation) * Matrix.CreateTranslation(position);
        //    _view.Invert();
        //    _viewProjection = Matrix.Multiply(_view, _projection);
        //}

        /// <summary>Converts the provided screen position to a globalized 3D world position.</summary>
        /// <param name="location">The screen position.</param>
        /// <returns></returns>
        public Vector3 ConvertScreenToWorld(Vector2 location)
        {
            Vector4 result = Vector2.Transform(location, Object.Transform.GlobalTransform);
            return new Vector3(result.X, result.Y, result.Z);
        }

        public Vector2 ConvertWorldToScreen(Vector3 position)
        {
            Vector4 result = Vector3.Transform(position, _view);
            return new Vector2(result.X, result.Y);
        }

        public Matrix View => _view;

        public Matrix Projection => _projection;

        public Matrix ViewProjection => _viewProjection;

        /// <summary>Gets the camera's current position.</summary>
        public Vector3 Position => _position;

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
