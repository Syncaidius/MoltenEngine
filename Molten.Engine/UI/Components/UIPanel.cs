using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIPanel : UIComponent<UIPanelRenderer>
    {
        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            BorderColor = settings.DefaultBorderColor;
            BackgroundColor = settings.DefaultBackgroundColor;
        }

        protected override void OnUpdate(Timing time)
        {
            
        }

        public ref Color BorderColor => ref Data.ExtData.BorderColor;

        public ref float BorderThickness => ref Data.ExtData.BorderThickness;

        public ref Color BackgroundColor => ref Data.ExtData.BackgroundColor;
    }

    public class UIPanelRenderer : UIComponentRenderer<UIPanelRenderer.Ext>
    {
        public struct Ext
        {
            public Color BorderColor;

            public float BorderThickness;

            public Color BackgroundColor;
        }

        public UIPanelRenderer()
        {

        }

        protected override void OnRender(SpriteBatcher sb, ref Ext ed)
        {
            if(ed.BackgroundColor.A > 0)
                sb.DrawRect(RenderBounds, ed.BackgroundColor);

            if(ed.BorderColor.A > 0 && ed.BorderThickness > 0)
                sb.DrawRectOutline(BorderBounds, ed.BorderColor, ed.BorderThickness);
        }
    }
}
