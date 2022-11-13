using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Graphics;
using Molten.Input;
using static Molten.UI.UITextBox;

namespace Molten.UI
{
    public partial class UITextBox : UIElement
    {
        internal class LineMargin
        {
            public event ObjectHandler<LineMargin> BoundsChanged;

            public event ObjectHandler<LineMargin> PaddingChanged;

            public Rectangle Bounds
            {
                get => _bounds;
                set
                {
                    _bounds = value;
                    _linePos = (Vector2F)_bounds.TopRight;
                    BoundsChanged?.Invoke(this);
                }
            }

            public int Width => _bounds.Width;

            public int Height => _bounds.Height;

            /// <summary>
            /// Gets the position at which the margin line starts.
            /// </summary>
            internal Vector2F DividerPosition => _linePos;

            public int Padding
            {
                get => _padding;
                set
                {
                    if(_padding != value)
                    {
                        _padding = value;
                        PaddingChanged?.Invoke(this);
                    }
                }
            }

            public RectStyle MarginStyle = new RectStyle(new Color(60, 60, 60, 255));

            public LineStyle Style = LineStyle.Default;

            Rectangle _bounds = new Rectangle(0, 0, 50, 0);
            Vector2F _linePos = new Vector2F(50,0);
            int _padding = 7;

            internal void Render(SpriteBatcher sb)
            {
                sb.DrawRect(_bounds, ref MarginStyle, 0, null, 0);
                sb.DrawLine(_linePos, _linePos + new Vector2F(0, _bounds.Height), ref Style, 0);
            }
        }
        /// <summary>
        /// Invoked when <see cref="ApplyRules()"/> was called.
        /// </summary>
        public event ObjectHandler<UITextBox> OnRulesApplied;

        RuleSet _rules;

        UIScrollBar _vScroll;
        UIScrollBar _hScroll;

        Chunk _firstChunk;
        Chunk _lastChunk;

        bool _isMultiline;
        string _fontName;
        LineMargin _margin;
        int _scrollbarWidth = 20;
        int _lineSpacing = 5;
        int _lineHeight = 25;
        Rectangle _textBounds;
        Rectangle _textClipBounds;
        Color _bgColor = new Color(30, 30, 30, 255);

        // Line numbers
        bool _showLineNumbers;
        Vector2F _lineNumPos;
        Color _lineNumColor = new Color(52, 156, 181, 255);

        // Line Selector
        Chunk _selectedChunk;
        Line _selectedLine;
        Segment _selectedSeg;
        RectStyle _selectorStyle = new RectStyle(new Color(60,60,60,200), new Color(160,160,160,255), 2);

        /* TODO:
         *  - Allow segment to have OnPressed and OnReleased virtual methods to allow custom segment actions/types, such as:
         *      - Open a URL
         *      - Open an in-game menu.
         *      - An item link e.g. Path of Exile chat items
         */

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _margin = new LineMargin();
            _margin.PaddingChanged += OnMarginPaddingChanged;

            _rules = new RuleSet();
            _firstChunk = new Chunk(1);
            _lastChunk = _firstChunk;

            FontName = settings.DefaultFontName;

            _vScroll = BaseElements.Add<UIScrollBar>();
            _vScroll.Increment = _lineHeight;
            _vScroll.ValueChanged += ScrollChanged;

            _hScroll = BaseElements.Add<UIScrollBar>();
            _hScroll.Increment = _lineHeight;
            _hScroll.ValueChanged += ScrollChanged;
            _hScroll.Direction = UIElementFlowDirection.Horizontal;
        }

        private void ScrollChanged(UIScrollBar element)
        {
            RenderOffset = new Vector2F(-_hScroll.Value, -_vScroll.Value);
        }

        public void ApplyRules()
        {
            OnRulesApplied?.Invoke(this);
        }

