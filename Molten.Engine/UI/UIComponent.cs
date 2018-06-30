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

        List<UIComponent> _children;
        Dictionary<string, UIComponent> _childrenByName;

        int _lockerValue = 0;
        Thread _lockOwner = null;

        UIComponent _parent;
        Scene _scene;
        string _name;

        Rectangle _localBounds;
        Rectangle _globalBounds;
        Rectangle _clippingBounds;

        UIMargin _margin;
        UIPadding _clipPadding;

        public UIComponent()
        {
            _children = new List<UIComponent>();
            _childrenByName = new Dictionary<string, UIComponent>();
            _margin = new UIMargin();
            _margin.OnChanged += _margin_OnChanged;

            _clipPadding = new UIPadding(0);
            _clipPadding.OnChanged += _clipPadding_OnChanged;
        }

        private void _margin_OnChanged(UIMargin o)
        {
            Lock(UpdateBounds);
        }

        private void _clipPadding_OnChanged(UIPadding o)
        {
            Lock(UpdateBounds);
        }

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
            internal set
            {
                _parent = value;
                UpdateBounds();
            }
        }

        public Scene Scene
        {
            get => _scene;
            internal set => _scene = value;
        }

        private void Lock(Action callback)
        {
            // If the same thread which holds the lock is calling again, skip the lock logic.
            // This is so nested lock calls on the same thread cannot cause it to deadlock itself.
            if (_lockOwner != Thread.CurrentThread)
            {
                SpinWait spin = new SpinWait();
                while (0 != Interlocked.Exchange(ref _lockerValue, 1))
                {
                    spin.SpinOnce();
                }

                _lockOwner = Thread.CurrentThread;
                callback();
                _lockOwner = null;
                Interlocked.Exchange(ref _lockerValue, 0);
            }
            else
            {
                callback();
            }
        }

        private void ThrowReleaseLock(string message)
        {
            _lockOwner = null;
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

        /// <summary>Updates the bounds (position, width, height, etc) of all child components.</summary>
        protected void UpdateBounds()
        {
            Rectangle parentBounds = _parent != null ? _parent.ClippingBounds : Rectangle.Empty;

            //handle margins
            if (_parent != null)
            {
                if (_margin.DockLeft)
                    _localBounds.X = _margin.Left;

                if (_margin.DockRight)
                {
                    if (_margin.DockLeft)
                        _localBounds.Width = parentBounds.Width - (_localBounds.X + _margin.Right);
                    else
                        _localBounds.X = parentBounds.Width - (_localBounds.Width + _margin.Right);
                }

                if (_margin.DockTop)
                    _localBounds.Y = _margin.Top;

                if (_margin.DockBottom)
                {
                    if (_margin.DockTop)
                        _localBounds.Height = parentBounds.Height - (_localBounds.Y + _margin.Bottom);
                    else
                        _localBounds.Y = parentBounds.Height - (_localBounds.Height + _margin.Bottom);
                }
            }

            _globalBounds = new Rectangle()
            {
                X = (int)parentBounds.X + _localBounds.X,
                Y = (int)parentBounds.Y + _localBounds.Y,
                Width = _localBounds.Width,
                Height = _localBounds.Height,
            };

            _clipPadding.SuppressEvents = true;
            OnApplyClipPadding();
            _clippingBounds = _clipPadding.ApplyPadding(_globalBounds);

            //force the clip bounds to fit inside its parent and never go outside of it.
            if (_parent != null)
            {
                Rectangle _parentClip = _parent.ClippingBounds;

                //clip left and top sides
                if (_clippingBounds.X < _parentClip.X)
                {
                    int xDif = _parentClip.X - _clippingBounds.X;

                    _clippingBounds.Width = Math.Max(_clippingBounds.Width - xDif, 0);
                    _clippingBounds.X = _parentClip.X;
                }

                if (_clippingBounds.Y < _parentClip.Y)
                {
                    int yDif = _parentClip.Y - _clippingBounds.Y;

                    _clippingBounds.Height = Math.Max(_clippingBounds.Height - yDif, 0);
                    _clippingBounds.Y = _parentClip.Y;
                }

                //cap bottom and right sides (offset by 1)
                if (_clippingBounds.X > _parentClip.Right - 1)
                    _clippingBounds.X = _parentClip.Right - 1;

                if (_clippingBounds.Y > _parentClip.Bottom - 1)
                    _clippingBounds.Y = _parentClip.Bottom - 1;


                if (_clippingBounds.Right >= _parentClip.Right)
                    _clippingBounds.Width = _parentClip.Right - _clippingBounds.X;

                if (_clippingBounds.Bottom >= _parentClip.Bottom)
                    _clippingBounds.Height = _parentClip.Bottom - _clippingBounds.Y;
            }

            OnUpdateBounds();
            _clipPadding.SuppressEvents = false;

            //update bounds of children
            foreach (UIComponent child in _children)
                child.UpdateBounds();
        }

        /// <summary>Called right before padding is applied to the global bounds to form the clipping bounds.</summary>
        protected virtual void OnApplyClipPadding() { }

        /// <summary>
        /// Called after the current <see cref="UIComponent"/> bounds are recalculated after a change.
        /// </summary>
        protected virtual void OnUpdateBounds() { }

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
        /// Gets the global bounds of the current <see cref="UIComponent"/>.
        /// </summary>
        public Rectangle GlobalBounds
        {
            get { return _globalBounds; }
        }

        /// <summary>
        /// Gets the clipping bounds of the current <see cref="UIComponent"/>.
        /// </summary>
        public Rectangle ClippingBounds
        {
            get { return _clippingBounds; }
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

        /// <summary>
        /// Gets the <see cref="UIPadding"/> instance containing padding values for determining the clipping region of the current <see cref="UIComponent"/>.
        /// </summary>
        public UIPadding ClipPadding
        {
            get { return _clipPadding; }
        }

        /// <summary>
        /// Gets the <see cref="UIMargin"/> instance containing margin values for the current <see cref="UIComponent"/>.
        /// </summary>
        public UIMargin Margin
        {
            get { return _margin; }
        }

        /// <summary>
        /// Gets or sets the local bounds of the current <see cref="UIComponent"/>.
        /// </summary>
        public Rectangle LocalBounds
        {
            get => _localBounds;
            set
            {
                _localBounds = value;
                Lock(UpdateBounds);
            }
        }

        /// <summary>Gets or sets the X position of the current <see cref="UIComponent"/>.</summary>
        public int X
        {
            get { return _localBounds.X; }
            set
            {
                _localBounds.X = value;
                Lock(UpdateBounds);
            }
        }

        /// <summary>Gets or sets the Y position of the current <see cref="UIComponent"/>.</summary>
        public int Y
        {
            get { return _localBounds.Y; }
            set
            {
                _localBounds.Y = value;
                Lock(UpdateBounds);
            }
        }

        /// <summary>Gets or sets the width of the current <see cref="UIComponent"/>.</summary>
        public int Width
        {
            get { return _localBounds.Width; }
            set
            {
                _localBounds.Width = value;
                Lock(UpdateBounds);
            }
        }

        /// <summary>Gets or sets the height of the current <see cref="UIComponent"/>.</summary>
        public int Height
        {
            get { return _localBounds.Height; }
            set
            {
                _localBounds.Height = value;
                Lock(UpdateBounds);
            }
        }
    }
}
