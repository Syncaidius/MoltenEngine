using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Camera2D : Camera
    {
        public static readonly Matrix4F DefaultView = Matrix4F.Identity;

        public Camera2D()
        {
            View = DefaultView;
            _nearClip = 0.0f;
            _farClip = 1.0f;
        }

        protected override void CalculateProjection()
        {
            _projection = Matrix4F.OrthoOffCenterLH(0, _surface.Width, -_surface.Height, 0, _nearClip, _farClip);
            _viewProjection = Matrix4F.Multiply(View, _projection);
        }

        public static Matrix4F GetDefaultProjection(IRenderSurface surface, float nearClip = 0.0f, float farClip = 1.0f)
        {
            return Matrix4F.OrthoOffCenterLH(0, surface.Width, -surface.Height, 0, nearClip, farClip);
        }
    }
}
