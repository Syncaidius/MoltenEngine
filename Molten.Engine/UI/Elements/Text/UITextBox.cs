using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Graphics;
using Molten.Input;

namespace Molten.UI
{
    public partial class UITextBox : UITextElement
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

        UITextChunk _firstChunk;
        UITextChunk _lastChunk;

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

        private void ScrollChanged(UIScrollBar element)
        {
            RenderOffset = new Vector2F(-_hScroll.Value, -_vScroll.Value);
        }

        private void OnMarginPaddingChanged(LineMargin obj)
        {
            OnUpdateBounds();
        }

        /// <inheritdoc/>
        public override UITextLine NewLine()
        {
            UITextLine line = new UITextLine(this);
            _lastChunk = _lastChunk.AppendLine(line);
            return line;
        }

        /// <inheritdoc/>
        public override void Clear()
        {
            _firstChunk = new UITextChunk(1);
            _lastChunk = _firstChunk;
            Recalculate();
        }

        /// <inheritdoc/>
        public override void AppendLine(UITextLine line)
        {
            _lastChunk = _lastChunk.AppendLine(line);
        }

        /// <inheritdoc/>
        public override void AppendSegment(UITextSegment segment)
        {
            _lastChunk.LastLine.AppendSegment(segment);
        }

        /// <inheritdoc/>
        public override void InsertLine(UITextLine line, UITextLine insertAfter)
        {
            _lastChunk.InsertLine(line, insertAfter);
        }

        /// <inheritdoc/>
        public override string GetText()
        {
            StringBuilder sb = new StringBuilder();
            UITextChunk chunk = _firstChunk;
            while(chunk != null)
            {
                UITextLine line = chunk.FirstLine;
                while(line != null)
                {
                    line.GetText(sb);
                    line = line.Next;
                }

                chunk = chunk.Next;
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
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

        public override void OnPressed(CameraInputTracker tracker)
        {
            base.OnPressed(tracker);

            UITextChunk chunk = _firstChunk;

            Rectangle cBounds = _textBounds;
            Vector2I pos = (Vector2I)tracker.Position;

            // If we've already picked a start and end point, clear caret and start again.
            if(Caret.End.Line != null)
                Caret.Clear();

            while (chunk != null)
            {
                cBounds.Height = chunk.Height;

                if (Caret.Start.Line == null)
                {
                    if (chunk.Pick(pos, ref cBounds, Caret.Start))
                        break;
                }
                else
                {
                    if (chunk.Pick(pos, ref cBounds, Caret.End))
                    {
                        Caret.CalculateSelected();
                        break;
                    }
                }               

                cBounds.Y += chunk.Height;
                chunk = chunk.Next;
            }
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            RectangleF gb = (RectangleF)GlobalBounds;

            sb.DrawRect(gb, _bgColor, 0, null, 0);

            _margin.Render(sb);

            sb.PushClip(_textClipBounds);
            UITextChunk chunk = _firstChunk;
            Rectangle cBounds = _textBounds;

            DrawLines(sb, chunk, cBounds, DrawLineSelection);
            DrawLines(sb, chunk, cBounds, DrawLineContent);

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
                        UITextLine line = chunk.FirstLine;
                        int lineNum = chunk.StartLineNumber;

                        while(line != null)
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

            UITextSegment seg = line.FirstSegment;
            while (seg != null)
            {
                segBounds.Width = seg.Size.X;
                segBounds.Height = seg.Size.Y;

                if (seg.IsSelected)
                {
                    if (seg == Caret.Start.Segment)
                    {
                        RectangleF eBounds = segBounds;
                        eBounds.X += Caret.Start.Char.StartOffset;
                        eBounds.Width = Caret.Start.Char.EndOffset;
                        sb.Draw(eBounds, ref Caret.SelectedSegmentStyle);
                    }
                    else if (seg == Caret.End.Segment)
                    {
                        RectangleF eBounds = segBounds;
                        eBounds.Width = Caret.End.Char.StartOffset;
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

        public override void Recalculate()
        {
            float distH = 0;
            float distV = 0;

            UITextChunk chunk = _firstChunk;
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

        /// <inheritdoc/>
        public override bool IsMultiLine { get; } = true;
    }
}
