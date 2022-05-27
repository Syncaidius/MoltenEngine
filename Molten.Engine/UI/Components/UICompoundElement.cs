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
        public UICompoundElement()
        {
            CompoundElements = new UIChildCollection(this);
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Rectangle gBounds = GlobalBounds;
            OnUpdateCompoundBounds(ref gBounds, CompoundElements);            
        }

        protected abstract void OnUpdateCompoundBounds(ref Rectangle globalBounds, IEnumerable<UIElement> compoundElements);

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            for (int i = CompoundElements.Count - 1; i >= 0; i--)
                CompoundElements[i].Update(time);
        }

        internal override void Render(SpriteBatcher sb)
        {
            base.Render(sb);

            // Render compound components, inside global bounds rather than render bounds.
            // Note - RenderBounds is intended for rendering child elements, not compound component elements.
            if (BaseData.IsClipEnabled)
            {
                sb.PushClip(BaseData.GlobalBounds);
                foreach (UIElement e in CompoundElements)
                    e.Render(sb);
                sb.PopClip();
            }
            else
            {
                foreach (UIElement e in CompoundElements)
                    e.Render(sb);
            }
        }

        internal UIChildCollection CompoundElements { get; }
    }
}
