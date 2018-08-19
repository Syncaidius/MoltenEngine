using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// A rectangular UI element which also includes support for a different colored border.
    /// </summary>
    public class UIRectangle : IRenderable2D
    {
        Rectangle _top;
        Rectangle _left;
        Rectangle _right;
        Rectangle _bottom;
        Rectangle _inner;

        /// <summary>
        /// Sets the bounds with a border extending inward from it's edges, as specified in the provided <see cref="UIPadding"/> configuration.
        /// </summary>
        /// <param name="outerBounds">The outer bounds of the whole rectangle.</param>
        /// <param name="padding">The amount of padding to apply to the outer bounds.</param>
        public void Set(Rectangle outerBounds, UIPadding padding)
        {
            _left = new Rectangle(outerBounds.X, outerBounds.Y, padding.Left, outerBounds.Height);
            _right = new Rectangle(outerBounds.Right - padding.Right, outerBounds.Y, padding.Right, outerBounds.Height);
            _top = new Rectangle(_left.Right, outerBounds.Y, _right.Left - _left.Right, padding.Top);
            _bottom = new Rectangle(_left.Right, outerBounds.Bottom - padding.Bottom, _right.Left - _left.Right, padding.Bottom);
            _inner = padding.ApplyPadding(outerBounds);
        }

        /// <summary>
        /// Sets the bounds with a border extending out from it's edges.
        /// </summary>
        /// <param name=""></param>
        /// <param name="borderLeft">The amount to extend the left border outward.</param>
        /// <param name="borderTop">The amount to extend the top border outward.</param>
        /// <param name="borderRight">The amount to extend the right border outward.</param>
        /// <param name="borderBottom">The amount to extend the bottom border outward.</param>
        public void Set(Rectangle innerBounds, int borderLeft, int borderTop, int borderRight, int borderBottom)
        {
            _left = new Rectangle(innerBounds.X - borderLeft, innerBounds.Y, borderLeft, innerBounds.Height);
            _right = new Rectangle(innerBounds.Right + borderRight, innerBounds.Y, borderRight, innerBounds.Height);
            _top = new Rectangle(_left.Left, innerBounds.Y - borderTop, innerBounds.Width + borderLeft + borderRight, borderTop);
            _bottom = new Rectangle(_left.Left, innerBounds.Bottom + borderBottom, _top.Width, borderBottom);
        }

        public void Render(SpriteBatch sb)
        {
            // Render boarder
            if (BorderColor.A > 0)
            {
                if (_left.Width > 0)
                    sb.DrawRect(_left, BorderColor);

                if (_right.Width > 0)
                    sb.DrawRect(_right, BorderColor);

                if (_top.Height > 0)
                    sb.DrawRect(_top, BorderColor);

                if (_bottom.Height > 0)
                    sb.DrawRect(_bottom, BorderColor);

                // Render background.
                if (Color.A > 0)
                    sb.DrawRect(_inner, Color);
            }
            else
            {
                // Render background.
                if (Color.A > 0)
                    sb.DrawRect(_inner, Color);
            }
        }

        /// <summary>
        /// Gets or sets the rectangle color.
        /// </summary>
        public Color Color { get; set; } = Color.Grey;

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        public Color BorderColor { get; set; } = Color.White;
    }
}
