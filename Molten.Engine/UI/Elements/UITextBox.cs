using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Graphics;
using static Molten.UI.UITextBox;

namespace Molten.UI
{
    public class UITextBox : UIElement
    {
        public class Line
        {
            public Segment First;

            public uint LineNumber;

            public Vector2F LineNumberSize;
        }

        public class Segment
        {
            public SpriteFont Font;

            public string Text;

            public Vector2F MeasuredSize;

            public Segment Previous;

            public Segment Next;

            public Color Color = Color.White;
        }

        bool _showLineNumbers;
        bool _isMultiline;
        SpriteFont _defaultFont;
        string _fontName;
        float _marginWidth = 50;
        float _marginPadding = 10;
        ThreadedList<Line> _lines;
        UIElementLayer _textLayer;
        Vector2F _textPos;

        Vector2F _lineNumPos;
        Color _lineNumColor = new Color(42, 136, 151, 255);

        Vector2F _marginPos;
        Color _bgColor = new Color(30, 30, 30, 255);
        Color _marginColor = new Color(60,60,60, 255);
        Color _marginLineColor = Color.White;
        Color _marginTextColor = Color.SkyBlue;

        /* TODO:
         *  - Allow segment to have OnPressed and OnReleased virtual methods to allow custom segment actions/types, such as:
         *      - Open a URL
         *      - Open an in-game menu.
         *      - An item link e.g. Path of Exile chat items
         * 
         */

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings); 
            
            _lines = new ThreadedList<Line>();

            FontName = settings.DefaultFontName;
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Rectangle gb = GlobalBounds;
            _textPos = (Vector2F)gb.TopLeft;
            _textPos.X += _marginPadding;
            _marginPos = new Vector2F(gb.X + _marginWidth, gb.Y);
            _lineNumPos = _marginPos - new Vector2F(_marginPadding, 0);

            if (_showLineNumbers)
                _textPos.X += _marginWidth;

        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            Rectangle gb = GlobalBounds;
            Vector2F p = _textPos;
            Vector2F numPos = _lineNumPos;

            sb.DrawRect(gb, _bgColor, 0, null, 0);

            if (_showLineNumbers)
            {
                sb.DrawRect(new RectangleF(gb.X, gb.Y, _marginWidth, gb.Height), _marginColor, 0, null, 0);
                sb.DrawLine(_marginPos, _marginPos + new Vector2F(0, gb.Height), _marginLineColor, 1, 1, 0);
            }

            for (int l = 0; l < _lines.Count; l++)
            {
                Line line = _lines[l];
                Segment seg = line.First;
                p.X = _textPos.X;

                if(_showLineNumbers)
                    sb.DrawString(_defaultFont, line.LineNumber.ToString(), numPos - new Vector2F(line.LineNumberSize.X, 0), _lineNumColor, null, 0);

                while(seg != null)
                {
                    sb.DrawString(seg.Font, seg.Text, p, seg.Color, null, 0);
                    p.X += seg.MeasuredSize.X;
                    seg = seg.Next;
                }

                p.Y += 25;
                numPos.Y += 25;
            }
        }

        public void SetText(string text)
        {
            string[] lines = Regex.Split(text, "\r?\n");
            for (int i = 0; i < lines.Length; i++)
            {
                Line line = new Line();
                line.LineNumber = (uint)i + 1U;
                line.LineNumberSize = _defaultFont.MeasureString(line.LineNumber.ToString());

                line.First = new Segment()
                {
                    Text = lines[i],
                    Color = Color.White,
                    Font = _defaultFont,
                    MeasuredSize = _defaultFont.MeasureString(lines[i]),
                };

                _lines.Add(line);
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="UITextBox"/> is a multi-line textbox. If false, any line breaks will be substituted with spaces.
        /// </summary>
        public bool IsMultiLine
        {
            get => _isMultiline;
            set
            {
                if(_isMultiline != value)
                {
                    _isMultiline = value;
                    OnUpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the font for the current <see cref="UILabel"/>. This will attempt to load/retrieve and populate <see cref="Font"/>.
        /// </summary>
        [UIThemeMember]
        public string FontName
        {
            get => _fontName;
            set
            {
                value = (value ?? string.Empty).ToLower();
                if (_fontName != value)
                {
                    _fontName = value;
                    if (!string.IsNullOrWhiteSpace(_fontName))
                    {
                        Engine.Content.LoadFont(_fontName, (font, isReload) =>
                        {
                            _defaultFont = font;
                            for(int i = 0; i < _lines.Count; i++)
                            {
                                Segment seg = _lines[i].First;
                                while(seg != null)
                                {
                                    seg.Font = font;
                                    seg = seg.Next;
                                }
                            }
                        },
                        new SpriteFontParameters()
                        {
                            FontSize = 16,
                        });
                    }
                }
            }
        }

        public string Text
        {
            get => "";
            set => SetText(value);
        }

        public bool ShowLineNumbers
        {
            get => _showLineNumbers;
            set
            {
                if(_showLineNumbers != value)
                {
                    _showLineNumbers = value;
                    OnUpdateBounds();
                }
            }
        }
    }
}
