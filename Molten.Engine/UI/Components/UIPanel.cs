using Molten.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Molten.Utilities;
using System.Runtime.Serialization;
using Molten.Serialization;

namespace Molten.UI
{
    public class UIPanel : UIComponent
    {
        Color _bgColor;

        public UIPanel(UISystem ui)
            : base(ui)
        {
            _bgColor = new Color(100, 100, 200, 200);
        }

        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
        {
            sb.Draw(_globalBounds, _bgColor);
        }

        /// <summary>The background color of the panel.</summary>
        [DataMember]
        public Color BackgroundColor
        {
            get { return _bgColor; }
            set { _bgColor = value; }
        }
    }
}
