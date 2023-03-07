using Molten.Graphics;

namespace Molten.UI
{
    public class UISprite : UIElement
    {
        static Vector2F _spriteOrigin = new Vector2F(0.5f);

        Sprite _sprite;
        RectStyle _style = RectStyle.Default;
        Vector2F _spriteScale;
        Vector2F _spritePos;
        UIFillType _fillType = UIFillType.Fit;

        private void AlignSprite()
        {
            if (_sprite == null)
                return;

            Rectangle gb = GlobalBounds;

            switch (_fillType)
            {
                case UIFillType.Fit:
                    _spriteScale = new Vector2F()
                    {
                        X = gb.Width / _sprite.ScaledWidth,
                        Y = gb.Height / _sprite.ScaledHeight
                    };

                    _spritePos = (Vector2F)gb.Center;
                    break;

                case UIFillType.Center:
                    _spriteScale = Vector2F.One;
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

            // Override the sprite's draw position, origin and scale
            if (_sprite != null)
            {
                sb.Draw(_sprite.Data.Texture,
                    _sprite.Data.Source,
                    _spritePos,
                    (_sprite.Data.Source.Size * _sprite.Scale) * _spriteScale,
                    _sprite.Rotation,
                    _spriteOrigin,
                    ref _sprite.Data.Style,
                    _sprite.Shader,
                    _sprite.Data.ArraySlice,
                    _sprite.TargetSurfaceSlice);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Molten.Graphics.Sprite"/> to be rendered in the current <see cref="UISprite"/>.
        /// <para>The <see cref="Sprite"/> position and origin are ignored. Scale is also ignored if <see cref="FillType"/> is not <see cref="UIFillType.Center"/>.</para>
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
        [UIThemeMember]
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
