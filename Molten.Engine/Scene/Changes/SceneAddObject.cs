namespace Molten;

internal class SceneAddObject : SceneChange<SceneAddObject>
{
    public SceneObject Object;

    public SceneLayer Layer;

    public override void ClearForPool()
    {
        Object = null;
    }

    internal override void Process()
    {
        if (Object.Scene != Scene)
        {
            // Remove from other scene
            if (Object.Layer != null)
                Object.Layer.Objects.Remove(Object);

            Object.Layer = Layer;
            Layer.Objects.Add(Object);
        }

        Recycle(this);
    }
}
