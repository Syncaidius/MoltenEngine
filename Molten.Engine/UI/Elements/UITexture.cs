using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UITexture : UIElement
    {
        ITexture2D _texture;
        RectStyle _style = RectStyle.Default;
        Rectangle _texBounds;
        UIFillType _fillType;

        private void AlignTexture()
        {
            if (_texture == null)
                return;

            switch (_fillType)
            {
                case UIFillType.Stretch:
                    _texBounds = GlobalBounds;
                    break;

                case UIFillType.Center:
                    Rectangle gb = GlobalBounds;
                    int w =  (int)_texture.Width;
                    int h = (int)_texture.Height;

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

            if(_texture != null)
                sb.Draw(_texBounds, ref _style, _texture, null, ArraySlice, 0);
        }

        /// <summary>
        /// Gets or sets the <see cref="ITexture2D"/> to be rendered in the current <see cref="UITexture"/>
        /// </summary>
        public ITexture2D Texture
        {
            get => _texture;
            set
            {
                if (_texture != value)
                {
                    _texture = value;
                    AlignTexture();
                }
            }
        }

        /// <summary>
        /// Gets or sets the texture array slice to be rendered in the current <see cref="UITexture"/>.
        /// </summary>
        public uint ArraySlice { get; set; }

        /// <summary>
        /// Gets or sets the texture fill type.
        /// </summary>
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
