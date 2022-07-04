using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIWindow : UIElement
    {
        UIPanel _titleBar;
        UIPanel _panel;
        UIButton _btnClose;
        UIButton _btnMinimize;
        UIButton _btnMaximize;
        UIText _title;
        int _titleBarSize = 25;
        int _iconSpacing = 5;
        List<UIButton> _titleBarButtons;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            _titleBarButtons = new List<UIButton>();

            // Change _panel corners to only round bottom left/right.
            _titleBar = CompoundElements.Add<UIPanel>();
            _panel = CompoundElements.Add<UIPanel>();
            _title = CompoundElements.Add<UIText>();
            _title.VerticalAlign = UIVerticalAlignment.Center;

            for(int i = 0; i < 10; i++)
            {
                UIButton b = CompoundElements.Add<UIButton>();
                b.Text = $"{i}";
            }

            _btnClose = AddTitleButton("X");
            _btnMaximize = AddTitleButton("^");
            _btnMinimize = AddTitleButton("_");

            Title = Name;
        }

        protected override void OnApplyTheme(UITheme theme, UIElementTheme elementTheme, UIStateTheme stateTheme)
        {
            base.OnApplyTheme(theme, elementTheme, stateTheme);

            for (int i = 0; i < _titleBarButtons.Count; i++)
            {
                if (i == 0)
                    _titleBarButtons[i].CornerRadius.Set(0, CornerRadius.TopRight, 0, 0);
                else
                    _titleBarButtons[i].CornerRadius.Set(0);
            }

            _titleBar.CornerRadius.Set(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0);
            _panel.CornerRadius.Set(0, 0, CornerRadius.BottomRight, CornerRadius.BottomLeft);
        }

        private UIButton AddTitleButton(string text)
        {
            UIButton btn = CompoundElements.Add<UIButton>();
            _titleBarButtons.Add(btn);
            btn.Text = text;
            return btn;
        }

        protected override void OnUpdateCompoundBounds()
        {
            base.OnUpdateCompoundBounds();

            ref Rectangle gb = ref BaseData.GlobalBounds;

            _titleBar.LocalBounds = new Rectangle(0, 0, gb.Width, _titleBarSize);
            _panel.LocalBounds = new Rectangle(0, _titleBarSize, gb.Width, gb.Height - _titleBarSize);
            _title.LocalBounds = new Rectangle(_titleBarSize + (_iconSpacing * 2), 0, gb.Width, _titleBarSize);
            for(int i = 0; i < _titleBarButtons.Count; i++)
                _titleBarButtons[i].LocalBounds = new Rectangle(gb.Width - (_titleBarSize * (i+1)), 0, _titleBarSize, _titleBarSize);
        }

        public string Title
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        public int TitleBarHeight
        {
            get => _titleBarSize;
            set
            {
                _titleBarSize = value;
                OnUpdateBounds();
            }
        }

        public ref CornerInfo CornerRadius => ref _panel.CornerRadius;
    }
}
