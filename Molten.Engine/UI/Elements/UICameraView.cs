using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UICameraView : UIElement
    {
        CameraComponent _camera;
        RectStyle _style = RectStyle.Default;
        Rectangle _texBounds;
        UIFillType _fillType;

        private void AlignTexture()
        {
            if (_camera == null)
                return;

            switch (_fillType)
            {
                case UIFillType.Fit:
                    _texBounds = GlobalBounds;
                    break;

                case UIFillType.Center:
                    Rectangle gb = GlobalBounds;
                    int w =  (int)_camera.Surface.Width;
                    int h = (int)_camera.Surface.Height;

                    _texBounds = new Rectangle()
                    {
                         X = gb.Center.X - (w / 2),
                         Y = gb.Center.Y - (h / 2),
                         Width = w,
                         Height = h,
                    };
                    break;
            }
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();
            AlignTexture();
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            if(_camera != null && _camera.Surface != null)
                sb.Draw(_texBounds, ref _style, _camera.Surface, null, ArraySlice, 0);
        }

        /// <summary>
        /// Gets or sets the <see cref="ITexture2D"/> to be rendered in the current <see cref="UITexture"/>
        /// </summary>
        public CameraComponent Camera
        {
            get => _camera;
            set
            {
                if (_camera != value)
                {
                    _camera = value;
                    AlignTexture();
                }
            }
        }

        /// <summary>
        /// Gets or sets the texture array slice to be rendered in the current <see cref="UITexture"/>.
        /// </summary>
        [UIThemeMember]
        public uint ArraySlice { get; set; }

        /// <summary>
        /// Gets or sets the texture fill type.
        /// </summary>
        [UIThemeMember]
        public UIFillType FillType
        {
            get => _fillType;
            set
            {
                if(_fillType != value)
                {
                    _fillType = value;
                    AlignTexture();
                }
            }
        }
    }
}
