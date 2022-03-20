namespace Molten.UI
{
    internal struct UIRemoveChildChange : IUIChange
    {
        internal UIRenderData Parent;

        internal UIRenderData Child;

        public void Process()
        {
            Parent.Children.Remove(Child);
        }
    }
}
