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

        internal class Selector<T> where T : class
        {
            public RectStyle Style;

            public T Selected;

            public Selector(Color fillColor, Color borderColor, float borderThickness)
            {
                Style = new RectStyle(fillColor, borderColor, borderThickness);
            }

            public Selector(Color color, float borderThickness)
            {
                Style = new RectStyle(color, color, borderThickness);
            }
        }

        UIScrollBar _vScroll;
        UIScrollBar _hScroll;

        Chunk _firstChunk;
        Chunk _lastChunk;

        bool _isMultiline;
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
        Selector<UITextSegment> _segSelector = new Selector<UITextSegment>(new Color(130, 130, 220, 255), 2); 
        Selector<UITextLine> _lineSelector = new Selector<UITextLine>(new Color(60,60,60,200), new Color(160,160,160,255), 2);

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

            _firstChunk = new Chunk(1);
            _lastChunk = _firstChunk;            

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

            ChunkPickResult result;

            Chunk chunk = _firstChunk;

            Rectangle cBounds = _textBounds;
            Vector2I pos = (Vector2I)tracker.Position;

            while (chunk != null)
            {
                cBounds.Height = chunk.Height;
                chunk.Pick(pos, ref cBounds, out result);

                if (result.Line != null)
                {
                    _segSelector.Selected = result.Segment;
                    _lineSelector.Selected = result.Line;
                    break;
                }

                cBounds.Y += chunk.Height;
                chunk = chunk.Next;
            }
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            Rectangle gb = GlobalBounds;

            sb.DrawRect(gb, _bgColor, 0, null, 0);

            _margin.Render(sb);

            sb.PushClip(_textClipBounds);
            Chunk chunk = _firstChunk;
            Rectangle cBounds = _textBounds;
            while(chunk != null)
            {
                cBounds.Height = chunk.Height;

                if (_textClipBounds.Intersects(cBounds) || _textClipBounds.Contains(cBounds))
                {
                    RectangleF segBounds = cBounds;
                    RectangleF lineBounds = cBounds;
                    UITextLine line = null;
                    UITextSegment seg = null;

                    for (int i = 0; i < chunk.Lines.Count; i++)
                    {
                        line = chunk.Lines[i];
                        seg = line.First;

                        lineBounds.Height = line.Height;

                        if (line == _lineSelector.Selected)
                            sb.DrawRect(lineBounds, ref _lineSelector.Style);

                        while (seg != null)
                        {
                            segBounds.Width = seg.Size.X;
                            segBounds.Height = seg.Size.Y;

                            if (seg == _segSelector.Selected)
                                sb.Draw(segBounds, ref _segSelector.Style);

                            seg.Render(sb, line.Parent, ref segBounds);

                            segBounds.X += seg.Size.X;
                            seg = seg.Next;
                        }

                        segBounds.X = cBounds.X;
                        segBounds.Y += line.Height;
                        lineBounds.Y += line.Height;
                    }
                }

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

        public void CalcScrollBars()
        {
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
