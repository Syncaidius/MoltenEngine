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
    /// UI render data that contains no fields. Used for <see cref="UIElement"/> that do not render any parts.
    /// </summary>
    public struct UIBlankData : IUIRenderData
    {
        public void ApplyTheme(UITheme theme, UIElementTheme eTheme, UIStateTheme stateTheme) { }

        public void Render(SpriteBatcher sb, UIRenderData data) { }

    }
}
