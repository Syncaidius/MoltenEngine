using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// The base class for a UI component.
    /// </summary>
    public abstract class UIComponent
    {
        [DataMember]
        internal UIBaseData BaseData;

        UIComponent _parent;
        List<UIComponent> _children;

        public UIComponent()
        {
            _children = new List<UIComponent>();
            Engine = Engine.Current;
            BaseData = new UIBaseData();
            OnInitialize(Engine, Engine.Settings.UI);
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
            if (_parent != null)
            {
                BaseData.GlobalBounds = new Rectangle()
                {
                    X = _parent.BaseData.GlobalBounds.X + BaseData.LocalBounds.X,
                    Y = _parent.BaseData.GlobalBounds.Y + BaseData.LocalBounds.Y,
                    Width = BaseData.LocalBounds.Width,
                    Height = BaseData.LocalBounds.Height,
                };

                BaseData.Parent = _parent.BaseData;
            }
            else
            {
                BaseData.GlobalBounds = BaseData.LocalBounds;

                BaseData.Parent = null;
            }

            UISpacing pad = BaseData.Padding;
            UISpacing mrg = BaseData.Margin;
            BaseData.BorderBounds = BaseData.GlobalBounds;
            BaseData.BorderBounds.Inflate(-mrg.Left, -mrg.Top, -mrg.Right, -mrg.Bottom);

            BaseData.RenderBounds = BaseData.BorderBounds;
            BaseData.RenderBounds.Inflate(-pad.Left, -pad.Top, -pad.Right, -pad.Bottom);
        }

        internal void Update(Timing time)
        {
            OnUpdate(time);       
        }

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

        public Rectangle GlobalBounds => BaseData.GlobalBounds;

        public Rectangle RenderBounds => BaseData.RenderBounds;

        public List<UIComponent> Children => _children;

        public UIComponent Parent
        {
            get => _parent;
            set
            {
                if(_parent != value)
                {
                    _parent.Children.Remove(this);
                    _parent = value;
                    _parent.Children.Add(this);
                }
            }
        }

        public Engine Engine { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <typeparam name="EP">Extended property structure.</typeparam>
    public abstract class UIComponent<EP> : UIComponent
        where EP : struct, IUIRenderData
    {
        EP _data = new EP();

        internal override void Render(SpriteBatcher sb)
        {
            _data.Render(sb, BaseData);
        }

        public ref EP Properties => ref _data;
    }
}
