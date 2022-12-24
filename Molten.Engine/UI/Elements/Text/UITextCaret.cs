using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UITextCaret
    {
        public enum MoveDirection
        {
            None = 0,

            Left = 1,

            Right = 2,

            Up = 3,

            Down = 4,
        }

        public class CaretPoint
        {
            internal void Clear()
            {
                Chunk = null;
                Line = null;
                Segment = null;
                CharIndex = 0;
                StartOffset = 0;
            }

            public override string ToString()
            {
                return Line != null ? $"Line: {Line} -- Seg: {Segment} -- Char: {CharIndex} -- Offset: {StartOffset}" : "[[None]]";
        }

            public UITextChunk Chunk { get; internal set; }

            public UITextLine Line { get; internal set; }

            public UITextSegment Segment { get; internal set; }

            /// <summary>
            /// The character index of the caret, within the selected segment's string.
            /// </summary>
            public int CharIndex { get; internal set; }

            /// <summary>
            /// The offset of the caret, from the start of the selected segment, in pixels.
            /// </summary>
            public float StartOffset { get; internal set; }
        }

        public RectStyle SelectedLineStyle = new RectStyle(new Color(60, 60, 60, 200), new Color(160, 160, 160, 255), 2);

        public RectStyle SelectedSegmentStyle = new RectStyle(new Color(130, 130, 220, 255));

        internal List<UITextSegment> Selected { get; } = new List<UITextSegment>();

        public bool EndBeforeStart { get; private set; }

        TimeSpan _blinkTime;
        bool _blinkVisible;

        internal UITextCaret(UITextBox element)
        {
            Parent = element;
            Start = new CaretPoint();
            End = new CaretPoint();
        }

        private void ResetSelected()
        {
            // Unselect previous segments
            for (int i = 0; i < Selected.Count; i++)
                Selected[i].IsSelected = false;

            Selected.Clear();
            EndBeforeStart = false;
        }

        internal void CalculateSelected()
        {
            ResetSelected();

            if (Start.Chunk != null && End.Chunk != null)
            {
                if (Start.Chunk == End.Chunk)
                {
                    if (Start.Line == End.Line)
                        CheckSegmentOrder();
                    else
                        CheckLineOrder();
                }
                else
                {
                    // Iterate backwards to check if end is before start
                    UITextChunk chunk = Start.Chunk;

                    while (chunk != null)
                    {
                        if (chunk == End.Chunk)
                        {
                            EndBeforeStart = true;
                            break;
                        }

                        chunk = chunk.Previous;
                    }
                }

                SelectChunkedSegments();
            }
        }

        /// <summary>
        /// Moves the caret by one index in the given <see cref="MoveDirection"/>. For left/right, the index is a character. For up/down, the index is a line.
        /// </summary>
        /// <param name="p">The <see cref="CaretPoint"/> to be moved.</param>
        /// <param name="dir">The direction to move.</param>
        public void Move(CaretPoint p, MoveDirection dir)
        {
            switch (dir)
            {
                case MoveDirection.Left:
                    if (p.Segment != null)
                    {
                        if (p.CharIndex == 0)
                        {
                            p.Segment = FindPrevSegment(p.Segment, p);
                        }
                        else
                        {
                            p.CharIndex--;
                            p.StartOffset -= p.Segment.MeasureCharWidth(p.CharIndex);
                        }
                    }

                    // Get to previous Line
                    if (p.Segment == null)
                    {
                        if (p.Line.Previous != null)
                        {
                            p.Line = p.Line.Previous;
                            p.Segment = FindPrevSegment(p.Line.LastSegment, p);
                        }
                        else
                        {
                            p.Segment = p.Line.FirstSegment;
                            p.CharIndex = 0;
                            p.StartOffset = 0;
                        }
                    }
                    break;
            }
        }

        private UITextSegment FindPrevSegment(UITextSegment seg, CaretPoint p)
        {
            if (seg == null)
                return seg;

            seg = seg.Previous;

            while (seg != null)
            {
                if (seg.Length > 0)
                {
                    p.CharIndex = seg.Text.Length - 1;
                    p.StartOffset = seg.Size.X - seg.MeasureCharWidth(p.CharIndex);
                    break;
                }

                seg = seg.Previous;
            }

            return seg;
        }


        /// <summary>
        /// Checks the line order of two lines within the same <see cref="UITextChunk"/> or chunkless <see cref="UITextBox"/>.
        /// </summary>
        private void CheckLineOrder()
        {
            // Iterate backwards to check if end is before start
            UITextLine line = Start.Line;

            while (line != null)
            {
                if(line == End.Line)
                {
                    EndBeforeStart = true;
                    break;
                }

                line = line.Previous;
            }
        }

        /// <summary>
        /// Checks the segment order of two segments on the same line.
        /// </summary>
        private void CheckSegmentOrder()
        {
            // Iterate backwards to check if end is before start
            UITextSegment seg = Start.Segment;

            while (seg != null)
            {
                if(seg == End.Segment)
                {
                    EndBeforeStart = true;
                    break;
                }

                seg = seg.Previous;
            }
        }

        private void SelectChunkedSegments()
        {
            UITextChunk chunk;
            UITextLine line;
            bool firstSegFound = false;

            if (EndBeforeStart)
            {
                chunk = End.Chunk;
                line = End.Line;
            }
            else
            {
                chunk = Start.Chunk;
                line = Start.Line;
            }

            while(chunk != null)
            {
                SelectSegments(line, ref firstSegFound);

                if (chunk == End.Chunk)
                    break;

                chunk = chunk.Next;
                line = chunk.FirstLine;
            }
        }

        private void SelectSegments(UITextLine startLine, ref bool firstSegFound)
        {
            UITextLine line = startLine;
            CaretPoint start, end;

            if (EndBeforeStart)
            {
                start = End;
                end = Start;
            }
            else
            {
                start = Start;
                end = End;
            }

            if (line == start.Line && start.Segment == null)
            {
                firstSegFound = true;
                line = line.Next;
            }

            while (line != null)
            {
                UITextSegment seg = line.FirstSegment;

                while (seg != null)
                {
                    Selected.Add(seg);

                    if (seg == start.Segment)
                        firstSegFound = true;

                    seg.IsSelected = firstSegFound;

                    if (seg == end.Segment)
                        break;

                    seg = seg.Next;
                }

                if (line == end.Line)
                    break;

                line = line.Next;
            }
        }

        internal void Update(Timing time)
        {
            _blinkTime += time.ElapsedTime;
            if (_blinkTime >= BlinkInterval)
            {
                _blinkTime -= BlinkInterval;
                _blinkVisible = !_blinkVisible;
            }
        }

        internal void Render(SpriteBatcher sb, Vector2F pos, float height)
        {
            if (_blinkVisible && IsVisible)
            {
                Vector2F end = new Vector2F(pos.X, pos.Y + height);
                sb.DrawLine(pos, end, Color.White, 2);
            }
        }

        public void Clear()
        {
            Start.Clear();
            End.Clear();
            ResetSelected();
        }

        /// <summary>
        /// Gets the start point of the caret.
        /// </summary>
        public CaretPoint Start { get; }

        /// <summary>
        /// Gets the end point of the caret.
        /// </summary>
        public CaretPoint End { get; }

        /// <summary>
        /// Gets the parent <see cref="UITextBox"/> of the current <see cref="UITextCaret"/>.
        /// </summary>
        public UITextBox Parent { get; }

        /// <summary>
        /// Gets or sets the blink interval of the caret.
        /// </summary>
        public TimeSpan BlinkInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Gets or sets whether or not the caret is visible.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
