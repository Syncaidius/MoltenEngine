using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Font;
using Molten.Graphics;
using Silk.NET.Core.Native;

namespace Molten.UI
{
    public enum UITextInsertType
    {
        After = 0,

        Before = 1
    }


    public class UITextChunk
    {
        const int CHUNK_CAPACITY = 128;

        int _width;
        int _height;

        internal UITextChunk() { }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LinkNext(UITextChunk next)
        {
            Next = next;

            if(next != null)
                next.Previous = this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LinkPrevious(UITextChunk prev)
        {
            Previous = prev;

            if(prev != null)
                prev.Next = this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnlinkNext()
        {
            if (Next != null)
            {
                Next.Previous = null;
                Next = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnlinkPrevious()
        {
            if (Previous != null)
            {
                Previous.Next = null;
                Previous = null;
            }
        }

        private void FastAppendLine(UITextLine line)
        {
            if (LastLine != null)
            {
                LastLine.LinkNext(line);
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
        }

        private void FastInsertLine(UITextLine line, UITextLine insertAfter)
        {
            if (insertAfter != null)
            {
                line.Next = insertAfter.Next;
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

        internal UITextChunk InsertLine(UITextLine line, UITextLine origin, UITextInsertType insertType = UITextInsertType.After)
        {
            UITextLine.FindResult fLast = line.FindLast();

            // Insert all chained lines
            if (insertType == UITextInsertType.Before)
            {
                origin.Previous?.LinkNext(line);
                origin.LinkPrevious(fLast.End);

                if (origin == FirstLine)
                    FirstLine = line;
            }
            else
            {
                origin.Next?.LinkPrevious(fLast.End);
                origin.LinkNext(line);

                if (origin == LastLine)
                    LastLine = fLast.End;
            }

            LineCount += fLast.Count;
            int overCap = LineCount - CHUNK_CAPACITY;

            // Over capacity?
            if (overCap > 0)
            {
                // Offload what we can into neighbour chunks
                if (Previous != null)
                {
                    int max = Math.Min(Previous.Capacity, overCap);
                    if (max > 0)
                    {
                        UITextLine.FindResult fResult = FirstLine.FindUntil(max);
                        UITextLine capNext = fResult.End.Next;

                        fResult.End.UnlinkNext();
                        Previous.AppendLine(FirstLine); // TODO refactor append line to take into account chained lines (Line.Next).
                        FirstLine = capNext;

                        overCap -= fResult.Count;
                        Previous._height += fResult.Height;
                        Previous._width = Math.Max(Previous._width, fResult.Width);
                    }
                }

                // Back-roll from the end of the chunk, inserting "next" chunks until capacity is fulfilled.
                while(overCap > 0)
                {
                    int nextCap = Next != null ? Next.Capacity : 0;

                    if (nextCap == 0)
                    {
                        InsertNextChunk();
                        nextCap = CHUNK_CAPACITY;
                    }

                    int max = Math.Min(nextCap, overCap);

                    UITextLine.FindResult fResult = LastLine.FindUntilReverse(max);
                    UITextLine capPrev = fResult.End.Previous;
                    fResult.End.UnlinkPrevious();

                    if (Next.FirstLine != null)
                    {
                        Next.InsertLine(fResult.End, Next.FirstLine, UITextInsertType.Before);
                    }
                    else
                    {
                        Next.FirstLine = fResult.End;
                        Next.LineCount += fResult.Count;
                    }

                    LastLine = capPrev;
                    overCap -= fResult.Count;
                    Next._height += fResult.Height;
                    Next._width = Math.Max(Next._width, fResult.Width);
                }

                LineCount = CHUNK_CAPACITY;
            }
            else // Resize the current chunk
            {
                _height += fLast.Height;
                _width = Math.Max(_width, fLast.Width);
            }

            return this;
        }

        /// <summary>
        /// Inserts a new <see cref="UITextChunk"/> after the current chunk and before the next chunk, if one exists.
        /// </summary>
        /// <returns>The newly-created <see cref="UITextChunk"/>.</returns>
        private UITextChunk InsertNextChunk()
        {
            UITextChunk newChunk = new UITextChunk();
            newChunk.LinkNext(Next);
            LinkNext(newChunk);
            return newChunk;
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

        private void NewNext()
        {
            UITextChunk next = new UITextChunk();

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
