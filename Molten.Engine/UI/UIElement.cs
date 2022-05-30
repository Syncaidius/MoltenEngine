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
        internal UIRenderData BaseData;

        UIManagerComponent _owner;

        public UIElement()
        {
            Children = new UIChildCollection(this);
            CompoundElements = new UIChildCollection(this);
            Engine = Engine.Current;
            BaseData = new UIRenderData();
            OnInitialize(Engine, Engine.Settings.UI, Engine.Settings.UI.Theme.Value);
        }

        protected virtual void OnInitialize(Engine engine, UISettings settings, UITheme theme)
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
                    X = Parent.BaseData.GlobalBounds.X + BaseData.LocalBounds.X,
                    Y = Parent.BaseData.GlobalBounds.Y + BaseData.LocalBounds.Y,
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

            foreach (UIElement e in Children)
                e.UpdateBounds();

            foreach (UIElement e in CompoundElements)
                e.UpdateBounds();

            OnUpdateBounds();
        }

        protected virtual void ApplyOwner(UIManagerComponent owner)
        {
            foreach (UIElement child in Children)
                child.Owner = owner;
        }

        internal void HandleInput(Timing time, SceneClickTracker tracker)
        {
            
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
        public virtual bool Contains(Vector2F point)
        {
            return BaseData.GlobalBounds.Contains(point);
        }

        protected override void OnDispose() { }
        protected virtual void OnUpdateBounds() { }

        protected virtual void OnUpdate(Timing time) { }

        internal virtual void Render(SpriteBatcher sb)
        {
            // Render compound components, inside global bounds rather than render bounds.
            // Note - RenderBounds is intended for rendering child elements, not compound component elements.
            if (CompoundElements.Count > 0)
            {
                if (BaseData.IsClipEnabled)
                {
                    sb.PushClip(BaseData.GlobalBounds);
                    foreach (UIElement e in CompoundElements)
                        e.Render(sb);
                    sb.PopClip();
                }
                else
                {
                    foreach (UIElement e in CompoundElements)
                        e.Render(sb);
                }
            }

            // Render children.
            if (Children.Count > 0)
            {
                if (BaseData.IsClipEnabled)
                {
                    sb.PushClip(BaseData.RenderBounds);
                    foreach (UIElement child in Children)
                        child.Render(sb);
                    sb.PopClip();
                }
                else
                {
                    foreach (UIElement child in Children)
                        child.Render(sb);
                }
            }
        }

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
        /// Gets the global bounds, relative to the <see cref="UIManagerComponent"/> that is drawing the current <see cref="UIElement"/>.s
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
        public UIElement Parent { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Engine"/> instance that the current <see cref="UIElement"/> is bound to.
        /// </summary>
        public Engine Engine { get; private set; }

        /// <summary>
        /// Gets the internal <see cref="UIManagerComponent"/> that will draw the current <see cref="UIElement"/>.
        /// </summary>
        internal UIManagerComponent Owner
        {
            get => _owner;
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    ApplyOwner(_owner);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="UITheme"/> that should be applied to the current <see cref="UIElement"/>. Themes provide a set of default appearance values and configuration.
        /// </summary>
        public UITheme Theme { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="EP">Extended property structure.</typeparam>
    public abstract class UIElement<EP> : UIElement
        where EP : struct, IUIRenderData
    {
        EP _data = new EP();

        internal override void Render(SpriteBatcher sb)
        {
            _data.Render(sb, BaseData);
            base.Render(sb);
        }

        protected ref EP Properties => ref _data;
    }
}
