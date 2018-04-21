using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A helper class for storing references to common material properties.
    /// </summary>
    internal class CommonMaterialProperties
    {
        internal IShaderValue View { get; private set; }

        internal IShaderValue Projection { get; private set; }

        internal IShaderValue ViewProjection { get; private set; }

        internal IShaderValue InvViewProjection { get; private set; }

        internal CommonMaterialProperties(Material material)
        {
            View = material["view"];
            Projection = material["projection"];
            ViewProjection = material["viewProjection"];
            InvViewProjection = material["invViewProjection"];
        }
    }
}
