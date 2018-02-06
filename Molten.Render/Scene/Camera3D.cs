using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>An implementation of <see cref="Camera"/> which provides a left-handed projection matrix based on it's <see cref="Camera.OutputSurface"/>.</summary>
    public class Camera3D : Camera
    {
        public static readonly Matrix DefaultView = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);

        public Camera3D()
        {
            _nearClip = 0.1f;
            _farClip = 100.0f;
            _view = DefaultView;
        }

        protected override void CalculateProjection()
        {
            _projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, _surface.Width / (float)_surface.Height, _nearClip, _farClip);
        }

        public void SetView(Vector3 position, Vector3 lookAtPosition, Vector3 upAxis)
        {
            _view = Matrix.LookAtLH(position, lookAtPosition, upAxis);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The camera's position.</param>
        /// <param name="xRotation">The Z-axis rotation, in degrees.</param>
        /// <param name="yRotation">The Z-axis rotation, in degrees.</param>
        /// <param name="zRotation">The Z-axis rotation, in degrees.</param>
        /// <param name="upAxis">The up axis.</param>
        public void SetView(Vector3 position, float xRotation, float yRotation, float zRotation, Vector3 upAxis, Vector3 forwardAxis)
        {
            SetView(position, new Vector3(xRotation, yRotation, zRotation), upAxis, forwardAxis);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The camera's position.</param>
        /// <param name="rotation">The XYZ rotation, in degrees.</param>
        /// <param name="upAxis">The up axis.</param>
        public void SetView(Vector3 position, Vector3 rotation, Vector3 upAxis, Vector3 forwardAxis)
        {
            Vector3 leftAxis = -Vector3.Cross(forwardAxis, upAxis);
            Quaternion qRot = Quaternion.RotationAxis(leftAxis, MathHelper.DegreesToRadians(rotation.X)) *
                Quaternion.RotationAxis(upAxis, MathHelper.DegreesToRadians(rotation.Y)) *
                Quaternion.RotationAxis(forwardAxis, MathHelper.DegreesToRadians(rotation.Z));

            SetView(position, qRot);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The camera's position.</param>
        /// <param name="rotation">The rotation quaternion.</param>
        /// <param name="upAxis">The up axis.</param>
        public void SetView(Vector3 position, Quaternion rotation)
        {
            _view = Matrix.FromQuaternion(rotation) * Matrix.CreateTranslation(position);
            _view.Invert();
            _viewProjection = Matrix.Multiply(_view, _projection);
        }
    }
}