        private void OnMarginPaddingChanged(LineMargin obj)
        {
            OnUpdateBounds();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Rectangle gb = GlobalBounds;
            _margin.Bounds = new Rectangle(gb.X, gb.Y, _margin.Width, gb.Height);
            _lineNumPos = _margin.DividerPosition - new Vector2F(_margin.Padding, -_margin.Padding);

            _textBounds = gb;
            _textBounds.Left += _margin.Padding;
            _textBounds.Top += _margin.Padding;
            _textBounds.Left += _margin.Width;

            _textClipBounds = _textBounds;
            _textBounds += RenderOffset;

            CalcScrollBars();

            if (_hScroll.IsVisible)
            {
                _textBounds.Bottom -= _scrollbarWidth;
                _hScroll.LocalBounds = new Rectangle(0, gb.Height - _scrollbarWidth, gb.Width - _scrollbarWidth, _scrollbarWidth);
            }

            if (_vScroll.IsVisible)
            {
                _textBounds.Right -= _scrollbarWidth;
                _vScroll.LocalBounds = new Rectangle(gb.Width - _scrollbarWidth, 0, _scrollbarWidth, gb.Height - _scrollbarWidth);
            }

            Chunk chunk = _firstChunk;

            /*Vector2F tl = (Vector2F)_textBounds.TopLeft;
            Vector2F p = tl;
            Line line;
            for (int i = 0; i < _lines.Count; i++)
            {
                line = _lines[i];
                line.Position = p;
                line.SelectorBounds = new RectangleF()
                {
                    X = _margin.DividerPosition.X + 2,
                    Y = gb.Y + 5 + (_lineHeight * i),
                    Width = gb.Width - (_margin.Width + 2 + _scrollbarWidth),
                    Height = Math.Max(_lineHeight, line.TextBounds.Height)
                };

                p.Y += _lineHeight;
            }*/
        }

        protected override void OnAdjustRenderBounds(ref Rectangle renderbounds)
        {
            base.OnAdjustRenderBounds(ref renderbounds);

            if (_hScroll.IsVisible)
                renderbounds.Height -= _scrollbarWidth;

            if (_vScroll.IsVisible)
                renderbounds.Width -= _scrollbarWidth;
        }

        public override bool OnScrollWheel(InputScrollWheel wheel)
        {
            base.OnScrollWheel(wheel);
            _vScroll.Value -= wheel.Delta * _vScroll.Increment;
            return true;
        }

        public override void OnPressed(UIPointerTracker tracker)
        {
            base.OnPressed(tracker);

            (Line line, Segment seg) result;

            Chunk chunk = _firstChunk;

            Rectangle cBounds = _textBounds;
            Vector2I pos = (Vector2I)tracker.Position;

            while (chunk != null)
            {
                cBounds.Height = chunk.Height;

                result = chunk.Pick(pos, ref cBounds);
                if (result.line != null)
                {
                    _selectedChunk = chunk;
                    _selectedLine = result.line;
                    _selectedSeg = result.seg;
                    break;
                }

                cBounds.Y += chunk.Height;
                chunk = chunk.Next;
            }

            /*for (int i = 0; i < _lines.Count; i++)
            {
                line = _lines[i];

                if (line.SelectorBounds.Contains(tracker.Position))
                {
                    _selectedLine = i;
                    _selectedSeg = line.OnPressed(tracker.Position);
                    break;
                }
            }*/
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            Rectangle gb = GlobalBounds;

            sb.DrawRect(gb, _bgColor, 0, null, 0);

            //if (_selectedLine.HasValue)
            //    sb.DrawRect(_lines[_selectedLine.Value].SelectorBounds, ref _selectorStyle, 0, null, 0);

            //if (_selectedSeg != null)
            //    sb.DrawRect(_selectedSeg.Bounds, Color.Red, 0, null, 0);

            _margin.Render(sb);

            sb.PushClip(_textClipBounds);
            Chunk chunk = _firstChunk;
            Rectangle cBounds = _textBounds;
            while(chunk != null)
            {
                cBounds.Height = chunk.Height;

                if (_textClipBounds.Intersects(cBounds) || _textClipBounds.Contains(cBounds))
                    chunk.Render(sb, ref cBounds);

                cBounds.Y += chunk.Height;
                chunk = chunk.Next;
            }
            sb.PopClip();

            if (_showLineNumbers)
            {
                chunk = _firstChunk;
                cBounds = _textBounds;
                
                while (chunk != null)
                {
                    cBounds.Height = chunk.Height;

                    if (_textClipBounds.Intersects(cBounds) || _textClipBounds.Contains(cBounds))
                    {
                        Vector2F numPos = _lineNumPos;
                        numPos.Y = cBounds.Y;

                        for (int i = 0; i < chunk.Lines.UnsafeCount; i++)
                        {
                            int lineNum = chunk.StartLineNumber + i;
                            string numString = lineNum.ToString(); // TODO cache line numbers in Chunk.
                            Vector2F numSize = DefaultFont.MeasureString(numString);
                            numSize.Y = chunk.Lines[i].Height;

                            sb.DrawString(DefaultFont, numString, numPos - new Vector2F(numSize.X, 0), _lineNumColor, null, 0);
                            numPos.Y += numSize.Y; 
                        }
                    }

                    cBounds.Y += chunk.Height;
                    chunk = chunk.Next;
                }
            }
        }

