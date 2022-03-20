namespace Molten.UI
{
    internal struct UIAddChildChange : IUIChange
    {
        internal UIRenderData Parent;
        internal UIRenderData Child;

        public void Process()
        {
            Parent.Children.Add(Child);
        }
    }
}
