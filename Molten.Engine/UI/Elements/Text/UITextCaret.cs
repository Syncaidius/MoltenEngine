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
        public class CaretPoint
        {
            public class SelectedChar
            {
                /// <summary>
                 /// The character index of the caret, within the selected segment's string.
                 /// </summary>
                public int? Index { get; internal set; }

                /// <summary>
                /// The offset of the caret, from the start of the selected segment, in pixels.
                /// </summary>
                public float StartOffset { get; internal set; }

                /// <summary>
                /// The offset of the caret, from the end of the selected segment, in pixels.
                /// </summary>
                public float EndOffset { get; internal set; }
            }

            internal void Clear()
            {
                Chunk = null;
                Line = null;
                Segment = null;
                Char.Index = null;
                Char.StartOffset = 0;
                Char.EndOffset = 0;
            }

            public UITextChunk Chunk { get; internal set; }

            public UITextLine Line { get; internal set; }

            public UITextSegment Segment { get; internal set; }

            public SelectedChar Char { get; } = new SelectedChar();

            public float CharSelectWidthOffset { get; internal set; }

            public float CharSelectWidth { get; internal set; }
        }

        public RectStyle SelectedLineStyle = new RectStyle(new Color(60, 60, 60, 200), new Color(160, 160, 160, 255), 2);

        public RectStyle SelectedSegmentStyle = new RectStyle(new Color(130, 130, 220, 255));

        internal List<UITextSegment> Selected { get; } = new List<UITextSegment>();

        TimeSpan _blinkTime;
        bool _blinkVisible;

        internal UITextCaret(UITextElement element)
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
        }

        internal void CalculateSelected()
        {
            ResetSelected();

            CaretPoint start = Start;
            CaretPoint end = End;
            CaretPoint temp = null;

            if (start.Chunk != null && end.Chunk != null)
            {
                if (start.Chunk == end.Chunk)
                {
                    if (start.Line == end.Line)
                        CheckSegmentOrder(ref start, ref end, ref temp);
                    else
                        CheckLineOrder(ref start, ref end, ref temp);
                }
                else
                {
                    // Is the end chunk before the start chunk?
                    if (End.Chunk.StartLineNumber < Start.Chunk.StartLineNumber)
                    {
                        temp = start;
                        start = end;
                        end = temp;
                    }
                }

                SelectChunkedSegments(start, end);
            }
            else
            {
                if (start.Line != null && end.Line != null)
                {
                    if (start.Line == end.Line)
                        CheckSegmentOrder(ref start, ref end, ref temp);
                    else
                        CheckLineOrder(ref start, ref end, ref temp);
                }

                bool firstSegFound = false;
                SelectSegments(start.Line, ref firstSegFound, start, end);
            }
        }

        /// <summary>
        /// Checks the line order of two lines within the same <see cref="UITextChunk"/> or chunkless <see cref="UITextElement"/>.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="temp"></param>
        private void CheckLineOrder(ref CaretPoint start, ref CaretPoint end, ref CaretPoint temp)
        {
            // Iterate backwards to check if end is before start
            UITextLine line = start.Line;
            while(line != null)
            {
                if(line == end.Line)
                {
                    temp = start;
                    start = end;
                    end = temp;
                    break;
                }

                line = line.Previous;
            }
        }

        /// <summary>
        /// Checks the segment order of two segments on the same line.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="temp"></param>
        private void CheckSegmentOrder(ref CaretPoint start, ref CaretPoint end, ref CaretPoint temp)
        {
            UITextSegment seg = start.Segment;
            while(seg != null)
            {
                if(seg == end.Segment)
                {
                    temp = start;
                    start = end;
                    end = temp;
                    break;
                }

                seg = seg.Previous;
            }
        }

        private void SelectChunkedSegments(CaretPoint start, CaretPoint end)
        {
            UITextChunk chunk = start.Chunk;
            UITextLine line = start.Line;
            bool firstSegFound = false;

            while(chunk != null)
            {
                SelectSegments(line, ref firstSegFound, start, end);

                if (chunk == End.Chunk)
                    break;

                chunk = chunk.Next;
                line = chunk.FirstLine;
            }
        }

        private void SelectSegments(UITextLine startLine, ref bool firstSegFound, CaretPoint start, CaretPoint end)
        {
            UITextLine line = startLine;

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
        /// Gets the parent <see cref="UITextElement"/> of the current <see cref="UITextCaret"/>.
        /// </summary>
        public UITextElement Parent { get; }

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
