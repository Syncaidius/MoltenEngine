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
        internal UIComponentRenderer BaseData;

        UIComponent _parent;
        List<UIComponent> _children;

        public UIComponent()
        {
            _children = new List<UIComponent>();
            Engine = Engine.Current;
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
            UISpacing mrg = BaseData.Padding;
            BaseData.BorderBounds = BaseData.GlobalBounds;
            BaseData.BorderBounds.Inflate(-mrg.Left, -mrg.Top, -mrg.Right, -mrg.Bottom);

            BaseData.RenderBounds = BaseData.BorderBounds;
            BaseData.RenderBounds.Inflate(-pad.Left, -pad.Top, -pad.Right, -pad.Bottom);
        }

        internal void Update(Timing time)
        {
            OnUpdate(time);       
        }

        protected abstract void OnUpdate(Timing time);

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

    public abstract class UIComponent<R> : UIComponent
        where R : UIComponentRenderer, new()
    {
        protected R Data;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            Data = new R();
            BaseData = Data;

            base.OnInitialize(engine, settings);
        }
    }
}
