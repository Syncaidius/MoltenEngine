using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Camera3D : Camera
    {
        public static readonly Matrix DefaultView = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);

        public Camera3D()
        {
            _nearClip = 0.1f;
            _farClip = 100.0f;
            View = DefaultView;
        }

        protected override void CalculateViewProjection()
        {
            _projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, _surface.Width / (float)_surface.Height, _nearClip, _farClip);
            _viewProjection = Matrix.Multiply(View, _projection);
        }
    }
}
