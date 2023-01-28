using System.Collections;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIElementLayer : IEnumerable<UIElement>
    {
        public event ObjectHandler<UIElement> OnElementAdded;

        public event ObjectHandler<UIElement> OnElementRemoved;

        List<UIElement> _elements;
        IReadOnlyList<UIElement> _readOnly;
        UIElement _element;


        internal UIElementLayer(UIElement parent, UIElementLayerBoundsUsage boundsUsage, Type[] filter)
        {
            _element = parent;
            _elements = new List<UIElement>();
            _readOnly = _elements.AsReadOnly();
            Filter = new UIElementFilter(filter);
            BoundsUsage = boundsUsage;
        }

        public override string ToString()
        {
            return $"{GetType().Name} - Owner: {_element} - Count: {_readOnly.Count}";
        }

        public T Add<T>(Rectangle? localBounds = null)
            where T : UIElement, new()
        {
            T e = new T();

            if (localBounds != null)
                e.LocalBounds = localBounds.Value;

            Add(e);

            return e;
        }

        public void Add(UIElement child)
        {
            if (child.ParentElement == _element)
                return;

            if (child.ParentElement != null && child.ParentElement != _element)
                throw new Exception("Element already has a parent. Remove from previous parent first.");

            if (!Filter.IsAccepted(child))
                throw new Exception("Child element type does not match the layer's filter");

            // Set new element parent.
            child.Manager = _element.Manager;
            _elements.Add(child);
            child.ParentLayer = this;
            child.Theme = _element.Theme;

            if (_element is UIWindow window)
                child.ParentWindow = window;
            else
                child.ParentWindow = _element.ParentWindow;

            OnElementAdded?.Invoke(child);
        }

        public void Remove(UIElement child)
        {
            if (child.ParentElement != _element)
                return;

            _elements.Remove(child);
            child.ParentLayer = null;
            child.ParentWindow = null;

            _element.Manager = null;
            OnElementRemoved?.Invoke(child);
        }

        internal void BringToFront(UIElement child)
        {
            if (child.ParentElement != _element || _elements.LastOrDefault() == child)
                return;

            _elements.Remove(child);
            _elements.Add(child);
        }

        internal void SendToBack(UIElement child)
        {
            if (child.ParentElement != _element || _elements.FirstOrDefault() == child)
                return;

            _elements.Remove(child);
            _elements.Insert(0, child);
        }

        public bool IsAtFront(UIElement child)
        {
            if (child.ParentElement != _element)
                return false;

            return _elements.LastOrDefault() == child;
        }

        public bool IsAtBack(UIElement child)
        {
            if (child.ParentElement != _element)
                return false;

            return _elements.FirstOrDefault() == child;
        }

        internal void Render(SpriteBatcher sb)
        {
            if (!IsEnabled || _elements.Count == 0)
                return;

            // We only need to push a new clip if we're in render-bounds clip mode.
            // Global bounds are already clipped, so we don't need to push another for it.
            if (BoundsUsage != UIElementLayerBoundsUsage.GlobalBounds)
            {
                if (sb.PushClip(_element.RenderBounds))
                {
                    for (int i = 0; i < _elements.Count; i++)
                        _elements[i].Render(sb);

                    sb.PopClip();
                }
            }
            else
            {
                for (int i = 0; i < _elements.Count; i++)
                    _elements[i].Render(sb);
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

        /// <summary>
        /// Gets or sets whether the given layer is enabled. If false, the layer will not render or update.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether not the current <see cref="UIElementLayer"/> should ignore input.
        /// </summary>
        public bool IgnoreInput { get; set; }

        public UIElementLayerBoundsUsage BoundsUsage { get; }

        /// <summary>
        /// Gets the <see cref="UIElement"/> that owns the current <see cref="UIElementLayer"/>.
        /// </summary>
        public UIElement Owner => _element;

        /// <summary>
        /// Gets the filter for the current <see cref="UIElementLayer"/>.
        /// </summary>
        public UIElementFilter Filter { get; }

        public UIElement this[int index] => _elements[index]; 
    }

    /// <summary>
    /// Determines which bounds of a parent <see cref="UIElement"/> will be used to constrain rendering of elements within a <see cref="UIElementLayer"/>.
    /// </summary>
    public enum UIElementLayerBoundsUsage
    {
        GlobalBounds = 0,

        RenderBounds = 1,
    }
}
