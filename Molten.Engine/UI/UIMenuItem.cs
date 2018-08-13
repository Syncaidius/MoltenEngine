using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIMenuItem : UIComponent
    {
        UIText _label;

        public UIMenuItem()
        {
            _label = new UIText(Engine.Current.DefaultFont, this.GetType().Name);
            _label.OnTextChanged += _label_OnTextChanged;
        }

        protected override void UpdateBounds()
        {
            base.UpdateBounds();
            _label.Bounds = _localBounds;
        }

        protected override void OnRender(SpriteBatch sb)
        {
            _label.Render(sb);
            base.OnRender(sb);
        }

        private void _label_OnTextChanged(UIText obj)
        {
            LocalBounds = new Rectangle(_localBounds.X, _localBounds.Y, (int)_label.Size.X, (int)_label.Size.Y);
        }

        public UIText Label => _label;
    }
}
