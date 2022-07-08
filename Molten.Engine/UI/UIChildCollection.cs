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
        UIElement _element;

        internal UIChildCollection(UIElement parent)
        {
            _element = parent;
            _elements = new List<UIElement>();
            _readOnly = _elements.AsReadOnly();
        }

        public override string ToString()
        {
            return $"{GetType().Name} - Owner: {_element} - Count: {_readOnly.Count}";
        }

        internal void SetManager(UIManagerComponent manager)
        {
            foreach (UIElement e in _elements)
                e.Manager = manager;
        }

        public T Add<T>()
            where T : UIElement, new()
        {
            T e = new T();
            Add(e);
            return e;
        }

        public void Add(UIElement child)
        {
            if (child.Parent == _element)
                return;

            if (child.Parent != null && child.Parent != _element)
                throw new Exception("Element already has a parent. Remove from previous first.");

            // Set new element parent.
            child.Manager = _element.Manager;
            _elements.Add(child);
            child.Parent = _element;
            child.Theme = _element.Theme;

            OnElementAdded?.Invoke(child);
        }

        public void Remove(UIElement child)
        {
            if (child.Parent != _element)
                return;

            _elements.Remove(child);
            child.Parent = null;

            _element.Manager = null;
            OnElementRemoved?.Invoke(child);
        }

        internal void Render(SpriteBatcher sb)
        {
            if (_element.BaseData.IsClipEnabled)
            {
                sb.PushClip(_element.BaseData.RenderBounds);
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
