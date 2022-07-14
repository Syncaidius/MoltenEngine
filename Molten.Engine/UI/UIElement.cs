using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// The base class for a UI component.
    /// </summary>
    [Serializable]
    public abstract class UIElement : EngineObject
    {
        [DataMember]
        protected internal UIRenderData BaseData { get; }

        UIManagerComponent _manager;
        UIElement _parent;
        UITheme _theme;
        UIElementState _state;

        public UIElement()
        {
            Children = new UIChildCollection(this);
            CompoundElements = new UIChildCollection(this);
            Engine = Engine.Current;
     
            BaseData = new UIRenderData();
            State = UIElementState.Default;
            OnInitialize(Engine, Engine.Settings.UI);
        }

        /// <summary>
        /// Loads a content file and invokes a callback with the loaded content object and it's originating <see cref="ContentContext"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">The path or name of the content object to be loaded.</param>
        /// <param name="callback">The callback to invoke once the content object has loaded.</param>
        /// <param name="p">Any content parameters to add to the request, if any. Default value is null.</param>
        protected void LoadContent<T>(string path, Action<ContentRequest, T> callback, IContentParameters p = null)
        {
            ContentRequest cr = Engine.Content.BeginRequest();
            cr.Load<T>(path, p);
            cr.OnCompleted += (request) =>
            {
                T content = request.Get<T>(0);
                callback(request, content);
            };
            cr.Commit();
        }

        protected virtual void OnInitialize(Engine engine, UISettings settings)
        {
            BaseData.Margin.OnChanged += MarginPadding_OnChanged;
            BaseData.Padding.OnChanged += MarginPadding_OnChanged;
        }

        private void MarginPadding_OnChanged()
        {
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (Parent != null)
            {
                BaseData.GlobalBounds = new Rectangle()
                {
                    X = Parent.BaseData.RenderBounds.X + BaseData.LocalBounds.X,
                    Y = Parent.BaseData.RenderBounds.Y + BaseData.LocalBounds.Y,
                    Width = BaseData.LocalBounds.Width,
                    Height = BaseData.LocalBounds.Height,
                };
            }
            else
            {
                BaseData.GlobalBounds = BaseData.LocalBounds;
            }

            UISpacing pad = BaseData.Padding;
            UISpacing mrg = BaseData.Margin;
            BaseData.BorderBounds = BaseData.GlobalBounds;
            BaseData.BorderBounds.Inflate(-mrg.Left, -mrg.Top, -mrg.Right, -mrg.Bottom);

            BaseData.RenderBounds = BaseData.BorderBounds;
            BaseData.RenderBounds.Inflate(-pad.Left, -pad.Top, -pad.Right, -pad.Bottom);

            OnUpdateCompoundBounds();
            foreach (UIElement e in Children)
                e.UpdateBounds();

            OnUpdateChildBounds();
            foreach (UIElement e in CompoundElements)
                e.UpdateBounds();

            OnUpdateBounds();
        }

        protected virtual void ApplyTheme()
        {
            if (_theme == null)
                return;

            foreach (UIElement e in CompoundElements)
                e.Theme = _theme;

            foreach (UIElement e in Children)
                e.Theme = _theme;

            _theme?.ApplyStyle(this);
            UpdateBounds();
        }

        internal void Update(Timing time)
        {
            OnUpdate(time);

            for (int i = CompoundElements.Count - 1; i >= 0; i--)
                CompoundElements[i].Update(time);

            for (int i = Children.Count - 1; i >= 0; i--)
                Children[i].Update(time);
        }

        /// <summary>
        /// Checks if the current <see cref="UIElement"/> contains the given <see cref="Vector2F"/>. This does not test any child <see cref="UIElement"/> objects.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Vector2F point)
        {
            return BaseData.GlobalBounds.Contains(point);
        }

        /// <summary>
        /// Returns the current <see cref="UIElement"/> or one of it's children (recursive), depending on which contains <paramref name="point"/>.
        /// </summary>
        /// <param name="point">The point to use for picking a <see cref="UIElement"/>.</param>
        /// <param name="ignoreRules">If true, <see cref="InputRules"/> is ignored when picking.</param>
        /// <returns></returns>
        public UIElement Pick(Vector2F point, bool ignoreRules = false)
        {
            UIElement result = null;

            if (Contains(point))
            {
                if ((!ignoreRules && HasInputRules(UIInputRuleFlags.Compound)))
                {
                    for (int i = CompoundElements.Count - 1; i >= 0; i--)
                    {
                        result = CompoundElements[i].Pick(point);
                        if (result != null)
                            return result;
                    }
                }

                if ((!ignoreRules && HasInputRules(UIInputRuleFlags.Children)))
                {
                    for (int i = Children.Count - 1; i >= 0; i--)
                    {
                        result = Children[i].Pick(point);
                        if (result != null)
                            return result;
                    }
                }

                if(!ignoreRules && HasInputRules(UIInputRuleFlags.Self))
                    return this;
            }

            return result;
        }

        /// <summary>
        /// Returns true if the given <see cref="UIInputRuleFlags"/> are set on the current <see cref="UIElement"/>.
        /// </summary>
        /// <param name="rules"></param>
        /// <returns></returns>
        public bool HasInputRules(UIInputRuleFlags rules)
        {
            return (InputRules & rules) == rules;
        }

        public virtual void OnPressed(ScenePointerTracker tracker)
        {
            State = UIElementState.Pressed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracker"></param>
        /// <param name="releasedOutside">The current <see cref="UIElement"/> was released outside of it's bounds.</param>
        public virtual void OnReleased(ScenePointerTracker tracker, bool releasedOutside)
        {
            State = UIElementState.Default;
        }

        protected override void OnDispose() { }

        protected virtual void OnUpdateBounds() { }

        /// <summary>
        /// Invoked right before updating the bounds of compound elements.
        /// </summary>
        protected virtual void OnUpdateCompoundBounds() { }

        /// <summary>
        /// Invoked right before updating the bounds of child elements.
        /// </summary>
        protected virtual void OnUpdateChildBounds() { }

        protected virtual void OnUpdate(Timing time) { }

        internal void Render(SpriteBatcher sb)
        {
            if (IsClipEnabled && sb.PushClip(GlobalBounds))
            {
                OnRenderSelf(sb);
                sb.PopClip();
            }
            else
            {
                OnRenderSelf(sb);
            }

            CompoundElements.Render(sb, ref BaseData.GlobalBounds);
            Children.Render(sb, ref BaseData.RenderBounds);
        }

        protected virtual void OnRenderSelf(SpriteBatcher sb) { }

        /// <summary>
        /// Gets or sets the local bounds of the current <see cref="UIElement"/>.
        /// </summary>
        [DataMember]
        public Rectangle LocalBounds
        {
            get => BaseData.LocalBounds;
            set
            {
                BaseData.LocalBounds = value;
                UpdateBounds();
            }
        }

        /// <summary>
        /// Gets the global bounds, relative to the <see cref="UIManagerComponent"/> that is drawing the current <see cref="UIElement"/>.
        /// <para>Global bounds are the area in which input is accepted and from which <see cref="RenderBounds"/> is calculated, based on padding, borders and other properties.</para>
        /// </summary>
        public Rectangle GlobalBounds => BaseData.GlobalBounds;

        /// <summary>
        /// Gets the bounds in which child components should be drawn.
        /// </summary>
        public Rectangle RenderBounds => BaseData.RenderBounds;

        /// <summary>
        /// Gets or sets whether clipping is enabled.
        /// </summary>
        public ref bool IsClipEnabled => ref BaseData.IsClipEnabled;

        /// <summary>
        /// Gets a read-only list of child components attached to the current <see cref="UIElement"/>.
        /// </summary>
        public UIChildCollection Children { get; }

        /// <summary>
        /// Gets a list of compound child <see cref="UIElement"/>. These cannot be externally-modified outside of the current <see cref="UIElement"/>.
        /// </summary>
        protected UIChildCollection CompoundElements { get; }

        /// <summary>
        /// Gets the parent of the current <see cref="UIElement"/>.
        /// </summary>
        public UIElement Parent
        {
            get => _parent;
            internal set
            {
                if (_parent != value)
                {
                    _parent = value;
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Engine"/> instance that the current <see cref="UIElement"/> is bound to.
        /// </summary>
        public Engine Engine { get; private set; }

        /// <summary>
        /// Gets the internal <see cref="UIManagerComponent"/> that will draw the current <see cref="UIElement"/>.
        /// </summary>
        internal UIManagerComponent Manager
        {
            get => _manager;
            set
            {
                if (_manager != value)
                {
                    _manager = value;
                    CompoundElements.SetManager(_manager);
                    Children.SetManager(_manager);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="UITheme"/> that should be applied to the current <see cref="UIElement"/>. Themes provide a set of default appearance values and configuration.
        /// </summary>
        public UITheme Theme
        {
            get => _theme;
            internal set
            {
                if (_theme != value)
                {
                    _theme = value;

                    if (_theme != null)
                        ApplyTheme();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="UIElementState"/> of the current <see cref="UIElement"/>.
        /// </summary>
        public UIElementState State
        {
            get => _state;
            set
            {
                if(_state != value)
                {
                    _state = value;
                    _theme?.ApplyStyle(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the input rules for the current <see cref="UIElement"/>.
        /// </summary>
        public UIInputRuleFlags InputRules { get; set; } = UIInputRuleFlags.All;

        /// <summary>
        /// Gets whether or not the current <see cref="UIElement"/> is a compound component of another element.
        /// </summary>
        public bool IsCompoundChild { get; internal set; }
    }
}
