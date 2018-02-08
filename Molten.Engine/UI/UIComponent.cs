using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Molten.Utilities;
using Molten.IO;
using System.ComponentModel;
using Molten.Graphics;
using Molten.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Molten.Collections;

namespace Molten.UI
{
    [JsonConverter(typeof(UIJsonConverter))]
    [DataContract]
    public abstract class UIComponent : EngineObject, ISprite, IUpdatable
    {
        protected static UISystem _ui;

        protected bool _enableClipping;
        protected string _tooltip = "";

        protected Rectangle _localBounds; //the bounds of the component in local space (relative to its parent).
        protected Rectangle _globalBounds; //the bounds of the component in screen/global space.
        protected Rectangle _clippingBounds;
        protected UIPadding _clipPadding; //how many pixels on either side will be used to seperate the clipping bounds from the border of the component.
        protected UIMargin _margin; //how many pixels on either side will seperate the component's borders from its parent's borders.
        protected UIComponent _parent;
        protected Engine _engine;
        protected object _tagObject;

        protected bool _enabled;
        protected bool _visible;
        protected bool _ignoreChildInput;
        protected bool _ignoreChildDrag;

        protected ThreadedList<UIComponent> _children;

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

        /// <summary>Triggered when a child component is added.</summary>
        public event UIComponentEventHandler<MouseButton> OnChildAdded;

        /// <summary>triggered when a child component is removed.</summary>
        public event UIComponentEventHandler<MouseButton> OnChildRemoved;

        public event UIComponentEventHandler<MouseButton> OnFocus;

        public event UIComponentEventHandler<MouseButton> OnUnfocus;

        /// <summary>Triggered when the scrollwheel is used while the mouse is over the component.</summary>
        public event UIComponentEventHandler<MouseButton> OnScrollWheel;

        public UIComponent(Engine engine)
        {
            _engine = engine;
            _children = new ThreadedList<UIComponent>();
            _margin = new UIMargin();
            _margin.OnChanged += _margin_OnChanged;

            _localBounds = new Rectangle();
            _enabled = true;
            Name = this.GetType().Name;
            _visible = true;
            _ignoreChildInput = false;
            _ignoreChildDrag = false;

            _clipPadding = new UIPadding(0);
            _clipPadding.OnChanged += _clipPadding_OnChanged;
        }

        private void _clipPadding_OnChanged(UIPadding o)
        {
            UpdateBounds();
        }

        private void _margin_OnChanged(UIMargin o)
        {
            UpdateBounds();
        }

        protected override void OnDispose()
        {
            if (_parent != null)
                _parent.RemoveChild(this);
        }

        public void Focus()
        {
            _ui.SetFocus(this);
        }

        public void UnFocus()
        {
            if (_ui.Focused == this)
                _ui.Unfocus();
        }

        /// <summary>Checks through all child components to see if one has the specified name. If so, it will be returned.</summary>
        /// <param name="componentName">The name of the component to look for.</param>
        /// <returns>A component, if found.</returns>
        public UIComponent this[string componentName]
        {
            get
            {
                UIComponent component = null;

                if (Name == componentName)
                    return this;

                foreach (UIComponent child in _children)
                {
                    component = child[componentName];
                    if (component != null)
                        return component;
                }

                return null;
            }
        }

        /// <summary>Checks through all child components to see if one has the specified name, but ignores the specified exception component.</summary>
        /// <param name="componentName"></param>
        /// <param name="exceptionComponent">The component to make an exception for in the search to skip it.</param>
        /// <returns></returns>
        public UIComponent this[string componentName, UIComponent exceptionComponent]
        {
            get
            {
                UIComponent component = null;

                if (Name == componentName && this != exceptionComponent)
                    return this;

                foreach (UIComponent child in _children)
                {
                    component = child[componentName];
                    if (component != null)
                        return component;
                }

                return null;
            }
        }

        public UIComponent HasChild(string childName)
        {
            foreach (UIComponent c in _children)
                if (c.Name == childName)
                    return c;

            // No matching name, return false
            return null;
        }

        /// <summary>Returns true if the provided <see cref="UIComponent"/> is a child of the current component.</summary>
        /// <param name="com">The component to test against the current parent component.</param>
        /// <returns></returns>
        public bool IsChild(UIComponent com)
        {
            UIComponent c = null;
            for(int i = 0; i < _children.Count; i++)
            {
                c = _children[i];
                if (c.IsChild(com))
                    return true;
            }
            
            // No match.
            return false;
        }

        public virtual void AddChild(UIComponent component)
        {
            if (_children.Contains(component) == false)
            {
                _children.Add(component);
                component.Parent = this;

                //trigger on child addition event
                Rectangle childBounds = component.LocalBounds;
                if (OnChildAdded != null)
                {
                    OnChildAdded.Invoke(new UIEventData<MouseButton>()
                    {
                        Component = component,
                        Position = new Vector2(childBounds.X, childBounds.Y),
                        Delta = new Vector2(),
                        InputValue = MouseButton.None,
                        WasDragged = false,
                    });
                }
            }
        }

