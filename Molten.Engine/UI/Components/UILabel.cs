using Molten.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectWrite;
using System.ComponentModel;
using System.Drawing.Design;
using Molten.Graphics;
using Molten.Utilities;
using System.Runtime.Serialization;
using Molten.Serialization;

namespace Molten.UI
{
    public class UILabel : UIComponent
    {
        UIRenderedText _text;

        public UILabel(UISystem ui)
            : base(ui)
        {
            _text = new UIRenderedText(ui);
        }

        protected override void OnDispose()
        {
            _text.Dispose();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();
            _text.Bounds = _globalBounds;
        }

        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
        {
            _text.Draw(sb);
        }

        /// <summary>Gets or sets the text.</summary>
        [DataMember]
        [ExpandableProperty]
        public string Text
        {
            get { return _text.Text; }
            set { _text.Text = value; }
        }
    }
}
