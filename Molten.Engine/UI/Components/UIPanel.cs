using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIPanel : UIComponent<UIPanel.RenderData>
    {
        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            Properties.BorderColor = theme.BorderColor;
            Properties.BackgroundColor = theme.BackgroundColor;
            Properties.BorderThickness = theme.BorderThickness;
        }

        /// <summary>
        /// Container for <see cref="UIPanel"/> render data.
        /// </summary>
        public struct RenderData : IUIRenderData
        {
            public Color BorderColor;

            public float BorderThickness;

            public Color BackgroundColor;

            public void Render(SpriteBatcher sb, UIRenderData data)
            {
                if (BackgroundColor.A > 0)
                    sb.DrawRect(data.RenderBounds, BackgroundColor);

                if (BorderColor.A > 0 && BorderThickness > 0)
                    sb.DrawRectOutline(data.BorderBounds, BorderColor, BorderThickness);
            }
        }
    }
}
