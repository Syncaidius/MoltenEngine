using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIMenuBar : UIComponent
    {
        Color _bgColor = new Color("#FF0000FF");

        public override void Render(SpriteBatch sb)
        {
            sb.DrawRect(GlobalBounds, _bgColor);
            base.Render(sb);
        }

        /// <summary>
        /// Gets or sets the background color of the menu bar.
        /// </summary>
        public Color BackgroundColor
        {
            get => _bgColor;
            set => _bgColor = value;
        }
    }
}
