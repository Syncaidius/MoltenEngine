using System.Xml.Linq;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// A control for listing <see cref="UIListViewItem"/> 
    /// </summary>
    public class UIListView : UIStackPanel
    {
        public event UIElementHandler<UIListViewItem> SelectionChanged;

        UIListViewItem _selectedItem;

        protected override Type[] OnGetChildFilter()
        {
            return new Type[] { typeof(UIListViewItem) };
        }

        protected override void OnChildAdded(UIElement obj)
        {
            obj.Pressed += Item_Pressed;

            Rectangle lb = obj.LocalBounds;
            lb.Width = LocalBounds.Width;
            obj.LocalBounds = lb;

            base.OnChildAdded(obj);
        }

        protected override void OnChildRemoved(UIElement obj)
        {
            obj.Released += Item_Released;
            base.OnChildRemoved(obj);
        }

        private void Item_Released(UIElement element, CameraInputTracker tracker)
        {
            
        }

        private void Item_Pressed(UIElement element, CameraInputTracker tracker)
        {
            SelectedItem = element as UIListViewItem;
        }

        /// <summary>
        /// Gets or sets the selected <see cref="UIListViewItem"/> for the current <see cref="UIListView"/>. 
        /// <para>Setting <see cref="SelectedItem"/> to null will clear the selection of the current <see cref="UIListView"/>.</para>
        /// </summary>
        public UIListViewItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if(_selectedItem != value)
                {
                    if (value.ParentElement != this)
                        throw new Exception("The provided list view item does not belong to the current list-view");

                    if (_selectedItem != null)
                        _selectedItem.IsSelected = false;

                    value.IsSelected = true;
                    _selectedItem = value;
                    SelectionChanged?.Invoke(_selectedItem);
                }
            }
        }
    }
}
