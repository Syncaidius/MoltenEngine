using Molten.Graphics;
using Molten.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIListBox : UICompoundComponent
    {
        public const int ITEM_HEIGHT = 25;
        public const int LIST_BORDER_THICKNESS = 2;

        List<UIListItem> _items;
        UIScrollArea _scroll;

        Color _bgColor;
        Color _borderColor;
        Color _selectedColor;
        UIListItem _selected;

        public event ObjectHandler<UIListBox> OnSelectionChanged;

        public UIListBox(UISystem ui) : base(ui)
        {
            _items = new List<UIListItem>();
            _bgColor = new Color(90, 90, 90, 255);
            _borderColor = new Color(120, 120, 120, 255);
            _selectedColor = new Color(120, 120, 190, 255);
            _scroll = new UIScrollArea(ui);

            Padding.Left = 2;
            Padding.Right = 2;
            Padding.Top = 2;
            Padding.Bottom = 2;

            AddPart(_scroll);
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();
            _scroll.LocalBounds = _clippingBounds;
        }

        /// <summary>Adds an item to the list box.</summary>
        /// <param name="item"></param>
        public void AddItem(UIListItem item)
        {
            if (item.Index > -1)
                return;

            item.Button = new UIButton(_ui)
            {
                DefaultColor = _bgColor, 
            };

            item.Index = _items.Count;
            item.Button.Tag = item;
            item.Button.Text.Text = item.Text;
            item.Button.OnClickEnded += Button_OnClickEnded;
            item.Button.LocalBounds = new Rectangle()
            {
                X = 0,
                Y = _items.Count * ITEM_HEIGHT,
                Width = _clippingBounds.Width,
                Height = ITEM_HEIGHT,
            };

            _scroll.AddChild(item.Button);
            _items.Add(item);
        }

        /// <summary>Removes an item from the list box.</summary>
        /// <param name="item"></param>
        public void RemoveItem(UIListItem item) {
            if (item.Index == -1)
                return;

            _items.Remove(item);

            // Update the index of all items ahead of the one being removed
            for (int i = item.Index; i < _items.Count; i++)
            {
                Rectangle bounds = _items[i].Button.LocalBounds;
                bounds.Y -= ITEM_HEIGHT;
                _items[i].Button.LocalBounds = bounds;
                _items[i].Index = i;
            }

            item.Index = -1;
            _scroll.RemoveChild(item.Button);

            if (item == _selected)
            {
                item.Button.Dispose();
                item.Button = null;
                _selected = null;
            }
        }

        private void Button_OnClickEnded(UIEventData<Input.MouseButton> data)
        {
            UIButton btn = data.Component as UIButton;
            UIListItem item = btn.Tag as UIListItem;
            SetSelected(item);
        }

        private void SetSelected(UIListItem item) {
            if (_selected != item)
            {
                if (_selected != null)
                    _selected.Button.DefaultColor = _bgColor;

                _selected = item;
                _selected.Button.DefaultColor = _selectedColor;
                OnSelectionChanged?.Invoke(this);
            }
        }

        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
        {
            base.OnRender(sb, proxy);

            // Draw main box
            sb.Draw(_globalBounds, _borderColor);
            sb.Draw(_clippingBounds, _bgColor);
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

        [DataMember]
        [DisplayName("Selected Color")]
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { _selectedColor = value; }
        }

        [Browsable(false)]
        /// <summary>Gets the number of avaiable items in the listbox.</summary>
        public int ItemCount
        {
            get { return _items.Count; }
        }

        [Browsable(false)]
        /// <summary>Gets the currently selected item, or null if none are selected.</summary>
        public UIListItem SelectedItem
        {
            get { return _selected; }
        }

        [Browsable(false)]
        public int SelectedIndex
        {
            get { return _selected.Index; }
            set
            {
                SetSelected(_items[value]);
            }            
        }
    }
}
