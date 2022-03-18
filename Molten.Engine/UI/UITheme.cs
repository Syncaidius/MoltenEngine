using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UITheme
    {
        [DataMember]
        public Color TextColor { get; set; } = Color.White;

        [DataMember]
        public Color BackgroundColor { get; set; } = new Color(40, 40, 150, 200);

        [DataMember]
        public Color BorderColor { get; set; } = new Color(80, 80, 190);

        [DataMember]
        public float BorderThickness { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the default font path, or name of a system font.
        /// </summary>
        [DataMember]
        public string FontName
        {
            get => _fontName;
            set
            {
                if(_fontName != value)
                {
                    _fontName = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default font size.
        /// </summary>
        [DataMember]
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if(_fontSize != value)
                {
                    _fontSize = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default size (in spaces) of the a tab character.
        /// </summary>
        [DataMember]
        public int TabSize
        {
            get => _tabSize;
            set
            {
                if (_tabSize != value)
                {
                    _tabSize = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default font texture sheet size.
        /// </summary>
        [DataMember]
        public int FontTextureSize
        {
            get => _texSize;
            set
            {
                if (_texSize != value)
                {
                    _texSize = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default number of points per curve, when rendering font characters.
        /// </summary>
        [DataMember]
        public int FontPointsPerCurve
        {
            get => _pointsPerCurve;
            set
            {
                if (_pointsPerCurve != value)
                {
                    _pointsPerCurve = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default font character padding.
        /// </summary>
        [DataMember]
        public int CharacterPadding
        {
            get => _charPadding;
            set
            {
                if (_charPadding != value)
                {
                    _charPadding = value;
                    _initialized = false;
                }
            }
        }

        public SpriteFont Font { get; private set; }

        internal bool IsInitialized => _initialized;

        bool _initialized;
        string _fontName;
        int _fontSize = 16;
        int _tabSize = 3;
        int _texSize = 512;
        int _pointsPerCurve = 12;
        int _charPadding = 2;

        /// <summary>
        /// Invoked when the current <see cref="UITheme"/> is set on a <see cref="UIRenderComponent"/>, or before UI is rendered.
        /// </summary>
        /// <param name="engine"></param>
        internal void Initialize(Engine engine)
        {
            if (_initialized)
                return;

            Font = engine.Fonts.GetFont(engine.Log, _fontName, _fontSize, _tabSize, _texSize, _pointsPerCurve, 1, _charPadding);
        }
    }
}
