using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIPanel : UIComponent
    {
        Color _bgColor;

        public UIPanel(Engine engine) : base(engine)
        {
            _bgColor = new Color(100, 100, 200, 200);
        }

        protected override void OnRender(SpriteBatch sb)
        {
            sb.DrawRect(_globalBounds, _bgColor);
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