        public virtual bool RemoveChild(UIComponent component)
        {
            bool removed = _children.Remove(component);
            if (removed)
            {
                component.OnRemoved();
                component.Parent = null;

                //trigger on child removal event.
                Rectangle childBounds = component.LocalBounds;
                OnChildRemoved?.Invoke(new UIEventData<MouseButton>()
                {
                    Component = component,
                    Position = new Vector2(childBounds.X, childBounds.Y),
                    Delta = new Vector2(),
                    InputValue = MouseButton.None,
                    WasDragged = false,
                });
            }

            return removed;
        }

        public virtual void OnRemoved()
        {
            foreach (UIComponent child in _children)
                child.OnRemoved();
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>Brings the component to the front of all other components that share the same parent.</summary>
        public void BringToFront()
        {
            if (_parent != null)
            {
                _parent.Children.Remove(this);
                _parent.Children.Add(this);
            }
        }

        /// <summary>Pushes the component behind all other components that share the same parent.</summary>
        public void SendToBack()
        {
            if (_parent != null)
            {
                _parent.Children.Remove(this);
                _parent.Children.Insert(0, this);
            }
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

        protected virtual void OnUISystemChanged(UISystem oldSystem, UISystem newSystem) { }

        /// <summary>Called right before padding is applied to the global bounds to form the clipping bounds.</summary>
        protected virtual void OnApplyClipPadding() { }

        protected virtual void OnUpdateBounds() { }

        public UIComponent GetComponent(Vector2 inputPos)
        {
            //if the component isn't visible, neither are its children. Don't handle input.
            return OnGetComponent(inputPos);
        }

        /// <summary>[Virtual] Calculates whether or not the component or one of its children contain the provided 2D position.</summary>
        /// <param name="inputPos">The position.</param>
        /// <returns>The component that contains the provided position.</returns>
        protected virtual UIComponent OnGetComponent(Vector2 inputPos)
        {
            if (!_visible) return null;

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

            // Check if the mouse was clicked over this component
            if (Contains(inputPos))
                return this;

            return null;
        }

        /// <summary>Returns true if the given location is inside/over the component's bounds.</summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual bool Contains(Vector2 location)
        {
            bool passed = true;

            // Check if the mouse was clicked over this component
            if (_parent != null)
                passed = _parent._clippingBounds.Contains(location);

            if (passed)
                passed = _globalBounds.Contains(location);

            return passed;
        }

        public void InvokeDrag(Vector2 inputPos, Vector2 inputDelta, MouseButton button)
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
        public void InvokePressStarted(Vector2 inputPos, MouseButton button)
        {
            OnClickStarted?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = new Vector2(),
                InputValue = button,
                WasDragged = false,
            });
        }

