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

        public override ICursorAcceptor HandleInput(Vector2F inputPos)
        {
            if (!IsVisible)
                return null;

            bool thisHasInput = false;

            // Check if any parts were clicked before proceeding
            if (Contains(inputPos))
            {
                ICursorAcceptor result = null;

                foreach (UIComponent part in _compoundParts)
                {
                    result = part.HandleInput(inputPos);

                    if (result != null)
                        return result;
                }

                thisHasInput = true;
            }


            //check if child input should be ignored
            if (!IgnoreChildInput)
            {
                //handle input for children in reverse order (last/top first)
                for (int c = _children.Count - 1; c >= 0; c--)
                {
                    ICursorAcceptor childResult = _children[c].HandleInput(inputPos);
                    //if a child was interacted with, return from this.
                    if (childResult != null)
                        return childResult;
                }
            }

            //if not enabled, don't handle input for this component.
            if (!IsEnabled)
                return null;

            if (thisHasInput)
                return this;
            else
                return null;
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

        public override void OnUpdateUi(Timing time)
        {
            for (int i = 0; i < _compoundParts.Count; i++)
                _compoundParts[i].OnUpdateUi(time);

            base.OnUpdateUi(time);
        }

        public override void OnRenderUi(SpriteBatcher sb)
        {
            for (int i = 0; i < _compoundParts.Count; i++)
                _compoundParts[i].OnRenderUi(sb);

            base.OnRenderUi(sb);
        }
    }
}
