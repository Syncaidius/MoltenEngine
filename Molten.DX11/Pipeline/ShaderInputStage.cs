using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ShaderInputStage<T> : PipelineComponent where T: HlslShader
    {
        protected BindableValue<T> _shader;

        internal ShaderInputStage(GraphicsPipe pipe) : base(pipe)
        {
            _shader = new BindableValue<T>();
        }

        protected ShaderStep<S, C, T> CreateStep<S, C>(C commonShaderStage)
            where S : DeviceChild
            where C : CommonShaderStage
        {
            return new ShaderStep<S, C, T>(Pipe, this, commonShaderStage);
        }

        protected void BindComposition<S, C>(ShaderComposition<S> composition) 
            where S : DeviceChild
            where C : CommonShaderStage
        {

        }

        public T Shader
        {
            get => _shader.Value;
            set => _shader.Value = value;
        }

        internal T BoundShader => _shader.BoundValue;
    }
}
