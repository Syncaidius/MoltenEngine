using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// A rectangular UI element which also includes support for a different colored border.
    /// </summary>
    public class UIPanel : UIComponent
    {
        Rectangle _top;
        Rectangle _left;
        Rectangle _right;
        Rectangle _bottom;
        Rectangle _inner;

        protected override void OnPostUpdateBounds()
        {
            base.OnPostUpdateBounds();
            Rectangle gBounds = GlobalBounds;
            _left = new Rectangle(gBounds.X, gBounds.Y, ClipPadding.Left, gBounds.Height);
            _right = new Rectangle(gBounds.Right - ClipPadding.Right, gBounds.Y, ClipPadding.Right, gBounds.Height);
            _top = new Rectangle(_left.Right, gBounds.Y, _right.Left - _left.Right, ClipPadding.Top);
            _bottom = new Rectangle(_left.Right, gBounds.Bottom - ClipPadding.Bottom, _right.Left - _left.Right, ClipPadding.Bottom);
            _inner = ClipPadding.ApplyPadding(gBounds);
        }

        public override void OnRenderUi(SpriteBatcher sb)
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
            }

            // Render background.
            if (BackgroundColor.A > 0)
                sb.DrawRect(_inner, BackgroundColor);

            base.OnRenderUi(sb);
        }

        /// <summary>
        /// Gets or sets the background color of the current <see cref="UIComponent"/>.
        /// </summary>
        public Color BackgroundColor { get; set; } = UIComponent.DefaultBackgroundColor;

        /// <summary>
        /// Gets or sets the border color of the current <see cref="UIComponent"/>.
        /// </summary>
        public Color BorderColor { get; set; } = UIComponent.DefaultBorderColor;
    }
}
