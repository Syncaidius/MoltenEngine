using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsState : GraphicsObject
    {
        protected GraphicsState(GraphicsDevice device) :
            base(device, GraphicsBindTypeFlags.Input)
        {
            
        }

        /// <summary>
        /// Gets or sets the depth write permission. the default value is <see cref="GraphicsDepthWritePermission.Enabled"/>.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Enum)]
        public GraphicsDepthWritePermission WritePermission { get; set; } = GraphicsDepthWritePermission.Enabled;
    }
}
