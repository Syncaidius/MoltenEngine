using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public struct UITextData : IUIRenderData
    {
        [DataMember]
        public Color Color;

        [DataMember]
        public string Text;

        internal TextFont Font;

        [DataMember]
        public Vector2F Position;

        public IMaterial Material;

        public void Render(SpriteBatcher sb, UIRenderData data)
        {
            if (Font != null && Color.A > 0)
                sb.DrawString(Font, Text, Position, Color, Material);
        }

        public void ApplyTheme(UITheme theme, UIElementTheme eTheme, UIStateTheme stateTheme)
        {
            Color = stateTheme.TextColor;
        }
    }
}
