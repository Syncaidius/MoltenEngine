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
        List<UIButton> _buttons;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            _buttons = new List<UIButton>();

            // TODO add title bar with top-left/right round corners

            // Change _panel corners to only round bottom left/right.
            _panel = CompoundElements.Add<UIPanel>();
            _title = CompoundElements.Add<UIText>();

            for(int i = 0; i < 10; i++)
            {
                UIButton b = CompoundElements.Add<UIButton>();
                b.Text = $"{i}";
            }

            /*_btnClose = CompoundElements.Add<UIButton>();
            _btnClose.Text = "X";

            _btnMinimize = CompoundElements.Add<UIButton>();
            _btnMinimize.Text = "_";

            _btnMaximize = CompoundElements.Add<UIButton>();
            _btnMaximize.Text = "^";*/

            Title = Name;
        }

        protected override void OnUpdateCompoundBounds()
        {
            base.OnUpdateCompoundBounds();

            ref Rectangle gb = ref BaseData.GlobalBounds;

            _panel.LocalBounds = new Rectangle(0, 0, gb.Width, gb.Height);
            _title.LocalBounds = new Rectangle(0, 0, _panel.LocalBounds.Width, _titleBarHeight);
            /*_btnClose.LocalBounds = new Rectangle(gb.Width - _titleBarHeight, 0, _titleBarHeight, _titleBarHeight);
            _btnMinimize.LocalBounds = new Rectangle(gb.Width - (_titleBarHeight * 2), 0, _titleBarHeight, _titleBarHeight);
            _btnMaximize.LocalBounds = new Rectangle(gb.Width - (_titleBarHeight * 3), 0, _titleBarHeight, _titleBarHeight);*/
            for(int i = 0; i < _buttons.Count; i++)
                _buttons[i].LocalBounds = new Rectangle(gb.Width - (_titleBarHeight * (i + 1)), 0, _titleBarHeight, _titleBarHeight);

            // TODO Implement button bar manager and use this to manage top-right window buttons (user can add more next to the 3 standard buttons)
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
    }
}
