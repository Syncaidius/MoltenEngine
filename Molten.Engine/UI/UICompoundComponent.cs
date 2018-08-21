using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UICompoundComponent : UIComponent
    {
        List<UIComponent> _compoundParts;

        public UICompoundComponent()
        {
            _compoundParts = new List<UIComponent>();
        }

        protected override void OnPostUpdateBounds()
        {
            base.OnPostUpdateBounds();

            for (int i = 0; i < _compoundParts.Count; i++)
                _compoundParts[i].UpdateBounds();
        }

        protected void AddPart(UIComponent part)
        {
            _compoundParts.Add(part);
            part.Parent = this;
        }

        protected void RemovePart(UIComponent part)
        {
            _compoundParts.Remove(part);
            part.Parent = null;
        }

        protected override void OnUpdate(Timing time)
        {
            for (int i = 0; i < _compoundParts.Count; i++)
                _compoundParts[i].Update(time);

            base.OnUpdate(time);
        }

        protected override void OnRender(SpriteBatch sb)
        {
            for (int i = 0; i < _compoundParts.Count; i++)
                _compoundParts[i].Render(sb);

            base.OnRender(sb);
        }
    }
}
