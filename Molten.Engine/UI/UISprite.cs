using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UISprite : UIElement
    {
        Sprite _sprite;
        RectStyle _style = RectStyle.Default;
        Vector2F _spriteScale;
        Vector2F _spritePos;
        UIFillType _fillType = UIFillType.Center;

        private void AlignSprite()
        {
            if (_sprite == null)
                return;

            Rectangle gb = GlobalBounds;

            switch (_fillType)
            {
                case UIFillType.Stretch:
                    _spriteScale = new Vector2F()
                    {
                        X = gb.Width / _sprite.ScaledWidth,
                        Y = gb.Height / _sprite.ScaledHeight
                    };
                    break;

                case UIFillType.Center:
                    _spriteScale = new Vector2F()
                    {
                        X = gb.Width / _sprite.ScaledWidth,
                        Y = gb.Height / _sprite.ScaledHeight
                    };

                    _spritePos = (Vector2F)gb.Center;
                    break;
            }
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();
            AlignSprite();
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            // Override the sprite's draw position and scale
            sb.Draw(_sprite.Data.Texture,
                _sprite.Data.Source,
                _spritePos,
                (_sprite.Data.Source.Size * _sprite.Scale) * _spriteScale,
                _sprite.Rotation,
                _sprite.Origin,
                ref _sprite.Data.Style,
                _sprite.Material,
                _sprite.Data.ArraySlice,
                _sprite.TargetSurfaceSlice);
        }

        /// <summary>
        /// Gets or sets the <see cref="Molten.Graphics.Sprite"/> to be rendered in the current <see cref="UISprite"/>.
        /// </summary>
        public Sprite Sprite
        {
            get => _sprite;
            set
            {
                if (_sprite != value)
                {
                    _sprite = value;
                    AlignSprite();
                }
            }
        }

        /// <summary>
        /// Gets or sets the sprite fill type for the current <see cref="UISprite"/>.
        /// </summary>
        public UIFillType FillType
        {
            get => _fillType;
            set
            {
                if(_fillType != value)
                {
                    _fillType = value;
                    AlignSprite();
                }
            }
        }
    }
}
