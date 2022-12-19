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

        internal UITextChunk AppendLine(UITextLine line)
        {
            return InsertLine(line, LastLine, UITextInsertType.After);
        }

        internal UITextChunk InsertLine(UITextLine line, UITextLine origin, UITextInsertType insertType = UITextInsertType.After)
        {
            UITextLine.FindResult fLast = line.FindLast();
            UITextChunk endChunk = this;

            // Insert all chained lines
            if (insertType == UITextInsertType.Before)
            {
                if (origin != null)
                {
                    origin.Previous?.LinkNext(line);
                    origin.LinkPrevious(fLast.End);

                    if (origin == FirstLine)
                        FirstLine = line;
                }
                else // No origin?
                {
                    // Insert at the start of the chunk.
                    if (FirstLine == null)
                    {
                        FirstLine = line;
                        LastLine = fLast.End;
                    }
                    else
                    {
                        FirstLine.LinkPrevious(fLast.End);
                        line.UnlinkPrevious();
                        FirstLine = line;
                    }
                }
            }
            else
            {
                if (origin != null)
                {
                    origin.Next?.LinkPrevious(fLast.End);
                    origin.LinkNext(line);

                    if (origin == LastLine)
                        LastLine = fLast.End;
                }
                else
                {
                    // Insert at the end of the chunk
                    if(LastLine == null)
                    {
                        FirstLine = line;
                        LastLine = fLast.End;
                    }
                    else
                    {
                        LastLine.LinkNext(line);
                        line.UnlinkNext();
                        LastLine = fLast.End;
                    }
                }
            }

            LineCount += fLast.Count;
            int overCap = LineCount - CHUNK_CAPACITY;
            int addCount = fLast.Count - overCap;

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
                        if(Previous.LastLine != null)
                        {
                            Previous.LastLine.LinkNext(FirstLine);
                        }
                        else
                        {
                            Previous.FirstLine = FirstLine;
                            Previous.LastLine = fResult.End;
                        }

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
                        Next.LastLine = LastLine;
                        Next.LineCount += fResult.Count;
                        Next._height += fResult.Height;
                        Next._width = Math.Max(Next._width, fResult.Width);
                    }

                    LastLine = capPrev;
                    overCap -= fResult.Count;
                    endChunk = Next;
                }

                LineCount = CHUNK_CAPACITY;

                // Calculate remaining width/height to be added
                while(line != null && addCount > 0)
                {
                    _height += line.Height;
                    _width = (int)Math.Max(_width, line.Width);

                    addCount--;
                    line = line.Next;
                }
            }
            else // Resize the current chunk
            {
                _height += fLast.Height;
                _width = Math.Max(_width, fLast.Width);
            }

            return endChunk;
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
