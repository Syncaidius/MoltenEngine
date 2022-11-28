using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Graphics;

namespace Molten.UI
{

    public class UITextChunk
    {
        const int CHUNK_CAPACITY = 128;

        int _width;
        int _height;
        int _startLineNumber;

        internal UITextChunk(int firstLineNumber)
        {
            _startLineNumber = firstLineNumber;
        }

        private void FastAppendLine(UITextLine line)
        {
            if (LastLine != null)
            {
                LastLine.Next = line;
                line.Previous = LastLine;
                LastLine = line;
            }
            else
            {
                // If there's no last line, there's also no first line. Set both.
                LastLine = line;
                FirstLine = line;
            }

            LineCount++;
            _width = Math.Max(_width, (int)Math.Ceiling(line.Width));
            _height += line.Height;

            if (Next != null)
                Next.StartLineNumber++;
        }

        private void FastInsertLine(UITextLine line, UITextLine insertAfter)
        {
            if (insertAfter != null)
            {
                insertAfter.Next = line;
                line.Previous = insertAfter;

                if (insertAfter == LastLine)
                    LastLine = line;
            }
            else
            {
                if (FirstLine != null)
                {
                    FirstLine.Previous = line;
                    line.Next = FirstLine;
                }
                else
                {
                    // If there's no first line, there's also no last line. Set it.
                    LastLine = line;
                }

                FirstLine = line;
            }

            LineCount++;
            _width = Math.Max(_width, (int)Math.Ceiling(line.Width));
            _height += line.Height;

            if (Next != null)
                Next.StartLineNumber++;
        }

        internal UITextChunk AppendLine(UITextLine line)
        {
            if (LineCount < CHUNK_CAPACITY)
            {
                FastAppendLine(line);
            }
            else
            {
                if (Next == null || Next.Capacity == 0)
                    NewNext();

                Next.FastInsertLine(line, Next.FirstLine);
                return Next;
            }

            return this;
        }

        internal UITextChunk InsertLine(UITextLine line, UITextLine insertAfter)
        {
            if (LineCount < CHUNK_CAPACITY)
            {
                FastInsertLine(line, insertAfter);
            }
            else
            {
                if (insertAfter == FirstLine)
                {
                    if (Previous == null || Previous.Capacity == 0)
                        NewPrevious();

                    Previous.FastAppendLine(line);
                    return Previous;
                }
                else if (insertAfter == LastLine)
                {
                    if (Next == null || Next.Capacity == 0)
                        NewNext();

                    // Directly insert line to avoid duplicated checks
                    Next.FastInsertLine(line, insertAfter);
                    return Next;
                }
                else
                {
                    Split(insertAfter);
                    FastAppendLine(line);
                }
            }

            return this;
        }

        /// <summary>
        /// Splits the current <see cref="UITextChunk"/>, moving all items from at and beyond the given index, into a new <see cref="UITextChunk"/>.
        /// </summary>
        /// <param name="splitAt">All lines at and beyond the given <see cref="UITextLine"/> are cut off into a new chunk, added after the current one.</param>
        private void Split(UITextLine splitAt)
        {
            UITextLine line = splitAt;
            UITextLine last = splitAt;

            int moveCount = 0;
            while (line != null)
            {
                moveCount++;
                last = line;
                line = line.Next;
            }

            if (Next == null || Next.Capacity < moveCount)
                NewNext();

            splitAt.Previous = null;
            Next.LineCount += moveCount;
            Next.LastLine = last;

            CalculateSize();
            Next.CalculateSize();
        }

        internal void CalculateSize()
        {
            _width = 0;
            _height = 0;
            UITextLine line = FirstLine;

            while (line != null)
            {
                _width = Math.Max(_width, (int)Math.Ceiling(line.Width));
                _height += line.Height;
                line = line.Next;
            }
        }

        private void NewPrevious()
        {
            UITextChunk prev = new UITextChunk(StartLineNumber - 1);

            if (Previous != null)
            {
                Previous.Next = prev;
                prev.Previous = Previous;
            }

            Previous = prev;
            Previous.Next = this;
        }

        private void NewNext()
        {
            UITextChunk next = new UITextChunk(StartLineNumber + LineCount);

            // Update the current "Next".
            if (Next != null)
            {
                Next.Previous = next;
                next.Next = Next;
            }

            // Update the new "Next".
            Next = next;
            Next.Previous = this;
        }

        internal bool Pick(Vector2I pos, ref Rectangle bounds, UITextCaret.CaretPoint point)
        {
            Rectangle lBounds = bounds;

            if (bounds.Contains(pos))
            {
                point.Chunk = this;
                point.Line = null;
                point.Segment = null;

                UITextLine line = FirstLine;
                while (line != null)
                {
                    lBounds.Height = line.Height;
                    if (line.Pick(ref lBounds, ref pos, point))
                        return true;                       

                    lBounds.Y += line.Height;
                    line = line.Next;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the previous <see cref="UITextChunk"/>, or null if none.
        /// </summary>
        public UITextChunk Previous { get; internal set; }

        /// <summary>
        /// Gets the next <see cref="UITextChunk"/>, or null if none.
        /// </summary>
        public UITextChunk Next { get; internal set; }

        /// <summary>
        /// Gets the first <see cref="UITextLine"/> in the current <see cref="UITextChunk"/>, or null if none.
        /// </summary>
        public UITextLine FirstLine { get; internal set; }

        /// <summary>
        /// Gets the last <see cref="UITextLine"/> in the current <see cref="UITextChunk"/>, or null if none.
        /// </summary>
        public UITextLine LastLine { get; internal set; }

        /// <summary>
        /// Gets the number of <see cref="UITextLine"/> held in the current <see cref="UITextChunk"/>.
        /// </summary>
        public int LineCount { get; private set; }

        /// <summary>
        /// Gets the line number/ID of the first <see cref="UITextLine"/> in the current <see cref="UITextChunk"/>.
        /// </summary>
        public int StartLineNumber
        {
            get => _startLineNumber;
            internal set
            {
                if (_startLineNumber != value)
                {
                    _startLineNumber = value;
                    if (Next != null)
                        Next.StartLineNumber = StartLineNumber + LineCount;
                }
            }
        }

        /// <summary>
        /// Gets the remaining line capacity of the current <see cref="UITextChunk"/>.
        /// </summary>
        public int Capacity => CHUNK_CAPACITY - LineCount;

        /// <summary>
        /// Gets the largest width of all <see cref="UITextLine"/> instances held in the current <see cref="UITextChunk"/>.
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Gets the total height of all <see cref="UITextLine"/> instances held in the current <see cref="UITextChunk"/>.
        /// </summary>
        public int Height => _height;
    }
}
