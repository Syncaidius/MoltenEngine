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
        /// The texture samplers to be used with the shader/component.
        /// </summary>
        internal ShaderSampler[] Samplers;

        /// <summary>
        /// The rasterizer state.
        /// </summary>
        internal GraphicsRasterizerState RasterizerState;

        /// <summary>
        /// The blend state.
        /// </summary>
        internal GraphicsBlendState BlendState;

        /// <summary>
        ///The depth state.
        /// </summary>
        internal GraphicsDepthState DepthState;

        internal GraphicsDevice Device { get; private set; }

        internal HlslFoundation(GraphicsDevice device)
        {
            Device = device;
            Samplers = new ShaderSampler[0];
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; internal set; } = "<no name>";

        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        public int Iterations { get; set; } = 1;
    }
}
