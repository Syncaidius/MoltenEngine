using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using System.ComponentModel;
using Molten.IO;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIButton : UIComponent
    {
        UIButtonState _state;
        bool _hovered;
        UIRenderedText _text;
        Color[] _color;

        /// <summary>Creates a new instances of <see cref="UIButton"/>.</summary>
        /// <param name="ui">The UI system to bind this control to.</param>
        public UIButton(Engine engine) : base(engine)
        {
            _state = UIButtonState.Default;
            _text = new UIRenderedText(engine);
            _text.HorizontalAlignment = UIHorizontalAlignment.Center;
            _text.VerticalAlignment = UIVerticalAlignment.Center;

            OnClickStarted += UIButton_OnPressStarted;
            OnClickEnded += UIButton_OnPressCompleted;
            OnClickEndedOutside += UIButton_OnPressCompletedOutside;
            OnEnter += UIButton_OnEnter;
            OnLeave += UIButton_OnLeave;

            _color = new Color[4];
            _color[(int)UIButtonState.Default] = new Color(80, 80, 180, 200);
            _color[(int)UIButtonState.Hover] = new Color(100, 100, 230, 255);
            _color[(int)UIButtonState.Clicked] = new Color(140, 140, 255, 255);
            _color[(int)UIButtonState.Disabled] = new Color(70, 70, 70, 200);
        }

        void UIButton_OnPressCompletedOutside(UIEventData<MouseButton> data)
        {
            _state = UIButtonState.Default;
        }

        void UIButton_OnLeave(UIEventData<MouseButton> data)
        {
            _hovered = false;
            _state = UIButtonState.Default;
        }

        void UIButton_OnEnter(UIEventData<MouseButton> data)
        {
            _hovered = true;
            _state = UIButtonState.Hover;
        }

        void UIButton_OnPressCompleted(UIEventData<MouseButton> data)
        {
            if (_hovered)
                _state = UIButtonState.Hover;
            else
                _state = UIButtonState.Default;
        }

        void UIButton_OnPressStarted(UIEventData<MouseButton> data)
        {
            _state = UIButtonState.Clicked;
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();
            _text.Bounds = _globalBounds;
        }

        protected override void OnRender(ISpriteBatch sb)
        {

            sb.DrawRect(_globalBounds, _color[(int)_state]);
            _text.Draw(sb);
        }

        [DataMember]
        public Color DefaultColor
        {
            get { return _color[(int)UIButtonState.Default]; }
            set { _color[(int)UIButtonState.Default] = value; }
        }

        [DataMember]
        public Color HoverColor
        {
            get { return _color[(int)UIButtonState.Hover]; }
            set { _color[(int)UIButtonState.Hover] = value; }
        }

        [DataMember]
        public Color DisabledColor
        {
            get { return _color[(int)UIButtonState.Disabled]; }
            set { _color[(int)UIButtonState.Disabled] = value; }
        }

        [DataMember]
        public Color ClickColor
        {
            get { return _color[(int)UIButtonState.Clicked]; }
            set { _color[(int)UIButtonState.Clicked] = value; }
        }

        [ExpandablePropertyAttribute]
        [DataMember]
        public UIRenderedText Text
        {
            get { return _text; }
        }

        [Browsable(false)]
        [DataMember]

        public UIButtonState State
        {
            get { return _state; }
        }
    }
}
