using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ShaderInputStage<H> : PipelineComponent<DeviceDX11, PipeDX11>
        where H: HlslShader
    {
        protected BindableValue<H> _shader;

        internal ShaderInputStage(PipeDX11 pipe) : base(pipe)
        {
            _shader = new BindableValue<H>();
        }

        protected ShaderStep<S, C, H> CreateStep<S, C>(C commonShaderStage, Action<C, ShaderComposition<S>> setCallback)
            where S : DeviceChild
            where C : CommonShaderStage
        {
            return new ShaderStep<S, C, H>(Pipe, this, commonShaderStage, setCallback);
        }

        protected void BindComposition<S, C>(ShaderComposition<S> composition) 
            where S : DeviceChild
            where C : CommonShaderStage
        {

        }

        public H Shader
        {
            get => _shader.Value;
            set => _shader.Value = value;
        }

        internal H BoundShader => _shader.BoundValue;
    }
}
