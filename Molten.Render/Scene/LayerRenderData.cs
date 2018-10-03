using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class LayerRenderData
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{base.ToString()} - {Name}";
        }
    }

    public class LayerRenderData<R> : LayerRenderData
        where R : IRenderable
    {
        public readonly Dictionary<R, List<ObjectRenderData>> Renderables = new Dictionary<R, List<ObjectRenderData>>();

        public override string ToString()
        {
            return $"{base.ToString()} - {Renderables.Count} objects";
        }
    }
}
