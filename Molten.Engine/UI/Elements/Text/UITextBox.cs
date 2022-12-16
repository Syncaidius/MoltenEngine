using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.Collections;
using Molten.Graphics;
using Molten.Input;

namespace Molten.UI
{
    public partial class UITextBox : UIElement
    {
        private delegate void LineRenderCallback(SpriteBatcher sb, ref RectangleF lineBounds, ref RectangleF segBounds, UITextLine line);

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
                sb.DrawRect((RectangleF)_bounds, ref MarginStyle, 0, null, 0);
                sb.DrawLine(_linePos, _linePos + new Vector2F(0, _bounds.Height), ref Style, 0);
            }
        }

        UIScrollBar _vScroll;
        UIScrollBar _hScroll;
        LineMargin _margin;

        string _fontName;
        UITextParser _parser;

        int _scrollbarWidth = 20;
        int _lineHeight = 25;
        Rectangle _textBounds;
        Rectangle _textClipBounds;
        Color _bgColor = new Color(30, 30, 30, 255);

        // Line numbers
        bool _showLineNumbers;
        Vector2F _lineNumPos;
        Color _lineNumColor = new Color(52, 156, 181, 255);

        /* TODO:
         *  - Allow segment to have OnPressed and OnReleased virtual methods to allow custom segment actions/types, such as:
         *      - Open a URL
         *      - Open an in-game menu.
         *      - An item link e.g. Path of Exile chat items
         */

        /// <inheritdoc/>
        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            Caret = new UITextCaret(this);
            DefaultFontName = settings.DefaultFontName;
            _parser = settings.DefaultTextParser ?? new UIDefaultTextParser();

            _margin = new LineMargin();
            _margin.PaddingChanged += OnMarginPaddingChanged;

            _vScroll = BaseElements.Add<UIScrollBar>();
            _vScroll.Increment = _lineHeight;
            _vScroll.ValueChanged += ScrollChanged;

            _hScroll = BaseElements.Add<UIScrollBar>();
            _hScroll.Increment = _lineHeight;
            _hScroll.ValueChanged += ScrollChanged;
            _hScroll.Direction = UIElementFlowDirection.Horizontal;

            Clear();
        }

        public override void OnKeyboardChar(KeyboardDevice keyboard, ref KeyboardKeyState state)
        {
            base.OnKeyboardChar(keyboard, ref state);

            Vector2F charSize;

            if (HandleSpecialCharacters(keyboard, ref state))
                return;

            if (Caret.Start.Segment != null)
            {
                charSize = Caret.Start.Segment.Font.MeasureChar(state.Character);
                int? charIndex = Caret.Start.Char.Index;
                if (charIndex.HasValue)
                    Caret.Start.Segment.Insert(charIndex.Value, state.Character.ToString());
                else
                    Caret.Start.Segment.Text += state.Character;

                Caret.Start.Char.Index++;
                Caret.Start.Char.StartOffset += charSize.X;
            }
            else
            {
                UITextLine line = Caret.Start.Line;
                if (line != null)
                {
                    Color col = Color.White; // TODO use textbox default color instead.
                    SpriteFont font = DefaultFont;

                    if (line.LastSegment != null)
                    {
                        col = line.LastSegment.Color;
                        font = line.LastSegment.Font;
                    }
                    else if (line.Previous != null && line.Previous.LastSegment != null)
                    {
                        col = line.Previous.LastSegment.Color;
                        font = line.Previous.LastSegment.Font;
                    }

                    charSize = font.MeasureChar(state.Character);
                    Caret.Start.Segment = Caret.Start.Line.NewSegment(state.Character.ToString(), col, font);
                    Caret.Start.Char.Index = 1;
                    Caret.Start.Char.StartOffset += charSize.X;
                }
            }
        }

        private bool HandleSpecialCharacters(KeyboardDevice kb, ref KeyboardKeyState state)
        {
            switch (state.Character)
            {
                case '\b':

                    return true;

                case '\r':
                    if (Caret.Start.Segment != null)
                    {
                        UITextLine curLine = Caret.Start.Line;
                        UITextLine newLine = curLine.Split(Caret.Start.Segment, Caret.Start.Char.Index);
                        if (curLine != newLine)
                        {
                            Caret.Start.Segment = Caret.Start.Line.FirstSegment;
                            Caret.Start.Char.Index = 0;
                            
                            Caret.Start.Chunk = Caret.Start.Chunk.InsertLine(newLine, curLine);
                            Caret.Start.Line = newLine;
                        }
                        else
                        {
                            UITextLine line = new UITextLine(this);
                            Caret.Start.Chunk = Caret.Start.Chunk.InsertLine(line, curLine.Previous);
                        }
                    }
                    else
                    {
                        // We're at the end of the current line, simply insert and go to the new line.
                        Caret.Start.Line = InsertNewLine(Caret.Start.Line);
                        Caret.Start.Segment = null;
                        Caret.Start.Char.Index = null;
                    }
                    return true;

                case '\t':

                    return true;
            }

            return false;
        }

        private void ScrollChanged(UIScrollBar element)
        {
            RenderOffset = new Vector2F(-_hScroll.Value, -_vScroll.Value);
        }

        private void OnMarginPaddingChanged(LineMargin obj)
        {
            OnUpdateBounds();
        }

        /// <summary>
        /// Sets the text of the current <see cref="UITextBox"/>. The string will be parsed by the <see cref="UITextParser"/> at <see cref="Parser"/>.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            Clear();

            if (MaxLength > 0 && text.Length > MaxLength)
                text = text.Substring(0, MaxLength);

            _parser.ParseText(this, text);
        }

        /// <summary>
        /// Retrieves the full text string of the current <see cref="UITextBox"/>.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            StringBuilder sb = new StringBuilder();
            UITextChunk chunk = FirstChunk;
            while (chunk != null)
            {
                UITextLine line = chunk.FirstLine;
                while (line != null)
                {
                    line.GetText(sb);
                    line = line.Next;
                }

                chunk = chunk.Next;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Inserts a blank new <see cref="UITextLine"/>.
        /// </summary>
        /// <returns></returns>
        public UITextLine NewLine()
        {
            UITextLine line = new UITextLine(this);
            LastChunk = LastChunk.AppendLine(line);
            return line;
        }


        /// <summary>
        /// Inserts the given <see cref="UITextLine"/> at the end of the current <see cref="UITextBox"/>'s text.
        /// </summary>
        /// <param name="line">The line to append to the end.</param>
        public void AppendLine(UITextLine line)
        {
            LastChunk = LastChunk.AppendLine(line);
        }

        /// <summary>
        /// Inserts the given <see cref="UITextSegment"/> to the end of the last <see cref="UITextLine"/>, in the current <see cref="UITextBox"/>.
        /// </summary>
        /// <param name="segment">The segment to append to the end.</param>
        public void AppendSegment(UITextSegment segment)
        {
            LastChunk.LastLine.AppendSegment(segment);
        }

        /// <summary>
        /// Inserts a <see cref="UITextLine"/> after the specified one.
        /// </summary>
        /// <param name="line">The line to be inserted.</param>
        /// <param name="insertAfter">The line to insert <paramref name="line"/> after.</param>
        public void InsertLine(UITextLine line, UITextLine insertAfter)
        {
            LastChunk.InsertLine(line, insertAfter);
        }

        /// <summary>
        /// Inserts a new <see cref="UITextLine"/> after the specified one.
        /// </summary>
        /// <param name="insertAfter">The line to insert the new line after.</param>
        /// <returns></returns>
        public UITextLine InsertNewLine(UITextLine insertAfter)
        {
            UITextLine newLine = new UITextLine(this);
            LastChunk.InsertLine(newLine, insertAfter);
            return newLine;
        }

        /// <summary>
        /// Clear all text from the current <see cref="UITextBox"/>.
        /// </summary>
        public void Clear()
        {
            FirstChunk = new UITextChunk();
            LastChunk = FirstChunk;
            Recalculate();
        }

        /// <inheritdoc/>
        public override void OnPressed(CameraInputTracker tracker)
        {
            base.OnPressed(tracker);

            UITextChunk chunk = FirstChunk;
            Rectangle cBounds = _textBounds;
            Vector2I pos = (Vector2I)tracker.Position;

            Caret.Clear();

            while (chunk != null)
            {
                cBounds.Height = chunk.Height;
                if (chunk.Pick(pos, ref cBounds, Caret.Start))
                    break;

                cBounds.Y += chunk.Height;
                chunk = chunk.Next;
            }
        }

        /// <inheritdoc/>
        public override void OnDragged(CameraInputTracker tracker)
        {
            base.OnDragged(tracker);

            if (Caret.Start.Chunk == null)
                return;

            UITextChunk chunk = FirstChunk;
            Rectangle cBounds = _textBounds;
            Vector2I pos = (Vector2I)tracker.Position;

            while (chunk != null)
            {
                cBounds.Height = chunk.Height;
                if (chunk.Pick(pos, ref cBounds, Caret.End))
                {
                    Caret.CalculateSelected();
                    break;
                }

                cBounds.Y += chunk.Height;
                chunk = chunk.Next;
            }
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
            _textBounds += (Vector2I)RenderOffset;

            Recalculate();

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

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            Caret.Update(time);
        }

        /// <inheritdoc/>
        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            RectangleF gb = (RectangleF)GlobalBounds;

            sb.DrawRect(gb, _bgColor, 0, null, 0);

            _margin.Render(sb);

            sb.PushClip(_textClipBounds);
            UITextChunk chunk = FirstChunk;
            Rectangle cBounds = _textBounds;

            DrawLines(sb, chunk, cBounds, DrawLineSelection);
            DrawLines(sb, chunk, cBounds, DrawLineContent);

            sb.PopClip();

            if (_showLineNumbers)
            {
                chunk = FirstChunk;
                cBounds = _textBounds;
                int startLineNum = 1;

                while (chunk != null)
                {
                    cBounds.Height = chunk.Height;

                    if (_textClipBounds.Intersects(cBounds) || _textClipBounds.Contains(cBounds))
                    {
                        Vector2F numPos = _lineNumPos;
                        numPos.Y = cBounds.Y;
                        UITextLine line = chunk.FirstLine;
                        int lineNum = startLineNum;

                        while (line != null)
                        {
                            string numString = lineNum.ToString(); // TODO cache line numbers in Chunk.
                            Vector2F numSize = DefaultFont.MeasureString(numString);
                            numSize.Y = line.Height;

                            sb.DrawString(DefaultFont, numString, numPos - new Vector2F(numSize.X, 0), _lineNumColor, null, 0);
                            numPos.Y += numSize.Y;
                            line = line.Next;
                            lineNum++;
                        }
                    }

                    startLineNum += chunk.LineCount;
                    cBounds.Y += chunk.Height;
                    chunk = chunk.Next;
                }
            }
        }

        private void DrawLines(SpriteBatcher sb, UITextChunk chunk, Rectangle cBounds, LineRenderCallback segmentCallback)
        {
            while (chunk != null)
            {
                cBounds.Height = chunk.Height;

                if (_textClipBounds.Intersects(cBounds) || _textClipBounds.Contains(cBounds))
                {
                    RectangleF segBounds = (RectangleF)cBounds;
                    RectangleF lineBounds = (RectangleF)cBounds;
                    UITextLine line = chunk.FirstLine;

                    while (line != null)
                    {
                        lineBounds.Height = line.Height;
                        segmentCallback(sb, ref lineBounds, ref segBounds, line);

                        segBounds.X = cBounds.X;
                        segBounds.Y += line.Height;
                        lineBounds.Y += line.Height;
                        line = line.Next;
                    }
                }

                cBounds.Y += chunk.Height;
                chunk = chunk.Next;
            }
        }

        private void DrawLineSelection(SpriteBatcher sb, ref RectangleF lineBounds, ref RectangleF segBounds, UITextLine line)
        {
            if (line == Caret.Start.Line && Caret.End.Line == null)
            {
                sb.DrawRect(lineBounds, ref Caret.SelectedLineStyle);
                return;
            }

            UITextCaret.CaretPoint start, end;

            if (Caret.EndBeforeStart)
            {
                start = Caret.End;
                end = Caret.Start;
            }
            else
            {
                start = Caret.Start;
                end = Caret.End;
            }

            UITextSegment seg = line.FirstSegment;
            while (seg != null)
            {
                segBounds.Width = seg.Size.X;
                segBounds.Height = seg.Size.Y;

                if (seg.IsSelected)
                {
                    if (seg == start.Segment)
                    {
                        RectangleF eBounds = segBounds;
                        eBounds.X += start.Char.StartOffset;
                        eBounds.Width = start.Char.EndOffset;
                        sb.Draw(eBounds, ref Caret.SelectedSegmentStyle);
                    }
                    else if (seg == end.Segment)
                    {
                        RectangleF eBounds = segBounds;
                        eBounds.Width = end.Char.StartOffset;
                        sb.Draw(eBounds, ref Caret.SelectedSegmentStyle);
                    }
                    else
                    {
                        sb.Draw(segBounds, ref Caret.SelectedSegmentStyle);
                    }
                }

                segBounds.X += seg.Size.X;
                seg = seg.Next;
            }
        }

        private void DrawLineContent(SpriteBatcher sb, ref RectangleF lineBounds, ref RectangleF segBounds, UITextLine line)
        {
            UITextSegment seg = line.FirstSegment;
            while (seg != null)
            {
                segBounds.Width = seg.Size.X;
                segBounds.Height = seg.Size.Y;
                seg.Render(sb, line.Parent, ref segBounds);

                if (seg == Caret.Start.Segment)
                {
                    if (Caret.End.Line == null)
                    {
                        RectangleF eBounds = segBounds;
                        eBounds.X += Caret.Start.Char.StartOffset;
                        eBounds.Width = Caret.Start.Char.EndOffset;
                        Caret.Render(sb, eBounds.TopLeft, eBounds.Height);
                    }
                }
                else if (seg == Caret.End.Segment)
                {
                    RectangleF eBounds = segBounds;
                    eBounds.X += Caret.End.Char.StartOffset;
                    Caret.Render(sb, eBounds.TopLeft, eBounds.Height);
                }

                segBounds.X += seg.Size.X;
                seg = seg.Next;
            }
        }

        /// <summary>
        /// Forces the current <see cref="UITextBox"/> to recalculate any peripherial values, such as scrollbars or effects.
        /// </summary>
        public void Recalculate()
        {
            float distH = 0;
            float distV = 0;

            UITextChunk chunk = FirstChunk;
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

        public UITextChunk FirstChunk { get; protected set; }

        public UITextChunk LastChunk { get; protected set; }

        /// <summary>
        /// Gets the default <see cref="SpriteFont"/> for the current <see cref="UITextBox"/>. This is controlled by setting <see cref="DefaultFontName"/>
        /// </summary>
        public SpriteFont DefaultFont { get; private set; }

        /// <summary>
        /// Gets the default line height of the current <see cref="UITextBox"/>. 
        /// <para>This is based off the <see cref="DefaultFont"/>, which is controlled by setting <see cref="DefaultFontName"/>.</para>
        /// </summary>
        public int DefaultLineHeight { get; private set; }

        /// <summary>
        /// Gets or sets whether the current <see cref="UITextBox"/> is a multi-line textbox. If false, any line breaks will be substituted with spaces.
        /// </summary>
        public bool IsMultiLine { get; } = true;

        /// <summary>
        /// Gets or sets the maximum number of characters that can be entered into the current <see cref="UITextBox"/>.
        /// </summary>
        public int MaxLength { get; set; } = 0;

        /// <summary>
        /// Gets the <see cref="UITextCaret"/> bound to the current <see cref="UITextBox"/>.
        /// </summary>
        public UITextCaret Caret { get; private set; }

        /// <summary>
        /// Gets or sets the name of the default font for the current <see cref="UITextBox"/>. This will attempt to load/retrieve and populate <see cref="Font"/>.
        /// </summary>
        [UIThemeMember]
        public string DefaultFontName
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
        /// Gets or sets the <see cref="UITextParser"/> of the current <see cref="UITextBox"/>.
        /// </summary>
        public UITextParser Parser
        {
            get => _parser;
            set
            {
                value = value ?? Engine.Settings.UI.DefaultTextParser;
                if (_parser != value)
                {
                    _parser = value;
                    Clear();
                    string text = GetText();
                    _parser.ParseText(this, text);
                }
            }
        }

    }
}
