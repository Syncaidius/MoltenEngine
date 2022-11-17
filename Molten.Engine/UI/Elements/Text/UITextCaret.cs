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
            public UITextChunk Chunk { get; internal set; }

            public UITextLine Line { get; internal set; }

            public UITextSegment Segment { get; internal set; }

            public int CharIndex { get; internal set; }
        }

        public RectStyle SelectedLineStyle = new RectStyle(new Color(60, 60, 60, 200), new Color(160, 160, 160, 255), 2);

        public RectStyle SelectedSegmentStyle = new RectStyle(new Color(130, 130, 220, 255));

        internal UITextCaret(UITextElement element)
        {
            Parent = element;
            Start = new CaretPoint();
            End = new CaretPoint();
        }

        public CaretPoint Start { get; }

        public CaretPoint End { get; }

        public UITextElement Parent { get; }
    }
}
