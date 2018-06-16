using Molten.Collections;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.UI
{
    public delegate void UIComponentHandler(UIComponent component);
    public delegate void UIComponentParentHandler(UIComponent parent, UIComponent child);

    public abstract class UIComponent : IRenderable2D, IUpdatable
    {
        public event UIComponentParentHandler OnChildAdded;

        public event UIComponentParentHandler OnChildRemoved;


        int _lockerValue = 0;
        List<UIComponent> _children = new List<UIComponent>();
        Dictionary<string, UIComponent> _childrenByName = new Dictionary<string, UIComponent>();
        UIComponent _parent;
        Scene _scene;
        string _name;

        public void Update(Timing time)
        {
            throw new NotImplementedException();
        }

        public void Render(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the parent of the UI 
        /// </summary>
        public UIComponent Parent
        {
            get => _parent;
            internal set => _parent = value;
        }

        public Scene Scene
        {
            get => _scene;
            internal set => _scene = value;
        }

        private void Lock(Action callback)
        {
            SpinWait spin = new SpinWait();
            while (0 != Interlocked.Exchange(ref _lockerValue, 1))
            {
                spin.SpinOnce();
            }

            callback();

            Interlocked.Exchange(ref _lockerValue, 0);
        }

        private void Lock(Func<UIComponent> callback)
        {
            SpinWait spin = new SpinWait();
            while (0 != Interlocked.Exchange(ref _lockerValue, 1))
            {
                spin.SpinOnce();
            }

            callback();

            Interlocked.Exchange(ref _lockerValue, 0);
        }

        private void ThrowReleaseLock(string message)
        {
            Interlocked.Exchange(ref _lockerValue, 0);
            throw new UIException(this, message);

        }
        Scene IUpdatable.Scene
        {
            get => _scene;
            set => Scene = value;
        }

        /// <summary>
        /// Adds a <see cref="UIComponent"/> as a child to the current <see cref="UIComponent"/>. 
        /// The child will be removed from its previous parent if one is set.
        /// </summary>
        /// <param name="child">The <see cref="UIComponent"/> to add as a child.</param>
        public void AddChild(UIComponent child)
        {
            if (child == this)
                throw new UIException(this, "Cannot add a UI component to itself.");

            Lock(() =>
            {
                if (child.Parent == this)
                    return;
                else
                    child.Parent?.RemoveChild(child);

                if (_childrenByName.ContainsKey(child.Name))
                    ThrowReleaseLock($"Another child with the same name ({child.Name}) already exists on the current UI component.");

                _children.Add(child);
                _childrenByName.Add(child.Name, child);
                child.Parent = this;
                child.Scene = _scene;
            });

            OnChildAdded?.Invoke(this, child);
        }

        /// <summary>
        /// Removes a child <see cref="UIComponent"/> from the current <see cref="UIComponent"/>.
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(UIComponent child)
        {
            if (child == this)
                throw new UIException(this, "Cannot remove a UI component from itself.");

            Lock(() =>
            {
                if (child.Parent != this)
                    ThrowReleaseLock("Unable to remove child because it does not belong to the current UI component.");

                _children.Remove(child);
                _childrenByName.Remove(child.Name);
                child.Parent = null;
                child.Scene = null;
            });

            OnChildRemoved?.Invoke(this, child);
        }

        /// <summary>
        /// Gets a child of the current <see cref="UIComponent"/>, with the specified name, or null if it doesn't exist.
        /// </summary>
        /// <param name="name">The name of the child component.</param>
        /// <returns></returns>
        public UIComponent this[string name]
        {
            get
            {
                UIComponent result = null;
                Lock(() => _childrenByName.TryGetValue(name, out result));
                return result;
            }
        }

        /// <summary>
        /// Gets a child of the current <see cref="UIComponent"/>, at the specified index.
        /// </summary>
        /// <param name="index">The child index.</param>
        /// <returns></returns>
        public UIComponent this[int index]
        {
            get
            {
                UIComponent result = null;
                Lock(() =>
                {
                    result = _children[index];
                });
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the name of the current <see cref="UIComponent"/>.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                // TODO if the name has changed, update name on parent (via a lock)
                throw new NotImplementedException();
            }
        }
    }
}
