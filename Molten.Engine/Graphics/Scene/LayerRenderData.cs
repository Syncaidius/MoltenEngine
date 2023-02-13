namespace Molten.Graphics
{
    public class LayerRenderData
    {
        public readonly Dictionary<Renderable, RenderDataBatch> Renderables = new Dictionary<Renderable, RenderDataBatch>();

        internal LayerRenderData(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"Layer '{Name}' - {Renderables.Count} objects";
        }

        public string Name { get; set; }
    }
}
