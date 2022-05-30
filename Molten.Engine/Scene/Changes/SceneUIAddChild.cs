using Molten.UI;

namespace Molten
{
    internal class SceneUIAddChild : SceneChange<SceneUIAddChild>
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
            Parent.Children.Add(Child);
            Recycle(this);
        }
    }
}
