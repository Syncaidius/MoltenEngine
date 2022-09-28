using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using static Molten.UI.UITextBox;

namespace Molten.UI
{
    public partial class UITextBox
    {
        public class Line
        {
            public Segment First { get; private set; }

            public uint LineNumber;

            public Vector2F LineNumberSize;

            public Vector2F MeasuredSize;

            public void SetText(SpriteFont font, string text)
            {
                MeasuredSize = font.MeasureString(text);

                First = new Segment()
                {
                    Text = text,
                    Color = Color.White,
                    Font = font,
                    Bounds = new RectangleF(Vector2F.Zero, MeasuredSize),
                };
            }
        }

    }
}
