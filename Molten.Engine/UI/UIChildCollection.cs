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
            child.Owner = _element.Owner;
            _elements.Add(child);
            child.Parent = _element;

            _element.Engine.Scenes.QueueChange(null, new SceneUIAddChild()
            {
                Child = _element.BaseData,
                Parent = _element.BaseData
            });

            OnElementAdded?.Invoke(child);
        }

        public void Remove(UIElement child)
        {
            if (child.Parent != _element)
                return;

            _elements.Remove(child);
            child.Parent = null;

            _element.Engine.Scenes.QueueChange(null, new SceneUIRemoveChild()
            {
                Child = child.BaseData,
                Parent = _element.BaseData
            });

            _element.Owner = null;
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
