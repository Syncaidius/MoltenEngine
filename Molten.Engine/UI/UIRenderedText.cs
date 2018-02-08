using Molten.IO;
using Molten.Graphics;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public delegate void UIRenderedTextHandler(UIRenderedText text);

    [DataContract]
    public class UIRenderedText : EngineObject
    {
        ISpriteFont _font;
        Color _color = new Color(255,255,255,255);
        Color _shadowColor = new Color(0,0,0,255);
        int _fontSize;
        string _fontName;
        string _text = "Text";
        bool _shadowsEnabled = false;
        bool _isFontDirty = true;
        bool _isTextDirty = false;

        Vector2 _textSize;
        Vector2 _textPos;
        Rectangle _bounds;
        Engine _engine;

        SpriteFontWeight _weight = SpriteFontWeight.Regular;
        SpriteFontStyle _style = SpriteFontStyle.Normal;
        SpriteFontStretch _stretch = SpriteFontStretch.Normal;
        UIHorizontalAlignment _hAlignment;
        UIVerticalAlignment _vAlignment;

        public event UIRenderedTextHandler OnChanged;

        /// <summary>Creates a new instance of <see cref="UIRenderedText"/>.</summary>
        /// <param name="ui"></param>
        public UIRenderedText(Engine engine)
        {
            _engine = engine;
            _bounds = new Rectangle(0, 0, 100, 100);

            _fontName = engine.Settings.DefaultFontName;
            _fontSize = engine.Settings.DefaultFontSize;

            _hAlignment = UIHorizontalAlignment.Left;
            _vAlignment = UIVerticalAlignment.Top;

            RefreshFont();
        }

        /// <summary>Immediately applies any font or text changes.</summary>
        public void Refresh()
        {
            if (_isFontDirty)
                RefreshFont();
            else if (_isTextDirty)
                RefreshText();
        }

        /// <summary>Applies any pending changes to the font.</summary>
        public void RefreshFont()
        {
            _isFontDirty = false;
            _font = _engine.Renderer.Resources.CreateFont(_fontName, _fontSize, _weight, _stretch, _style);
            RefreshText();
        }

        private void RefreshText()
        {
            _isTextDirty = false;
            _textSize = _font.MeasureString(_text);
            _textPos = new Vector2(_bounds.X, _bounds.Y);

            switch (_hAlignment)
            {
                case UIHorizontalAlignment.Center:
                    _textPos.X = _bounds.Center.X - (_textSize.X / 2);
                    break;

                case UIHorizontalAlignment.Right:
                    _textPos.X = _bounds.Right - _textSize.X;
                    break;
            }

            switch (_vAlignment)
            {
                case UIVerticalAlignment.Center:
                    _textPos.Y = _bounds.Center.Y - (_textSize.Y / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    _textPos.Y = _bounds.Bottom - _textSize.Y;
                    break;
            }
        }

        public void Draw(ISpriteBatch sb)
        {
            Refresh();

            if (_shadowsEnabled)
                sb.DrawString(_font, _text, _textPos + new Vector2(3), _shadowColor);

            if (_text != null)
                sb.DrawString(_font, _text, _textPos, _color);
        }

        protected override void OnDispose()
        {
            // TODO log de-reference of current font (needs usage tracking in SpriteFont).
        }

        public Vector2 GetSize()
        {
            return _textSize;
        }

        public Vector2 GetSize(int maxLength)
        {
            if (_isFontDirty)
                RefreshFont();

            return _font.MeasureString(_text, maxLength);
        }

        public Rectangle GetSize(int startIndex, int length)
        {
            if (_isFontDirty)
                RefreshFont();

            return _font.MeasureString(_text, startIndex, length);
        }

        public int NearestCharacter(Vector2 location)
        {
            return _font.NearestCharacter(_text, location);
        }

        public static implicit operator string(UIRenderedText text)
        {
            return text._text;
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Font Name")]
        /// <summary>Gets or sets the name of the font to be used.</summary>
        public string FontName
        {
            get { return _font.FontName; }
            set
            {
                _fontName = value;
                _isFontDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Text")]
        /// <summary>Gets or sets the string of text to be drawn.</summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                if (OnChanged != null)
                    OnChanged(this);

                _isTextDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Font Size")]
        /// <summary>Gets or sets the font size.</summary>
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                _isFontDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Weight")]
        /// <summary>Gets or sets the weight of the rendered text (e.g. bold, black, etc).</summary>
        public SpriteFontWeight Weight
        {
            get { return _weight; }
            set
            {
                _weight = value;
                _isFontDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Font Style")]
        /// <summary>Gets or sets the style of the rendered text. </summary>
        public SpriteFontStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                _isFontDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Font Stretch")]
        /// <summary>Gets or sets the stretching mode.</summary>
        public SpriteFontStretch Stretch
        {
            get { return _stretch; }
            set
            {
                _stretch = value;
                _isFontDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Color")]
        /// <summary>Gets or sets the color of the text.</summary>
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Shadow Color")]
        /// <summary>Gets or sets the color of the text.</summary>
        public Color ShadowColor
        {
            get { return _shadowColor; }
            set { _shadowColor = value; }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Horizontal Alignment")]
        public UIHorizontalAlignment HorizontalAlignment
        {
            get { return _hAlignment; }
            set
            {
                _hAlignment = value;
                _isTextDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Vertical Alignment")]
        public UIVerticalAlignment VerticalAlignment
        {
            get { return _vAlignment; }
            set
            {
                _vAlignment = value;
                _isTextDirty = true;
            }
        }

        [DataMember]
        [Category("Appearance")]
        [DisplayName("Shadows Enabled")]
        public bool ShadowsEnabled
        {
            get { return _shadowsEnabled; }
            set { _shadowsEnabled = value; }
        }

        [Browsable(false)]
        public int Length
        {
            get { return _text != null ?  _text.Length : 0;}
        }

        [Browsable(false)]
        public ISpriteFont Font
        {
            get { return _font; }
        }

        [DataMember]
        /// <summary>Gets or sets the bounds of the rendered text area.</summary>
        [Browsable(false)]
        public Rectangle Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = value;
                _isTextDirty = true;
            }
        }

        /// <summary>Gets the actual position of the rendered text.</summary>
        [Browsable(false)]
        public Vector2 ActualPosition
        {
            get { return _textPos; }
        }
    }
}
