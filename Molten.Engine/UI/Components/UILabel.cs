using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UILabel : UIComponent
    {
        UIRenderedText _text;

        public UILabel(Engine engine) : base(engine)
        {
            _text = new UIRenderedText(engine);
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

        protected override void OnRender(SpriteBatch sb)
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
