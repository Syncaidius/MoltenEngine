namespace Molten
{
    internal class SceneRemoveObject : SceneChange<SceneRemoveObject>
    {
        internal SceneObject Object;

        internal SceneLayer Layer;

        public override void ClearForPool()
        {
            Object = null;
            Layer = null;
        }

        internal override void Process()
        {
            if (Object.Layer == Layer)
            {
                Layer.Objects.Remove(Object);
                Object.Layer = null;
            }

            Recycle(this);
        }
    }
}