        private void SetText(string text)
        {
            string[] lines = Regex.Split(text, "\r?\n");
            for (int i = 0; i < lines.Length; i++)
            {
                Line line = new Line(this);
                line.SetText(DefaultFont, lines[i]);

                _lastChunk.AppendLine(line);
            }

            CalcScrollBars();
        }

        private void CalcScrollBars()
        {
            Line line;
            float distH = 0;
            float distV = 0;

            Chunk chunk = _firstChunk;
            while(chunk != null)
            {
                distH = Math.Max(distH, chunk.Width - _textBounds.Width);

                distV += chunk.Height;
                chunk = chunk.Next;
            }

            // Horizontal scroll bar
            if(distH > 0)
            {
                _hScroll.IsVisible = true;
                _hScroll.MaxValue = distH + _scrollbarWidth;
            }
            else
            {
                _hScroll.IsVisible = false;
            }

            // Virtual scroll bar
            if(distV > _textBounds.Height)
            {
                _vScroll.IsVisible = true;
                _vScroll.MaxValue = distV + _scrollbarWidth;
            }
            else
            {
                _vScroll.IsVisible = false;
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

        internal SpriteFont DefaultFont { get; private set; }

        internal int DefaultLineHeight { get; private set; }

        /// <summary>
        /// Gets or sets the name of the default font for the current <see cref="UITextBox"/>. This will attempt to load/retrieve and populate <see cref="Font"/>.
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
                            DefaultFont = font;
                            DefaultLineHeight = (int)Math.Ceiling(DefaultFont.MeasureString(" ").Y);
                        },
                        new SpriteFontParameters()
                        {
                            FontSize = 16,
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the text for the current <see cref="UITextBox"/>.
        /// </summary>
        public string Text
        {
            get => "";
            set => SetText(value);
        }

        /// <summary>Gets or sets whether or not line-numbers are visible.</summary>
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

        /// <summary>
        /// Gets or sets the horizontal and vertical scroll-bar width.
        /// </summary>
        public int ScrollBarWidth
        {
            get => _scrollbarWidth;
            set
            {
                if(_scrollbarWidth != value)
                {
                    _scrollbarWidth = value;
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RuleSet"/> for the current <see cref="UITextBox"/>.
        /// </summary>
        public RuleSet Rules
        {
            get => _rules;
            set
            {
                if(_rules != value)
                {
                    if (value == null)
                        throw new NullReferenceException("UITextbox.Rules cannot be set to null.");

                    _rules = value;
                    ApplyRules();
                }
            }
        }
    }
}
