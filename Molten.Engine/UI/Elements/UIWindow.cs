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
        UIPanel _panel;
        UIButton _btnClose;
        UIButton _btnMinimize;
        UIButton _btnMaximize;
        UIText _title;
        int _titleBarHeight = 25;
        List<UIButton> _titleBarButtons;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            _titleBarButtons = new List<UIButton>();

            // Change _panel corners to only round bottom left/right.
            _panel = CompoundElements.Add<UIPanel>();
            _title = CompoundElements.Add<UIText>();

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

            _panel.LocalBounds = new Rectangle(0, 0, gb.Width, gb.Height);
            _title.LocalBounds = new Rectangle(0, 0, _panel.LocalBounds.Width, _titleBarHeight);
            for(int i = 0; i < _titleBarButtons.Count; i++)
                _titleBarButtons[i].LocalBounds = new Rectangle(gb.Width - (_titleBarHeight * (i+1)), 0, _titleBarHeight, _titleBarHeight);
        }

        public string Title
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        public int TitleBarHeight
        {
            get => _titleBarHeight;
            set
            {
                _titleBarHeight = value;
                OnUpdateBounds();
            }
        }

        public ref CornerInfo CornerRadius => ref _panel.CornerRadius;
    }
}
