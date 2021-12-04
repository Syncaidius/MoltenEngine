using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a managed device context pipeline stage.
    /// </summary>
    internal unsafe abstract class PipeShaderStage : PipeStage
    {
        
        internal PipeShaderStage(PipeDX11 pipe, ShaderType shaderType) :
           base(pipe, shaderType.ToStageType())
        {
            DefineSlots
        }

        protected override void OnDispose()
        {
            
        }

        /// <summary>
        /// Bind the stage to it's <see cref="ID3D11DeviceContext1"/> context.
        /// </summary>
        internal override void Bind()
        {

        }

        protected abstract void OnBind();
    }
}
