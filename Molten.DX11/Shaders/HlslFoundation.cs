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
        internal ShaderStateBank<ShaderSampler>[] Samplers;

        /// <summary>
        /// The available rasterizer state.
        /// </summary>
        internal ShaderStateBank<GraphicsRasterizerState> RasterizerState = new ShaderStateBank<GraphicsRasterizerState>();

        /// <summary>
        /// The available blend state.
        /// </summary>
        internal ShaderStateBank<GraphicsBlendState> BlendState = new ShaderStateBank<GraphicsBlendState>();

        /// <summary>
        ///The available depth state.
        /// </summary>
        internal ShaderStateBank<GraphicsDepthState> DepthState = new ShaderStateBank<GraphicsDepthState>();

        internal GraphicsDeviceDX11 Device { get; private set; }

        internal HlslFoundation(GraphicsDeviceDX11 device)
        {
            Device = device;
            Samplers = new ShaderStateBank<ShaderSampler>[0];
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
