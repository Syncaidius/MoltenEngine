using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIChildCollection : IEnumerable<UIElement>
    {
        public event ObjectHandler<UIElement> OnElementAdded;

        public event ObjectHandler<UIElement> OnElementRemoved;

        List<UIElement> _elements;
        IReadOnlyList<UIElement> _readOnly;
        UIElement _owner;

        internal UIChildCollection(UIElement parent)
        {
            _owner = parent;
            _elements = new List<UIElement>();
            _readOnly = _elements.AsReadOnly();
        }

        internal void Add(UIElement element)
        {
            element.Root = _owner.Root;
            _elements.Add(element);
            _owner.Root.RenderComponent.QueueChange(new UIAddChildChange()
            {
                Child = _owner.BaseData,
                Parent = _owner.BaseData
            });

            OnElementAdded(element);
        }

        internal void Remove(UIElement element)
        {
            _elements.Remove(element);
            _owner.Root.RenderComponent.QueueChange(new UIRemoveChildChange()
            {
                Child = element.BaseData,
                Parent = _owner.BaseData
            });

            OnElementRemoved(element);
        }

        internal void Render(SpriteBatcher sb)
        {
            if (_owner.BaseData.IsClipEnabled)
            {
                sb.PushClip(_owner.BaseData.RenderBounds);
                foreach (UIElement child in _elements)
                    child.Render(sb);
                sb.PopClip();
            }
            else
            {
                foreach (UIElement child in _elements)
                    child.Render(sb);
            }
        }

        public IEnumerator<UIElement> GetEnumerator()
        {
            return _readOnly.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _readOnly.GetEnumerator();
        }

        public int Count => _elements.Count;

        public UIElement this[int index] => _elements[index]; 
    }
}
