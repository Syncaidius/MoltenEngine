using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// The base class for a UI component.
    /// </summary>
    public abstract class UIComponent
    {
        internal UIComponentRenderer Data;
        UIComponent _parent;
        List<UIComponent> _children;

        internal UIComponent()
        {
            _children = new List<UIComponent>();
            Data.Margin.OnChanged += MarginPadding_OnChanged;
            Data.Padding.OnChanged += MarginPadding_OnChanged;
        }

        private void MarginPadding_OnChanged()
        {
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (_parent != null)
            {
                Data.GlobalBounds = new Rectangle()
                {
                    X = _parent.Data.GlobalBounds.X + Data.LocalBounds.X,
                    Y = _parent.Data.GlobalBounds.Y + Data.LocalBounds.Y,
                    Width = Data.LocalBounds.Width,
                    Height = Data.LocalBounds.Height,
                };

                Data.ParentData = _parent.Data;
            }
            else
            {
                Data.GlobalBounds = Data.LocalBounds;

                Data.ParentData = null;
            }

            UISpacing pad = Data.Padding;
            UISpacing mrg = Data.Padding;
            Data.BorderBounds = Data.GlobalBounds;
            Data.BorderBounds.Inflate(-mrg.Left, -mrg.Top, -mrg.Right, -mrg.Bottom);

            Data.RenderBounds = Data.BorderBounds;
            Data.RenderBounds.Inflate(-pad.Left, -pad.Top, -pad.Right, -pad.Bottom);
        }

        internal void Update(Timing time)
        {
            OnUpdate(time);       
        }

        protected abstract void OnUpdate(Timing time);

        public Rectangle LocalBounds
        {
            get => Data.LocalBounds;
            set
            {
                Data.LocalBounds = value;
                UpdateBounds();
            }
        }

        public Rectangle GlobalBounds => Data.GlobalBounds;

        public Rectangle RenderBounds => Data.RenderBounds;

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
    }

    public abstract class UIComponent<D> : UIComponent
        where D : UIComponentRenderer, new()
    {
        public UIComponent() : base()
        {
            Data = new D();
        }
    }
}
