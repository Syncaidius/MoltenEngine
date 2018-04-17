using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// An a base class implementation of key shader components (e.g. name, render states, samplers, etc).
    /// </summary>
    public abstract class HlslFoundation : PipelineObject
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; internal set; } = "<no name>";

        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        public int Iterations { get; set; } = 1;

        /// <summary>
        /// Gets or sets the rasterizer state.
        /// </summary>
        internal GraphicsRasterizerState RasterizerState { get; set; }

        /// <summary>
        /// Gets or sets the blend state.
        /// </summary>
        internal GraphicsBlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets the depth state.
        /// </summary>
        internal GraphicsDepthState DepthState { get; set; }

        /// <summary>
        /// Gets or sets the texture samplers to be used with the shader/component.
        /// </summary>
        internal ShaderSampler[] Samplers { get; set; }
    }
}
