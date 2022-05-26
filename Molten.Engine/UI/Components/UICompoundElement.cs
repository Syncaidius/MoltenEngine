using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI.Components
{
    internal abstract class UICompoundElement<EP> : UIElement<EP>
        where EP : struct, IUIRenderData
    {
        class CompoundContainer
        {
            public UIElement Element;
            public Vector2I Offset;
        }

        List<CompoundContainer> _compoundElements;

        public UICompoundElement()
        {
            _compoundElements = new List<CompoundContainer>();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Rectangle gBounds = GlobalBounds;

            foreach (CompoundContainer cc in _compoundElements)
            {
                cc.Element.BaseData.LocalBounds.X = gBounds.X + cc.Offset.X;
                cc.Element.BaseData.LocalBounds.Y = gBounds.Y + cc.Offset.Y;
            }
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            for (int i = _compoundElements.Count - 1; i >= 0; i--)
                _compoundElements[i].Element.Update(time);
        }

        internal override void Render(SpriteBatcher sb)
        {
            base.Render(sb);

            // Render compound components, inside global bounds rather than render bounds.
            // Note - RenderBounds is intended for rendering child elements, not compound component elements.
            if (BaseData.IsClipEnabled)
            {
                sb.PushClip(BaseData.GlobalBounds);
                foreach (CompoundContainer cc in _compoundElements)
                    cc.Element.Render(sb);
                sb.PopClip();
            }
            else
            {
                foreach (CompoundContainer cc in _compoundElements)
                    cc.Element.Render(sb);
            }
        }
    }
}
