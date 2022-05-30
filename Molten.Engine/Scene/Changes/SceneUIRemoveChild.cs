using Molten.UI;

namespace Molten
{
    internal class SceneUIRemoveChild : SceneChange<SceneUIRemoveChild>
    {
        internal UIRenderData Parent;
        internal UIRenderData Child;

        public override void ClearForPool()
        {
            Parent = null;
            Child = null;
        }

        internal override void Process()
        {
            Parent.Children.Remove(Child);
            Recycle(this);
        }
    }
}
