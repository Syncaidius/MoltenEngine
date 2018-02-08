using SharpDX;
using Molten.Graphics;
using Molten.IO;
using Molten.Rendering;
using Molten.Serialization;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UICheckbox : UIComponent
    {
        const int SPACING = 5; //pixels between checkbox and label text.
        const int CHECK_PADDING = 3;

        UIRenderedText _text;
        bool _isChecked;
        int _boxSize = 20;

        Color _colorBox;
        Color _colorChecked;

        Rectangle _boxBounds;
        Rectangle _checkedBounds;

        public event ObjectHandler<UICheckbox> OnCheckChanged;

        public UICheckbox(UISystem ui) : base(ui)
        {
            _text = new UIRenderedText(ui)
            {
                VerticalAlignment = UIVerticalAlignment.Center,
            };
            _colorBox = new Color(90, 90, 90, 255);
            _colorChecked = new Color(180, 180, 255, 255);

            OnClickEnded += UICheckbox_OnPressCompleted;
        }

        void UICheckbox_OnPressCompleted(UIEventData<MouseButton> data)
        {
            _isChecked = !_isChecked;
            OnCheckChanged?.Invoke(this);
        }

        private void CalculateBox()
        {
            _boxBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Center.Y - (_boxSize / 2),
                Width = _boxSize,
                Height = _boxSize,
            };

            _checkedBounds = _boxBounds;
            _checkedBounds.Inflate(-CHECK_PADDING, -CHECK_PADDING);
        }

        protected override void OnUpdateBounds()
        {
            CalculateBox();

            _text.Bounds = new Rectangle()
            {
                X = _boxBounds.Right + SPACING,
                Y = _globalBounds.Y,
                Width = _globalBounds.Width - (_boxBounds.Width + SPACING),
                Height = _globalBounds.Height,
            };
        }

        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
        {
            base.OnRender(sb, proxy);

            sb.Draw(_boxBounds, _colorBox);

            if (_isChecked)
                sb.Draw(_checkedBounds, _colorChecked);

            _text.Draw(sb);
        }

        /// <summary>Gets or sets whether or not the checkbox is checked.</summary>
        [Category("Appearance")]
        [DisplayName("Is Checked")]
        [DataMember]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnCheckChanged?.Invoke(this);
                }
            }
        }

        /// <summary>Gets text object which controls how the caption text is displayed.</summary>
        [Category("Appearance")]
        [DisplayName("Text")]
        [ExpandablePropertyAttribute]
        [DataMember]
        public UIRenderedText Text
        {
            get { return _text; }
        }

        /// <summary>Gets the instance responsbile for managing the checkbox color.</summary>
        [Category("Appearance")]
        [DisplayName("Box Color")]
        [DataMember]
        public Color BoxColor
        {
            get { return _colorBox; }
            set { _colorBox = value; }
        }

        /// <summary>Gets the color instance repsonsible for the color of the box when it is checked.</summary>
        [Category("Appearance")]
        [DisplayName("Checked Color")]
        [DataMember]
        public Color CheckedColor
        {
            get { return _colorChecked; }
            set { _colorChecked = value; }
        }

        /// <summary>Gets or sets the size of the checkbox area.</summary>
        [Category("Appearance")]
        [DisplayName("Box Size")]
        [DataMember]
        public int BoxSize
        {
            get { return _boxSize; }
            set
            {
                _boxSize = value;
                CalculateBox();
            }
        }
    }
}
