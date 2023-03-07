namespace Molten.Graphics
{
    public class Sprite
    {
        public delegate void OnDataChangedHandler(Sprite sprite, SpriteData data);

        public event OnDataChangedHandler OnDataChanged;

        SpriteData _data;
        float _width;
        float _height;
        Vector2F _scale = Vector2F.One;

        protected virtual void CalcDimensions()
        {
            _width = _data.Source.Width * _scale.X;
            _height = _data.Source.Height * _scale.Y;
        }

        /// <summary>
        /// Gets or sets the <see cref="SpriteData"/> source used by the current <see cref="Sprite"/>. This contains texture and style information.
        /// </summary>
        public SpriteData Data
        {
            get => _data;
            set
            {
                if(_data != value)
                {
                    if (value == null)
                        throw new Exception("Data cannot be null");

                    _data = value;
                    CalcDimensions();
                    OnDataChanged?.Invoke(this, _data);
                }
            }
        }

        /// <summary>
        /// Gets the position of the current <see cref="Sprite"/>.
        /// </summary>
        public Vector2F Position { get; set; }

        /// <summary>
        /// Gets the rotation of the current <see cref="Sprite"/>, in radians.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets the scale of the current <see cref="Sprite"/>. A scale of 1.0f is the default and original size of the sprite.
        /// </summary>
        public Vector2F Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                CalcDimensions();
            }
        }

        /// <summary>
        /// Gets the origin of the current <see cref="Sprite"/>, between 0.0f and 1.0f along each axis. 0,0 is the top-left corner of the spirte. 1,1 is the bottom-right.
        /// </summary>
        public Vector2F Origin { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Shader"/> to use when drawing the current <see cref="Sprite"/>.
        /// </summary>
        public HlslShader Shader { get; set; }

        /// <summary>
        /// Gets or sets the surface array slice to which the current <see cref="Sprite"/> should be drawn.
        /// </summary>
        public uint TargetSurfaceSlice { get; set; }

        /// <summary>
        /// Gets the width of the current <see cref="Sprite"/>.
        /// </summary>
        public float Width => _data.Source.Width;

        /// <summary>
        /// Gets the height of the current <see cref="Sprite"/>
        /// </summary>
        public float Height => _data.Source.Height;

        /// <summary>
        /// Gets the width of the current <see cref="Sprite"/> after <see cref="Scale"/> is applied.
        /// </summary>
        public float ScaledWidth => _width;

        /// <summary>
        /// Gets the height of the current <see cref="Sprite"/> after <see cref="Scale"/> is applied.
        /// </summary>
        public float ScaledHeight => _height;
    }
}
