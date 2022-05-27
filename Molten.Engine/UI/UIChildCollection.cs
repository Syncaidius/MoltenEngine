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

        public T Add<T>()
            where T : UIElement, new()
        {
            T e = new T();
            Add(e);
            return e;
        }

        public void Add(UIElement element)
        {
            if (element.Parent == _owner)
                return;

            // Remove from old parent, if any.
            if (element.Parent != null)
                element.Parent.Children.Remove(element);

            // Set new element parent.
            element.Root = _owner.Root;
            _elements.Add(element);
            _owner.Root.RenderComponent.QueueChange(new UIAddChildChange()
            {
                Child = _owner.BaseData,
                Parent = _owner.BaseData
            });

            OnElementAdded?.Invoke(element);
        }

        public void Remove(UIElement element)
        {
            if (element.Parent != _owner)
                return;

            _elements.Remove(element);
            element.Parent = null;

            _owner.Root.RenderComponent.QueueChange(new UIRemoveChildChange()
            {
                Child = element.BaseData,
                Parent = _owner.BaseData
            });

            OnElementRemoved?.Invoke(element);
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
