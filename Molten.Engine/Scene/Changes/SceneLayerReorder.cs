namespace Molten
{
    /// <summary>A <see cref="SceneLayerReorder"/> for changing the draw order of a <see cref="SceneLayer"/> instance.</summary>
    internal class SceneLayerReorder : SceneChange<SceneLayerReorder>
    {
        public SceneLayer Layer;
        public ReorderMode Mode;

        public override void ClearForPool()
        {
            Layer = null;
        }

        internal override void Process(Scene scene)
        {
            int indexOf = scene.Layers.IndexOf(Layer);
            if (indexOf > -1)
            {
                scene.Layers.RemoveAt(indexOf);

                switch (Mode)
                {
                    case ReorderMode.PushBackward:
                        Layer.LayerID = Math.Max(0, indexOf - 1);
                        scene.Layers.Insert(Layer.LayerID, Layer);
                        break;

                    case ReorderMode.BringToFront:
                        Layer.LayerID = scene.Layers.Count;
                        scene.Layers.Add(Layer);
                        break;

                    case ReorderMode.PushForward:
                        if (indexOf + 1 < scene.Layers.Count)
                        {
                            Layer.LayerID = indexOf + 1;
                            scene.Layers.Insert(Layer.LayerID, Layer);
                        }
                        else
                        {
                            Layer.LayerID = scene.Layers.Count;
                            scene.Layers.Add(Layer);
                        }
                        break;

                    case ReorderMode.SendToBack:
                        scene.Layers.Insert(0, Layer);
                        Layer.LayerID = 0;
                        break;
                }
            }

            // Now pass this change to the render data too.
            scene.RenderData.ReorderLayer(Layer.Data, Mode);
            Recycle(this);
        }
    }
}