        /// <summary>Forcefully triggers a release tap action on the component.</summary>
        /// <param name="inputPos"></param>
        public void InvokePressCompleted(Vector2 inputPos, bool wasDragged, MouseButton button)
        {
            OnClickEnded?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = new Vector2(),
                InputValue = button,
                WasDragged = wasDragged,
            });
        }

        /// <summary>Forcefully triggers a release tap action on the component.</summary>
        /// <param name="inputPos"></param>
        public void InvokeCompletedOutside(Vector2 inputPos, Vector2 inputDelta, MouseButton button)
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

        public void InvokeHold(Vector2 inputPos, bool wasDragged, MouseButton button)
        {
            OnHold?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = new Vector2(),
                InputValue = button,
                WasDragged = wasDragged,
            });
        }

        public void InvokeEnter(Vector2 inputPos)
        {
            OnEnter?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = new Vector2(),
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        public void InvokeLeave(Vector2 inputPos)
        {
            OnLeave?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = new Vector2(),
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        internal void InvokeFocus()
        {
            OnFocus?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = _globalBounds.TopLeft,
                Delta = new Vector2(),
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        internal void InvokeUnfocus()
        {
            OnUnfocus?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = _globalBounds.TopLeft,
                Delta = new Vector2(),
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        internal void InvokeHover(Vector2 inputPos)
        {
            OnHover?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = inputPos,
                Delta = new Vector2(),
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        internal void InvokeScrollWheel(float delta)
        {
            OnScrollWheel?.Invoke(new UIEventData<MouseButton>()
            {
                Component = this,
                Position = _globalBounds.TopLeft,
                Delta = new Vector2(0, delta),
                InputValue = MouseButton.None,
                WasDragged = false,
            });
        }

        /// <summary>Updates the component and all its children.</summary>
        /// <param name="time"></param>
        public virtual void Update(Timing time)
        {
            if (_enabled)
            {
                OnUpdate(time);

                _children.ForInterlock(0, 1, (id, child) =>
                {
                    if (!child.IsVisible || !child.IsEnabled)
                        return false;

                    child.Update(time);

                    return false;
                });
                foreach (UIComponent child in _children)
                {
                    if (!child.IsVisible || !child.IsEnabled)
                        continue;

                    child.Update(time);
                }
            }
        }

        /// <summary>Draws the component and all its children.</summary>
        /// <param name="sb">The surface that the UI component must draw on to.</param>
        public virtual void Render(ISpriteBatch sb)
        {
            if (!_visible)
                return;

            OnRender(sb);

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

        protected virtual void OnUpdate(Timing time) { }

        /// <summary>Called right before the standard draw method draws the component's children. You can change this behaviour by overriding
        /// <see cref="UIComponent.Render"/>.</summary>
        /// <param name="surface"></param>
        protected virtual void OnRender(ISpriteBatch sb) { }

        /// <summary>Called after the component has rendered normally. If clipping is enabled, this will be called once the clip 
        /// zone is created and anything rendered will be clipped..</summary>
        /// <param name="sb"></param>
        protected virtual void OnRenderClipped(ISpriteBatch sb) { }

        /// <summary>Called when the component needs to draw its child components.</summary>
        /// <param name="sb">The spritebatch used for drawing UI components.</param>
        protected virtual void OnRenderChildren(ISpriteBatch sb)
        {
            _children.ForInterlock(0, 1, (id, child) =>
            {
                if (child.IsVisible == true)
                    child.Render(sb);

                return false;
            });
        }

        /// <summary>Gets or sets the local bounds of the UI component. This will cause all child components to update their bounds too.</summary>
        [DataMember]
        [Browsable(false)]
        public virtual Rectangle LocalBounds
        {
            get { return _localBounds; }
            set
            {
                _localBounds = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the X position of the UI component.</summary>
        public int X
        {
            get { return _localBounds.X; }
            set
            {
                _localBounds.X = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the Y position of the UI component.</summary>
        public int Y
        {
            get { return _localBounds.Y; }
            set
            {
                _localBounds.Y = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the width of the UI component.</summary>
        public int Width
        {
            get { return _localBounds.Width; }
            set
            {
                _localBounds.Width = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets or sets the height of the UI component.</summary>
        public int Height
        {
            get { return _localBounds.Height; }
            set
            {
                _localBounds.Height = value;
                UpdateBounds();
            }
        }

        /// <summary>Gets the parent component.</summary>
        [Browsable(false)]
        public UIComponent Parent
        {
            get { return _parent; }
            private set
            {
                _parent = value;
                _ui = _parent != null ? value.UI : null;
                UpdateBounds();
            }
        }

        /// <summary>Gets the global bounds.</summary>
        [Browsable(false)]
        public Rectangle GlobalBounds
        {
            get { return _globalBounds; }
        }

        [Browsable(false)]
        public Rectangle ClippingBounds
        {
            get { return _clippingBounds; }
        }

        [DataMember]
        [ExpandablePropertyAttribute]
        [DisplayName("Padding")]
        public UIPadding Padding
        {
            get { return _clipPadding; }
        }

        [DataMember]
        [Browsable(false)]
        public ThreadedList<UIComponent> Children
        {
            get { return _children; }
        }

        [DataMember]
        [DisplayName("Is Enabled")]
        public virtual bool IsEnabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [DataMember]
        [DisplayName("Is Visible")]
        public virtual bool IsVisible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        [DataMember]
        [DisplayName("Ignore child Input")]
        public bool IgnoreChildInput
        {
            get { return _ignoreChildInput; }
            set { _ignoreChildInput = value; }
        }

        [DataMember]
        [DisplayName("Ignore Child Dragging")]
        public bool IgnoreChildDrag
        {
            get { return _ignoreChildDrag; }
            set { _ignoreChildDrag = value; }
        }

        [ExpandablePropertyAttribute]
        [DisplayName("Margin")]
        [DataMember]
        public UIMargin Margin
        {
            get { return _margin; }
        }

        [DataMember]
        [DisplayName("Tooltip")]
        public string Tooltip
        {
            get { return _tooltip; }
            set { _tooltip = value; }
        }

        [DataMember]
        [DisplayName("Clipping Enabled")]
        /// <summary>Gets or sets whether or not clipping (scissor testing) is enabled for the component. 
        /// If enabled, all children will be clipped to the bounds of the component.</summary>
        public bool EnableClipping
        {
            get { return _enableClipping; }
            set { _enableClipping = value; }
        }

        /// <summary>Gets the UI system that the component is bound to.</summary>
        public UISystem UI
        {
            get { return _ui; }
            internal set
            {
                if (_ui != value)
                {
                    // Set the UI system of the current and child components
                    for (int i = _children.Count - 1; i >= 0; i--)
                        _children[i].UI = value;

                    OnUISystemChanged(_ui, value);
                    _ui = value;
                }
            }
        }

        [DataMember]
        /// <summary>Gets or sets the component's name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the tag object.</summary>
        public object Tag { get; set; }
    }
}
