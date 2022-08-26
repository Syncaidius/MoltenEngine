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

        private void Item_Released(UIElement element, ScenePointerTracker tracker)
        {
            
        }

        private void Item_Pressed(UIElement element, ScenePointerTracker tracker)
        {
            UIListViewItem item = element as UIListViewItem;
            if (element != _selectedItem)
            {
                if(_selectedItem != null)
                    _selectedItem.IsSelected = false;

                item.IsSelected = true;
                _selectedItem = item;
            }
        }
    }
}
