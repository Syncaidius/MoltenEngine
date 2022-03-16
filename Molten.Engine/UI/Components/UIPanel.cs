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
        public struct RenderData : IUIRenderData
        {
            public Color BorderColor;

            public float BorderThickness;

            public Color BackgroundColor;

            public RenderData()
            {
                BorderColor = UISettings.DefaultBorderColor;
                BackgroundColor = UISettings.DefaultBackgroundColor;
                BorderThickness = 2;
            }

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
