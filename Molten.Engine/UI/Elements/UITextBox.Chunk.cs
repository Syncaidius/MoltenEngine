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
    public partial class UITextBox
    {
        const int CHUNK_CAPACITY = 512;

        private class Chunk
        {
            ThreadedList<Line> _lines = new ThreadedList<Line>(CHUNK_CAPACITY);
            Rectangle _bounds;

            internal void AppendLine(Line line)
            {
                if(_lines.Count < CHUNK_CAPACITY)
                {
                    _lines.Add(line);
                    if (Next != null)
                        Next.StartLineNumber++;
                }
                else
                {
                    if (Next == null || Next.Capacity == 0)
                        AddNext();

                    // Directly insert line to avoid duplicated checks
                    Next._lines.Insert(0, line);
                }
            }

            internal void InsertLine(Line line, int index)
            {
                if (_lines.Count < CHUNK_CAPACITY)
                {
                    _lines.Insert(index, line);
                    if (Next != null)
                        Next.StartLineNumber++;
                }
                else
                {
                    if (index == 0)
                    {
                        if (Previous == null || Previous.Capacity == 0)
                            AddPrevious();

                        Previous._lines.Append(line);
                    }
                    else if (index == CHUNK_CAPACITY - 1)
                    {
                        if (Next == null || Next.Capacity == 0)
                            AddNext();

                        // Directly insert line to avoid duplicated checks
                        Next._lines.Insert(0, line);
                    }
                    else
                    {
                        Split(index);
                        _lines.Add(line);
                    }
                }
            }

            /// <summary>
            /// Splits the current <see cref="Chunk"/>, moving all items from at and beyond the given index, into a new <see cref="Chunk"/>.
            /// </summary>
            /// <param name="splitIndex">All lines at and beyond the current index are cut off into a new chunk, added after the current one.</param>
            private void Split(int splitIndex)
            {
                int nextCount = _lines.Count - splitIndex;
                if (Next == null || Next.Capacity == 0)
                    AddNext();

                Next._lines.AddRange(_lines, splitIndex, nextCount);
                _lines.RemoveRange(splitIndex, nextCount);
            }

            private void AddPrevious()
            {
                Chunk prev = new Chunk();

                if (Previous != null)
                {
                    Previous.Next = prev;
                    prev.Previous = Previous;
                }

                Previous = prev;
                Previous.Next = this;
            }

            private void AddNext()
            {
                Chunk next = new Chunk();

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

            public Line Pick(Vector2I pos)
            {
                Rectangle lBounds = new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, 0);

                if (_bounds.Contains(pos))
                {
                    Line l = null;
                    for(int i = _lines.Count - 1; i >= 0; i--)
                    {
                        l = _lines[i];
                        lBounds.Height = l.Height;
                        lBounds.Y += l.Height;

                        if (lBounds.Contains(pos))
                            return l;
                    }
                }

                return null;
            }

            public void Render(SpriteBatcher sb)
            {
                RectangleF rBounds = _bounds;
                Line line = null;
                Segment seg = null;

                for (int i = _lines.Count - 1; i >= 0; i--)
                {
                    line = _lines[i];
                    seg = line.First;

                    while(seg != null)
                    {
                        rBounds.Width = seg.Size.X;
                        rBounds.Height = seg.Size.Y;

                        seg.Render(sb, ref rBounds);

                        rBounds.X += seg.Size.X;
                        seg = seg.Next;
                    }

                    rBounds.X = _bounds.X;
                    rBounds.Y += line.Height;
                }
            }

            internal Chunk Previous { get; set; }
            
            internal Chunk Next { get; set; }

            public int StartLineNumber { get; set; }

            public int EndLineNumber => StartLineNumber + _lines.Count;

            public int Capacity => CHUNK_CAPACITY - _lines.Count;

            public int Width
            {
                get => _bounds.Width;
                set => _bounds.Width = value; // TODO update word-wrapping
            }

            public Vector2I Position
            {
                get => _bounds.TopLeft;
                set
                {
                    _bounds.Left = value.X;
                    _bounds.Top = value.Y;
                }
            }
        }
    }
}
