using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// Container for panel render data.
    /// </summary>
    public struct UIPanelData : IUIRenderData
    {
        [DataMember]
        public Color BorderColor;

        [DataMember]
        public float BorderThickness;

        [DataMember]
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
