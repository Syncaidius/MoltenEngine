using Molten.Graphics;
using System;

namespace Molten.UI
{
    public class UIMenuItem : UICompoundComponent
    {
        UIPanel _bgPanel;
        UILabel _label;
        Sprite _icon;
        int _iconSpacing = 16;
        int _iconSize;
        int _iconMargin;
        int _padSpacing;

        public UIMenuItem()
        {
            _bgPanel = new UIPanel();
            _bgPanel.BackgroundColor = Color.Transparent;
            _bgPanel.BorderColor = Color.Transparent;
            _bgPanel.Margin.SetDock(true, true, true, true);

            _label = new UILabel(Engine.Current.DefaultFont, this.Name);
            _label.VerticalAlignment = UIVerticalAlignment.Center;
            _label_OnTextChanged(_label);
            _label.OnTextChanged += _label_OnTextChanged;

            AddPart(_bgPanel);
            AddPart(_label);
        }

        private void _label_OnTextChanged(UILabel obj)
        {
            UpdateBounds();
        }

        protected override void OnPreUpdateBounds()
        {
            base.OnPreUpdateBounds();
            _iconSize = _label.Size.Y;
            _iconMargin = (_icon != null || (Parent is UIMenu pMenu && pMenu.FlowDirection == UIMenu.ItemFlowDirection.Vertical)) ? _iconSize + _iconSpacing : 0;
            _padSpacing = _iconSize / 2;

            _localBounds.Width = ClipPadding.Left + _padSpacing + _iconMargin + _label.Size.X + _padSpacing + ClipPadding.Right;
            _localBounds.Height = Math.Max(_label.Size.Y + ClipPadding.Top + ClipPadding.Bottom, _localBounds.Height);
        }

        protected override void OnPostUpdateBounds()
        {
            base.OnPostUpdateBounds();
            _label.LocalBounds = new Rectangle()
            {
                X = _padSpacing + _iconMargin,
                Y = 0,
                Width = ClippingBounds.Width,
                Height = ClippingBounds.Height,
            };
        }

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public SpriteFont Font
        {
            get => _label.Font;
            set => _label.Font = value;
        }

        public UIVerticalAlignment VerticalAlignment
        {
            get => _label.VerticalAlignment;
            set => _label.VerticalAlignment = value;
        }

        public UIHorizontalAlignment HorizontalAlignment
        {
            get => _label.HorizontalAlignment;
            set => _label.HorizontalAlignment = value;
        }
    }
}
