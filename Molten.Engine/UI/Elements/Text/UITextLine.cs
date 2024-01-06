using Molten.Graphics;
using System.Text;

namespace Molten.UI;

/// <summary>
/// Represents a line of text that is displayed by an implemented <see cref="UIElement"/>.
/// </summary>
public class UITextLine : UITextLinkable<UITextLine>
{
    public struct FindResult
    {
        /// <summary>
        /// The end-point. IF <see cref="IsReverse"/> is true, this will be the start-point instead.
        /// </summary>
        public UITextLine End;

        /// <summary>
        /// The total height of all lines in the result.
        /// </summary>
        public int Height;

        /// <summary>
        /// The largest line width found.
        /// </summary>
        public int Width;

        /// <summary>
        /// The number of lines that were found.
        /// </summary>
        public int Count;

        /// <summary>
        /// If true, <see cref="End"/> is the first line and the last line is the line that executed the find.
        /// </summary>
        public bool IsReverse;

        /// <summary>
        /// Creates a new instance of <see cref="FindResult"/>.
        /// </summary>
        /// <param name="isReverse">If true, find operation was done in reverse and <see cref="End"/> is the starting point.</param>
        /// <param name="startLine"></param>
        public FindResult(bool isReverse, UITextLine startLine)
        {
            Width = 0;
            Height = 0;
            IsReverse = isReverse;
            End = startLine;
        }
    }

    int _height;

    internal UITextLine(UITextBox element)
    {
        Parent = element;
        HasText = false;
    }

    public void Clear()
    {
        Width = 0;
        _height = 0;
        HasText = false;
        FirstSegment = new UITextSegment("", Color.White, null);
        LastSegment = FirstSegment;
    }

    internal bool Pick(ref Rectangle lBounds, ref Vector2I pickPoint, UITextCaret.CaretPoint caretPoint)
    {
        UITextSegment seg = FirstSegment;
        Vector2F fPos = (Vector2F)pickPoint;

        if (lBounds.Contains(pickPoint))
        {
            caretPoint.Line = this;
            RectangleF segBounds = (RectangleF)lBounds;

            while (seg != null)
            {
                segBounds.Width = seg.Size.X;
                if (segBounds.Contains(fPos))
                {
                    caretPoint.Segment = seg;
                    SpriteFont segFont = seg.Font ?? Parent.DefaultFont;

                    if (!string.IsNullOrWhiteSpace(seg.Text))
                    {
                        float dist = 0;
                        float prevDist = 0;
                        float charDist = 0;
                        float halfDist = 0;

                        for (int i = 0; i < seg.Text.Length; i++)
                        {
                            charDist = segFont.GetAdvanceWidth(seg.Text[i]);
                            dist += charDist;
                            halfDist = prevDist + (charDist / 2);

                            if (pickPoint.X <= segBounds.Left + dist)
                            {
                                if (pickPoint.X >= segBounds.Left + halfDist)
                                {
                                    caretPoint.CharIndex = i+1;
                                    caretPoint.StartOffset = dist;
                                }
                                else
                                {
                                    caretPoint.CharIndex = i;
                                    caretPoint.StartOffset = prevDist;
                                }                                    
                                break;
                            }

                            prevDist = dist;
                        }
                    } 

                    return true;
                }
                segBounds.X += seg.Size.X;
                seg = seg.Next;
            }
        }

        return false;
    }

    public UITextLine Split(UITextSegment seg, int? charIndex)
    {
        // Split the text behind the caret char-index, so that it can remain on the current line.
        if (charIndex.HasValue)
        {
            UITextSegment newSeg = null;

            // Do we need to split the current segment off at a certain char index?
            if (charIndex.Value > 0 && !string.IsNullOrEmpty(seg.Text))
            {
                string toStay = seg.Text.Substring(0, charIndex.Value);

                seg.Text = seg.Text.Remove(0, charIndex.Value);
                newSeg = new UITextSegment(toStay, seg.Color, seg.Font);
            }

            // If there's any split off remains to re-add to the current line, do so.
            seg.Previous?.LinkNext(newSeg);

            // Create the new line with the split-off segment and any following ones, if any.
            LastSegment = newSeg;
            if (seg == FirstSegment)
                FirstSegment = newSeg;
        }
        else
        {
            if (seg == FirstSegment)
                FirstSegment = seg.Next;

            if (seg == LastSegment)
                LastSegment = seg.Previous;

            seg.UnlinkPrevious();
        }

        UITextLine newLine = new UITextLine(Parent);
        newLine.AppendSegment(seg);
        return newLine;
    }

    public UITextSegment NewSegment(string text, Color color, SpriteFont font)
    {
        UITextSegment segment = new UITextSegment(text, color, font);
        AppendSegment(segment);
        return segment;
    }

    public void AppendSegment(UITextSegment seg)
    {
        if (seg != null)
        {
            if (LastSegment != null)
            {
                LastSegment.Next = seg;
                seg.Previous = LastSegment;
            }
            else
            {
                LastSegment = seg;
                FirstSegment = seg;
            }

            LastSegment = seg;
            Width += seg.Size.X;
            _height = Math.Max(_height, (int)Math.Ceiling(seg.Size.Y));
        }
    }       

    /// <summary>
    /// Iterates over a chain of <see cref="UITextLine"/>, starting with the current <see cref="UITextLine"/> until the last one is found.
    /// </summary>
    /// <param name="count">Output for number of <see cref="UITextLine"/> found between the current and last <see cref="UITextLine"/>.</param>
    /// <returns></returns>
    internal FindResult FindLast()
    {
        FindResult result = new FindResult(false, this);
        UITextLine line = this;

        while (line != null)
        {
            result.Width = (int)float.Max(result.Width, line.Width);
            result.Height += line.Height;
            result.Count++;

            result.End = line;
            line = line.Next;
        }

        return result;
    }

