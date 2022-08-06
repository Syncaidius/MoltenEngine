using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    public class UIWindow : UIElement
    {
        public event UIElementHandler<UIWindow> Closing;

        UIPanel _titleBar;
        UIPanel _panel;
        UIButton _btnClose;
        UIButton _btnMinimize;
        UIButton _btnMaximize;
        UILabel _title;
        int _iconSpacing = 5;
        List<UIButton> _titleBarButtons;

        Color _borderColor= new Color(52, 189, 235, 255);
        Color _fillColor = new Color(0, 109, 155, 200);
        CornerInfo _cornerRadius = new CornerInfo(8f);

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _titleBarButtons = new List<UIButton>();

            // Change _panel corners to only round bottom left/right.
            _titleBar = CompoundElements.Add<UIPanel>();
            _panel = CompoundElements.Add<UIPanel>();
            _title = CompoundElements.Add<UILabel>();
            _title.VerticalAlign = UIVerticalAlignment.Center;

            _btnClose = AddTitleButton("X");
            _btnMaximize = AddTitleButton("^");
            _btnMinimize = AddTitleButton("_");

            Title = Name;
            TitleBarHeight = 25;
        }

        protected override void ApplyTheme()
        {
            base.ApplyTheme();

            _titleBar.CornerRadius.Set(_cornerRadius.TopLeft, _cornerRadius.TopRight, 0, 0);
            _titleBar.FillColor = _borderColor;
            _panel.CornerRadius.Set(0, 0, _cornerRadius.BottomRight, _cornerRadius.BottomLeft);
        }

        private UIButton AddTitleButton(string text)
        {
            UIButton btn = CompoundElements.Add<UIButton>();
            _titleBarButtons.Add(btn);
            btn.Text = text;
            return btn;
        }

        protected override bool OnPicked(Vector2F globalPos)
        {
            return !RenderBounds.Contains(globalPos);
        }

        protected override void OnUpdateCompoundBounds()
        {
            base.OnUpdateCompoundBounds();

            Rectangle gb = GlobalBounds;

            _titleBar.LocalBounds = new Rectangle(0, 0, gb.Width, TitleBarHeight);
            _panel.LocalBounds = new Rectangle(0, TitleBarHeight, gb.Width, gb.Height - TitleBarHeight);
            _title.LocalBounds = new Rectangle(TitleBarHeight + (_iconSpacing * 2), 0, gb.Width, TitleBarHeight);

            for(int i = 0; i < _titleBarButtons.Count; i++)
                _titleBarButtons[i].LocalBounds = new Rectangle(gb.Width - (TitleBarHeight * (i+1)), 0, TitleBarHeight, TitleBarHeight);
        }

        /// <summary>
        /// The title of the current <see cref="UIWindow"/>.
        /// </summary>
        [JsonProperty]
        public string Title
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        /// <summary>
        /// The title bar height of the current <see cref="UIWindow"/>, in pixels. This value will also be the size of any buttons on the title bar.
        /// </summary>
        [JsonProperty]
        public int TitleBarHeight
        {
            get => InternalPadding.Top;
            set => InternalPadding.Top = value; // Triggers a bounds update, so we don't need to do one ourselves here.
        }

        [UIThemeMember]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                _titleBar.FillColor = _borderColor;
                _titleBar.BorderColor = _borderColor;
                _panel.BorderColor = _borderColor;
            }
        }

        [UIThemeMember]
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                _panel.FillColor = _fillColor;
            }
        }

        [UIThemeMember]
        public CornerInfo CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;

                _titleBar.CornerRadius.Set(_cornerRadius.TopLeft, _cornerRadius.TopRight, 0, 0);
                _panel.CornerRadius.Set(0, 0, _cornerRadius.BottomRight, _cornerRadius.BottomLeft);

                if (_titleBarButtons.Count > 0)
                {
                    CornerInfo btnRadius = _titleBarButtons[0].CornerRadius;
                    btnRadius.TopRight = _cornerRadius.TopRight;
                    _titleBarButtons[0].CornerRadius = btnRadius;
                }
            }
        }
    }
}
