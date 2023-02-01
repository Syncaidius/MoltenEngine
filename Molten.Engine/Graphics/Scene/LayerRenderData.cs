namespace Molten.Graphics
{
    public class LayerRenderData
    {
        public readonly Dictionary<Renderable, List<ObjectRenderData>> Renderables = new Dictionary<Renderable, List<ObjectRenderData>>();

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
