using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace Molten.UI
{
    public class UIComboBox : UICompoundComponent
    {
        const int BUTTON_SIZE = 25;
        const int ITEM_HEIGHT = 25;
        const int LIST_BORDER_THICKNESS = 2;

        UIButton _toggle;
        UIListBox _list;
        UIRenderedText _text;

        bool _listVisible;
        Color _bgColor;
        Color _borderColor;
        int _maxItems = 10;

        Rectangle _boxBounds;
        Rectangle _innerBounds;


        public event ObjectHandler<UIComboBox> OnSelectionChanged;

        public UIComboBox(Engine engine) : base(engine)
        {
            _bgColor = new Color(90, 90, 90, 255);
            _borderColor = new Color(120, 120, 120, 255);

            _toggle = new UIButton(engine);
            _toggle.Text.Text = "v";

            _text = new UIRenderedText(engine);
            _text.Text = "";
            _list = new UIListBox(engine);
            _list.OnSelectionChanged += _list_OnSelectionChanged;

            Padding.Left = 2;
            Padding.Right = 2;
            Padding.Top = 2;
            Padding.Bottom = 2;

            AddPart(_toggle);

            _toggle.OnClickEnded += _toggle_OnClickEnded;
        }

        private void _list_OnSelectionChanged(UIListBox o)
        {
            _text.Text = o.SelectedItem.Text;
            OnSelectionChanged?.Invoke(this);
            SetListVisibility(false);
        }

        public void AddItem(UIListItem item)
        {
            _list.AddItem(item);
            UpdateListBounds();
        }

        public void RemoveItem(UIListItem item)
        {
            _list.RemoveItem(item);
            UpdateListBounds();
        }

        private void _toggle_OnClickEnded(UIEventData<Input.MouseButton> data)
        {
            SetListVisibility(!_listVisible);
        }

        private void SetListVisibility(bool visible)
        {
            _listVisible = visible;
            if (_listVisible)
                _ui.AddUI(_list);
            else
                _ui.RemoveUI(_list);
        }

        private void UpdateListBounds()
        {
            _list.LocalBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Bottom,
                Width = _globalBounds.Width,
                Height = Math.Min(_maxItems, _list.ItemCount) * ITEM_HEIGHT,
            };
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            int buttonWidth = Math.Min(BUTTON_SIZE, _globalBounds.Width);
            int boxWidth = Math.Max(0, _globalBounds.Width - BUTTON_SIZE);

            _boxBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Y,
                Width = boxWidth,
                Height = _globalBounds.Height,
            };

            _innerBounds = Padding.ApplyPadding(_boxBounds);
            _text.Bounds = _innerBounds;

            UpdateListBounds();

            _toggle.LocalBounds = new Rectangle()
            {
                X = _globalBounds.Right - buttonWidth,
                Y = _globalBounds.Top,
                Width = buttonWidth,
                Height = _globalBounds.Height,
            };
        }

        protected override void OnRender(SpriteBatch sb)
        {
            base.OnRender(sb);

            // Draw main box
            sb.DrawRect(_boxBounds, _borderColor);
            sb.DrawRect(_innerBounds, _bgColor);

            _text.Draw(sb);
        }

        /// <summary>Gets or sets the maximum number of items that are visible in the item list, without needing to scroll.</summary>
        [DataMember]
        [DisplayName("Max Visible Items")]
        public int MaxVisibleItems
        {
            get { return _maxItems; }
            set
            {
                _maxItems = value;
                OnUpdateBounds();
            }
        }

        [DataMember]
        [DisplayName("Background Color")]
        public Color BackgroundColor
        {
            get { return _bgColor; }
            set { _bgColor = value; }
        }

        [DataMember]
        [DisplayName("Border Color")]
        public Color BoderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        [Browsable(false)]
        /// <summary>Gets the currently selected item.</summary>
        public UIListItem SelectedItem
        {
            get { return _list.SelectedItem; }
        }

        [Browsable(false)]
        /// <summary>Gets or sets the selected index</summary>
        public int SelectedIndex
        {
            get { return _list.SelectedIndex; }
            set { _list.SelectedIndex = value; }
        }

        /// <summary>Gets the listbox used to display the combo-box items.</summary>
        [DataMember]
        [DisplayName("List Box")]
        public UIListBox ListBox
        {
            get { return _list; }
        }

        /// <summary>Gets the text representing the currently selected item.</summary>
        [DataMember]
        [DisplayName("Selection Text")]
        public UIRenderedText SelectionText
        {
            get { return _text; }
        }
    }
}
