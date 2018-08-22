﻿using Molten.Collections;
using Molten.Graphics;
using Molten.Input;
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

    
    /// <summary>
    /// The base class for all types of user interface (UI) components.
    /// </summary>
    public class UIComponent : IRenderable2D, IUpdatable
    {
        /// <summary>
        /// The default background color for <see cref="UIComponent"/> and it's sub-classes.
        /// </summary>
        public static readonly Color DefaultBackgroundColor = new Color("#1b1b1c");

        /// <summary>
        /// The default border color for <see cref="UIComponent"/> and it's sub-classes.
        /// </summary>
        public static readonly Color DefaultBorderColor = new Color("#434346");

        static int _componentCounter = 0;

        /// <summary>
        /// Invoked when a child <see cref="UIComponent"/> is added to the current component.
        /// </summary>
        public event UIComponentParentHandler OnChildAdded;

        /// <summary>
        /// Invoked when a child <see cref="UIComponent"/> is removed from the current component.
        /// </summary>
        public event UIComponentParentHandler OnChildRemoved;

        /// <summary>Triggered when the user presses down with a pointing device.</summary>
        public event UIComponentEventHandler<MouseButton> OnClickStarted;

        /// <summary>Triggered when the component is released.</summary>
        public event UIComponentEventHandler<MouseButton> OnClickEnded;

        /// <summary>Triggered when the component is released while the pointer was outside of the component, but originally began inside it.</summary>
        public event UIComponentEventHandler<MouseButton> OnClickEndedOutside;

        /// <summary>Triggered when the pointer enters the bounds of the component.</summary>
        public event UIComponentEventHandler<MouseButton> OnEnter;

        /// <summary>Triggered when the pointer leaves the bounds of the component.</summary>
        public event UIComponentEventHandler<MouseButton> OnLeave;

        public event UIComponentEventHandler<MouseButton> OnHover;

        /// <summary>Triggered when the component is being dragged.</summary>
        public event UIComponentEventHandler<MouseButton> OnDrag;

        /// <summary>Triggered when the component is being held (most likely via touch screen).</summary>
        public event UIComponentEventHandler<MouseButton> OnHold;

        public event UIComponentEventHandler<MouseButton> OnFocus;

        public event UIComponentEventHandler<MouseButton> OnUnfocus;

        /// <summary>Triggered when the scrollwheel is used while the mouse is over the component.</summary>
        public event UIComponentEventHandler<MouseButton> OnScrollWheel;

        protected List<UIComponent> _children;
        protected List<UIComponent> _childRenderList;
        protected Dictionary<string, UIComponent> _childrenByName;
        bool _renderListDirty;

        int _lockerValue = 0;
        Thread _lockOwner = null;

        UIComponent _parent;
        Scene _scene;
        string _name;

        protected Rectangle _localBounds;
        Rectangle _globalBounds;
        Rectangle _clippingBounds;

        UIMargin _margin;
        bool _clippingEnabled;


        /// <summary>
        /// Creates a new instance of <see cref="UIComponent"/>
        /// </summary>
        public UIComponent()
        {
            _children = new List<UIComponent>();
            _childRenderList = new List<UIComponent>();
            _childrenByName = new Dictionary<string, UIComponent>();
            _margin = new UIMargin();
            _margin.OnChanged += _margin_OnChanged;
            _name = $"{this.GetType().Name}{Interlocked.Increment(ref _componentCounter)}";

            ClipPadding = new UIPadding(0);
            ClipPadding.OnChanged += _clipPadding_OnChanged;
        }

        private void _margin_OnChanged(UIMargin o)
        {
            UpdateBounds();
        }

        private void _clipPadding_OnChanged(UIPadding o)
        {
            UpdateBounds();
        }

        /// <summary>[Virtual] Calculates whether or not the component or one of its children contain the provided 2D position.</summary>
        /// <param name="inputPos">The position.</param>
        /// <returns>The component that contains the provided position.</returns>
        public virtual UIComponent GetComponent(Vector2F inputPos)
        {
            if (!IsVisible)
                return null;

            //check if child input should be ignored
            if (!IgnoreChildInput)
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
            if (!IsEnabled)
                return null;

            // Check if the mouse was clicked over this component
            if (Contains(inputPos))
                return this;

            return null;
        }

        protected void InvokeDrag(Vector2F inputPos, Vector2F inputDelta, MouseButton button)
        {
            if (OnDrag != null)
            {
                if (inputDelta.Length() != 0)
                {
                    OnDrag.Invoke(new UIEventData<MouseButton>()
                    {
                        Component = this,
                        Position = inputPos,
                        Delta = inputDelta,
                        InputValue = button,
                        WasDragged = true,
                    });
                }
            }
        }

        /// <summary>Forcefully triggers a right tap action on the component.</summary>
        /// <param name="inputPos"></param>
        protected void InvokePressStarted(Vector2F inputPos, MouseButton button)
        {
            OnClickStarted?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = Vector2F.Zero,
                InputValue = button,
                WasDragged = false,
            });
        }

        /// <summary>Forcefully triggers a release tap action on the component.</summary>
        /// <param name="inputPos"></param>
        protected void InvokePressCompleted(Vector2F inputPos, bool wasDragged, MouseButton button)
        {
            OnClickEnded?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = Vector2F.Zero,
                InputValue = button,
                WasDragged = wasDragged,
            });
        }

        /// <summary>Forcefully triggers a release tap action on the component.</summary>
        /// <param name="inputPos"></param>
        protected void InvokeCompletedOutside(Vector2F inputPos, Vector2F inputDelta, MouseButton button)
        {
            OnClickEndedOutside?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = inputDelta,
                InputValue = button,
                WasDragged = inputDelta.Length() != 0,
            });
        }

        protected void InvokeHold(Vector2F inputPos, bool wasDragged, MouseButton button)
        {
            OnHold?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = Vector2F.Zero,
                InputValue = button,
                WasDragged = wasDragged,
            });
        }

        protected void InvokeEnter(Vector2F inputPos)
        {
            OnEnter?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = Vector2F.Zero,
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        protected void InvokeLeave(Vector2F inputPos)
        {
            OnLeave?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = Vector2F.Zero,
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        protected void InvokeFocus()
        {
            OnFocus?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = _globalBounds.TopLeft,
                Delta = Vector2F.Zero,
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        protected void InvokeUnfocus()
        {
            OnUnfocus?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = _globalBounds.TopLeft,
                Delta = Vector2F.Zero,
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        protected void InvokeHover(Vector2F inputPos)
        {
            OnHover?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = Vector2F.Zero,
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        protected void InvokeScrollWheel(float delta)
        {
            OnScrollWheel?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = _globalBounds.TopLeft,
                Delta = new Vector2F(0, delta),
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        /// <summary>Returns true if the given location is inside/over the component's bounds.</summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual bool Contains(Vector2F location)
        {
            bool passed = true;

            // Check if the mouse was clicked over this component
            if (_parent != null)
                passed = _parent._clippingBounds.Contains(location);

            if (passed)
                passed = _globalBounds.Contains(location);

            return passed;
        }

        /// <summary>
        /// Updates the current <see cref="UIComponent"/>. When the component is added to a scene, this will be called automatically.
        /// </summary>
        /// <param name="time">A <see cref="Timing"/> instance.</param>
        public void Update(Timing time)
        {
            if (!IsEnabled)
                return;

            LockChildren(() =>
            {
                for (int i = 0; i < _children.Count; i++)
                    _children[i].Update(time);
            });

            OnUpdate(time);
        }

        protected virtual void OnUpdate(Timing time) { }

        /// <summary>
        /// Renders the current <see cref="UIComponent"/> with the provided <see cref="SpriteBatch"/>. When the component is added to a scene, this will be called automatically.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> that will perform the render operation.</param>
        public void Render(SpriteBatch sb)
        {
            if (!IsVisible)
                return;

            // TODO find a better solution for this lock, if possible.
            // It's a potential bottleneck in the renderer if another thread holds the lock when the render thread hits it.
            if (_renderListDirty)
            {
                LockChildren(() =>
                {
                    _childRenderList.Clear();
                    _childRenderList.AddRange(_children);
                    _renderListDirty = false;
                });
            }

            OnRender(sb);
        }

        protected virtual void OnRender(SpriteBatch sb)
        {
            if(_clippingEnabled)
            {
                sb.PushClip(_clippingBounds);
                for (int i = 0; i < _childRenderList.Count; i++)
                    _childRenderList[i].Render(sb);
                sb.PopClip();
            }
            else
            {
                for (int i = 0; i < _childRenderList.Count; i++)
                    _childRenderList[i].Render(sb);
            }
        }


        protected void LockChildren(Action callback)
        {
            // If the same thread which holds the lock is calling again, skip the lock logic.
            // This is so nested lock calls on the same thread cannot cause it to deadlock itself.
            if (_lockOwner != Thread.CurrentThread)
            {
                SpinWait spin = new SpinWait();
                while (0 != Interlocked.Exchange(ref _lockerValue, 1))
                    spin.SpinOnce();

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

        /// <summary>
        /// Adds a <see cref="UIComponent"/> as a child to the current <see cref="UIComponent"/>. 
        /// The child will be removed from its previous parent if one is set.
        /// </summary>
        /// <param name="child">The <see cref="UIComponent"/> to add as a child.</param>
        public virtual void AddChild(UIComponent child)
        {
            if (child == this)
                throw new UIException(this, "Cannot add a UI component to itself.");

            LockChildren(() =>
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
                _renderListDirty = true;
            });

            OnChildAdded?.Invoke(this, child);
        }

        /// <summary>
        /// Removes a child <see cref="UIComponent"/> from the current <see cref="UIComponent"/>.
        /// </summary>
        /// <param name="child"></param>
        public virtual void RemoveChild(UIComponent child)
        {
            if (child == this)
                throw new UIException(this, "Cannot remove a UI component from itself.");

            LockChildren(() =>
            {
                if (child.Parent != this)
                    ThrowReleaseLock("Unable to remove child because it does not belong to the current UI component.");

                _children.Remove(child);
                _childrenByName.Remove(child.Name);
                child.Parent = null;
                child.Scene = null;
                _renderListDirty = false;
            });

            OnChildRemoved?.Invoke(this, child);
        }

        /// <summary>Updates the bounds (position, width, height, etc) of all child components.</summary>
        protected internal void UpdateBounds()
        {
            OnPreUpdateBounds();
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

            ClipPadding.SuppressEvents = true;
            OnApplyClipPadding();
            _clippingBounds = ClipPadding.ApplyPadding(_globalBounds);

            // Force the clip bounds to fit inside its parent and never go outside of it, if clipping is enabled.
            if (_parent != null && _clippingEnabled)
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

            ClipPadding.SuppressEvents = false;

            // Update bounds of children
            LockChildren(() =>
            {
                foreach (UIComponent child in _children)
                    child.UpdateBounds();
            });

            OnPostUpdateBounds();
        }

        /// <summary>
        /// Occurs just before any changes to the bounds of the current <see cref="UIComponent"/> are made..
        /// </summary>
        protected virtual void OnPreUpdateBounds() { }

        /// <summary>
        /// Occurs just after changes to the bounds of the current <see cref="UIComponent"/> have been made.
        /// </summary>
        protected virtual void OnPostUpdateBounds() { }

        /// <summary>Called right before padding is applied to the global bounds to form the clipping bounds.</summary>
        protected virtual void OnApplyClipPadding() { }

        /// <summary>
        /// Invoked when the component's parent has been changed.
        /// </summary>
        protected virtual void OnParentChanged() { }

        /// <summary>
        /// Gets the scene that the current <see cref="UIComponent"/> is bound to, or null if not bound to any scene.
        /// </summary>
        public Scene Scene
        {
            get => _scene;
            internal set => _scene = value;
        }

        Scene ISceneObject.Scene
        {
            get => _scene;
            set => Scene = value;
        }

        /// <summary>
        /// Gets the parent of the UI 
        /// </summary>
        public UIComponent Parent
        {
            get => _parent;
            internal set
            {
                if (_parent != value)
                {
                    _parent = value;
                    OnParentChanged();
                    UpdateBounds();
                }
            }
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
                LockChildren(() => _childrenByName.TryGetValue(name, out result));
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
                LockChildren(() =>
                {
                    result = _children[index];
                });
                return result;
            }
        }

        /// <summary>
        /// Gets the global bounds of the current <see cref="UIComponent"/>. This the final screen bounds of the component after all other properties have been taken into account (i.e. margins, padding, parent bounds, etc).
        /// </summary>
        public Rectangle GlobalBounds => _globalBounds;

        /// <summary>
        /// Gets the clipping bounds of the current <see cref="UIComponent"/>. This is the area in which the component's children are rendered into.
        /// </summary>
        public Rectangle ClippingBounds => _clippingBounds;

        /// <summary>
        /// Gets or sets whether or not clipping is enabled for the current <see cref="UIComponent"/>. The default value is false. If enabled, any child components will be clipped to the bounds
        /// of the current <see cref="UIComponent.ClippingBounds"/>.
        /// </summary>
        public bool IsClippingEnabled
        {
            get => _clippingEnabled;
            set
            {
                if(_clippingEnabled != value)
                {
                    _clippingEnabled = value;
                    UpdateBounds();
                }
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

        /// <summary>
        /// Gets the <see cref="UIPadding"/> instance containing padding values for determining the clipping region of the current <see cref="UIComponent"/>.
        /// </summary>
        public UIPadding ClipPadding { get; }

        /// <summary>
        /// Gets the <see cref="UIMargin"/> instance containing margin values for the current <see cref="UIComponent"/>.
        /// </summary>
        public UIMargin Margin => _margin;

        /// <summary>
        /// Gets or sets the local bounds of the current <see cref="UIComponent"/>.
        /// </summary>
        public Rectangle LocalBounds
        {
            get => _localBounds;
            set
            {
                _localBounds = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the X position of the current <see cref="UIComponent"/>.</summary>
        public int X
        {
            get { return _localBounds.X; }
            set
            {
                _localBounds.X = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the Y position of the current <see cref="UIComponent"/>.</summary>
        public int Y
        {
            get { return _localBounds.Y; }
            set
            {
                _localBounds.Y = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the width of the current <see cref="UIComponent"/>.</summary>
        public int Width
        {
            get { return _localBounds.Width; }
            set
            {
                _localBounds.Width = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the height of the current <see cref="UIComponent"/>.</summary>
        public int Height
        {
            get { return _localBounds.Height; }
            set
            {
                _localBounds.Height = value;
                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="UIComponent"/> is enabled. If disabled, the component and it's child components will not update or respond to input.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the current <see cref="UIComponent"/> is rendered. If disabled, the component and it's child components will not be rendered.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets whether all of the child components of the current <see cref="UIComponent"/> will ignore input. The default value is false.
        /// </summary>
        public bool IgnoreChildInput { get; set; } = false;

        /// <summary>
        /// Gets or sets whether drag input of child components is ignored for the current <see cref="UIComponent"/>. The default value is false.
        /// </summary>
        public bool IgnoreChildDrag { get; set; } = false;
    }
}
