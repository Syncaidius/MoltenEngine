using Molten.IO;
using Molten.Graphics;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>A base class that allows a UI component to use other UI Components as parts.</summary>
    public abstract class UICompoundComponent : UIComponent
    {
        List<UIComponent> _parts;

        public UICompoundComponent(UISystem ui)
            : base(ui)
        {
            _parts = new List<UIComponent>();
        }

        /// <summary>Adds a UI component to act as a part of the compound component..</summary>
        /// <param name="part"></param>
        protected void AddPart(UIComponent part)
        {
            if (_parts.Contains(part) == false)
                _parts.Add(part);
        }

        /// <summary>Removes a component currently acting as a part of the compound component.</summary>
        /// <param name="part"></param>
        protected void RemovePart(UIComponent part)
        {
            if (_parts.Contains(part))
                _parts.Remove(part);
        }

        protected override UIComponent OnGetComponent(Vector2 inputPos)
        {
            if (!_visible)
                return null;

            bool thisHasInput = false;

            // Check if any parts were clicked before proceeding
            if (Contains(inputPos))
            {
                UIComponent result = null;

                foreach (UIComponent part in _parts)
                {
                    result = part.GetComponent(inputPos);

                    if (result != null)
                        return result;
                }

                thisHasInput = true;
            }


            //check if child input should be ignored
            if (_ignoreChildInput == false)
            {
                //handle input for children in reverse order (last/top first)
                for (int c = _children.Count - 1; c >= 0; c--)
                {
                    UIComponent childResult = _children[c].GetComponent(inputPos);
                    //if a child was interacted with, return from this.
                    if (childResult != null)
                        return childResult;
                }
            }

            //if not enabled, don't handle input for this component.
            if (!_enabled)
                return null;

            if (thisHasInput)
                return this;
            else
                return null;
        }

        /// <summary>Updates the component and all its children.</summary>
        /// <param name="time"></param>
        public override void Update(Timing time)
        {
            if (_enabled)
            {
                OnUpdate(time);

                foreach (UIComponent component in _parts)
                    component.Update(time);

                _children.ForInterlock(0, 1, (id, child) =>
                {
                    if (!child.IsVisible || !child.IsEnabled)
                        return false;

                    child.Update(time);

                    return false;
                });
            }
        }

        /// <summary>Draws the component and all its children.</summary>
        /// <param name="sb">The surface that the UI component must draw on to.</param>
        public override void Render(ISpriteBatch sb)
        {
            if (!_visible)
                return;

            OnRender(sb);

            foreach (UIComponent component in _parts)
                component.Render(sb);

            // Render all children inside the component's clipping bounds.
            if (_enableClipping)
            {
                sb.PushClip(_clippingBounds);
                OnRenderClipped(sb);
                OnRenderChildren(sb);
                sb.PopClip();
            }
            else
            {
                OnRenderClipped(sb);
                OnRenderChildren(sb);
            }
        }
    }
}
