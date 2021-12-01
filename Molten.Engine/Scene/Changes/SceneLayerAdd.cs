namespace Molten
{
    internal class SceneLayerAdd : SceneChange<SceneLayerAdd>
    {
        public Scene ParentScene;

        public SceneLayer Layer;

        public override void ClearForPool()
        {
            Layer = null;
            ParentScene = null;
        }

        internal override void Process(Scene scene)
        {
            Layer.LayerID = ParentScene.Layers.Count;
            ParentScene.Layers.Add(Layer);
            Recycle(this);
        }
    }
}