    /// <summary>
    /// Iterates over a chain of lines, starting with the current <see cref="UITextLine"/> until the <paramref name="end"/> <see cref="UITextLine"/> is found. 
    /// <para>Returns 0 if <paramref name="end"/> is not found.</para>
    /// </summary>
    /// <param name="end">The end <see cref="UITextLine"/>.</param>
    /// <returns>The number of lines between the current and <paramref name="end"/>.</returns>
    internal FindResult FindUntil(UITextLine end)
    {
        FindResult result = new FindResult(false, this);
        UITextLine line = this;
        result.End = end;

        while (line != null)
        {
            result.Width = (int)float.Max(result.Width, line.Width);
            result.Height += line.Height;
            result.Count++;

            if (line == end)
                break;

            if (line.Next == null)
            {
                result.End = line;
                break;
            }

            line = line.Next;
        }

        return result;
    }

    /// <summary>
    /// Iterates over a chain of lines, starting with the current <see cref="UITextLine"/> until the the <paramref name="maxCount"/> is found, the last line is found.
    /// </summary>
    /// <param name="maxCount">The max number of lines to find.</param>
    /// <returns>A <see cref="FindResult"/></returns>
    internal FindResult FindUntil(int maxCount)
    {
        FindResult result = new FindResult(false, this);
        UITextLine line = this;

        while (line != null)
        {
            result.Width = (int)float.Max(result.Width, line.Width);
            result.Height += line.Height;
            result.Count++;

            if (result.Count == maxCount)
                break;

            result.End = line;
            line = line.Next;
        }

        return result;
    }

    /// <summary>
    /// Iterates over a chain of lines, starting with the current <see cref="UITextLine"/> until the the <paramref name="maxCount"/> is found, the first line is found.
    /// </summary>
    /// <param name="maxCount">The max number of lines to find.</param>
    /// <returns>The last line that was found.</returns>
    internal FindResult FindUntilReverse(int maxCount)
    {
        FindResult result = new FindResult(true, this);
        UITextLine line = this;

        while (line != null)
        {
            result.Width = (int)float.Max(result.Width, line.Width);
            result.Height += line.Height;
            result.Count++;

            if (result.Count == maxCount)
                break;

            result.End = line;
            line = line.Previous;
        }

        return result;
    }

    /// <summary>
    /// Creates and inserts a new <see cref="UITextSegment"/> after the specified <see cref="UITextSegment"/>.
    /// </summary>
    /// <param name="after">The <see cref="UITextSegment"/> that the new segment should be inserted after.
    /// <para>If null, the new segment will be insert at the beginning of the line.</para></param>
    /// <param name="color">The color of the new segment's text.</param>
    /// <param name="font">The font of the new segment.</param>
    /// <returns></returns>
    public UITextSegment InsertSegment(UITextSegment after, Color color, SpriteFont font)
    {
        UITextSegment next = new UITextSegment("", Color.White, font);

        // Do we need to insert before another "next" segment also?
        if (after != null)
        {
            if (after.Next != null)
            {
                after.Next.Previous = next;
                next.Next = after.Next;
            }

            after.Next = next;
            next.Previous = after;
        }
        else
        {
            if(FirstSegment != null)
            {
                FirstSegment.Previous = after;
                after.Next = FirstSegment;
                FirstSegment = after;
            }
            else
            {
                FirstSegment = after;
                LastSegment = after;
            }
        }

        Width += after.Size.X;
        _height = Math.Max(_height, (int)Math.Ceiling(after.Size.Y));

        return next;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"First Segment: {FirstSegment?.ToString() ?? "[[None]]"}";
    }

    /// <summary>
    /// Gets the string of text represented by all <see cref="UITextSegment"/>s of the current <see cref="UITextLine"/>.
    /// </summary>
    /// <returns></returns>
    public string GetText()
    {
        string result = "";
        UITextSegment seg = FirstSegment;
        while(seg != null)
        {
            result += seg.Text;
            seg = seg.Next;
        }

        return result;
    }

    /// <summary>
    /// Gets the string of text represented by all <see cref="UITextSegment"/>s of the current <see cref="UITextLine"/> and appends it to the provided <see cref="StringBuilder"/>.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to output the line contents into.</param>
    /// <returns></returns>
    public void GetText(StringBuilder sb)
    {
        UITextSegment seg = FirstSegment;
        while (seg != null)
        {
            sb.Append(seg.Text);
            seg = seg.Next;
        }
    }

    /// <summary>
    /// Gets the measured width of the current <see cref="UITextLine"/>.
    /// </summary>
    public float Width { get; private set; }

    /// <summary>
    /// Gets the measured height of the current <see cref="UITextLine"/>.
    /// <para>If the line contains no text, the default line height of the <see cref="Parent"/> will be used instead.</para>
    /// </summary>
    public int Height => HasText ? _height : Parent.DefaultLineHeight;

    /// <summary>
    /// Gets the parent <see cref="UITextBox"/>, or null if none.
    /// </summary>
    public UITextBox Parent { get; internal set; }

    /// <summary>
    /// Gets the first <see cref="UITextSegment"/> on the current <see cref="UITextLine"/>.
    /// </summary>
    public UITextSegment FirstSegment { get; private set; }

    /// <summary>
    /// Gets the last <see cref="UITextSegment"/> on the current <see cref="UITextLine"/>.
    /// </summary>
    public UITextSegment LastSegment { get; private set; }

    /// <summary>
    /// Gets whether or not the current <see cref="UITextLine"/> contains text.
    /// </summary>
    public bool HasText { get; private set; }
}
