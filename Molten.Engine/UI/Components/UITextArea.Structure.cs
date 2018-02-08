using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;

namespace Molten.UI
{
    public partial class UITextArea
    {
        struct CursorLocation
        {
            public int line;
            public int index;

            public static bool operator ==(CursorLocation a, CursorLocation b)
            {
                return (a.index == b.index) && (a.line == b.line);
            }

            public static bool operator !=(CursorLocation a, CursorLocation b)
            {
                return (a.index != b.index) || (a.line != b.line);
            }
        }

        class Line : IPoolable
        {
            public Rectangle bounds;
            public Rectangle selectionBounds;
            public UIRenderedText textObject;

            public Line(Engine engine)
            {
                textObject = new UIRenderedText(engine);
                textObject.Text = "";
            }

            public void Clear()
            {
                textObject.Text = "";
            }

            public string Text
            {
                get { return textObject.Text; }
                set { textObject.Text = value; }
            }

            public int Length
            {
                get { return textObject.Length; }
            }
        }
    }
}
