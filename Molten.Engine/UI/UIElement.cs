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

        UIElement _parent;
        List<UIElement> _children;
        UIElement _root;

        public UIElement()
        {
            _children = new List<UIElement>();
            Children = _children.AsReadOnly();
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
            if (_parent != null)
            {
                BaseData.GlobalBounds = new Rectangle()
                {
                    X = _parent.BaseData.GlobalBounds.X + BaseData.LocalBounds.X,
                    Y = _parent.BaseData.GlobalBounds.Y + BaseData.LocalBounds.Y,
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

            foreach (UIElement e in _children)
                e.UpdateBounds();

            OnUpdateBounds();
        }

        internal void HandleInput(Timing time, SceneClickTracker tracker)
        {
            
        }

        internal void Update(Timing time)
        {
            OnUpdate(time);

            for (int i = _children.Count - 1; i >= 0; i--)
                _children[i].Update(time);
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

        internal abstract void Render(SpriteBatcher sb);

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
        public IReadOnlyList<UIElement> Children { get; }

        public UIElement Parent
        {
            get => _parent;
            set
            {
                if(_parent != value)
                {
                    if (_parent != null)
                    {
                        _parent._children.Remove(this);
                        _root.RenderComponent.QueueChange(new UIRemoveChildChange()
                        {
                            Child = BaseData,
                            Parent = _parent.BaseData
                        });
                    }

                    _parent = value;

                    if (_parent != null)
                    {
                        Root = _parent.Root;
                        _parent._children.Add(this);
                        _root.RenderComponent.QueueChange(new UIAddChildChange()
                        {
                            Child = BaseData,
                            Parent = _parent.BaseData
                        });
                    }
                }
            }
        }

        public Engine Engine { get; private set; }

        /// <summary>
        /// Gets the root <see cref="UIElement"/>.
        /// </summary>
        public UIElement Root
        {
            get => _root;
            internal set
            {
                if(_root != value)
                {
                    _root = value;
                    RenderComponent = _root.RenderComponent;
                    foreach (UIElement child in Children)
                        child.Root = _root;
                }
            }
        }

        /// <summary>
        /// Gets the internal <see cref="UIManagerComponent"/> that will draw the current <see cref="UIElement"/>.
        /// </summary>
        internal UIManagerComponent RenderComponent { get; set; }

        public UITheme Theme { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="EP">Extended property structure.</typeparam>
    public abstract class UIElement<EP> : UIElement
        where EP : struct, IUIRenderData
    {
        EP _data = new EP();

        internal override void Render(SpriteBatcher sb)
        {
            _data.Render(sb, BaseData);

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

        public ref EP Properties => ref _data;
    }
}
