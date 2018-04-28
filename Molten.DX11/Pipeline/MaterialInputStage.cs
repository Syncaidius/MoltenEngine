using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class MaterialInputStage : ShaderInputStage<Material>
    {
        ShaderStep<VertexShader, VertexShaderStage, Material> _vStage;
        ShaderStep<GeometryShader, GeometryShaderStage, Material> _gStage;
        ShaderStep<HullShader, HullShaderStage, Material> _hStage;
        ShaderStep<DomainShader, DomainShaderStage, Material> _dStage;
        ShaderStep<PixelShader, PixelShaderStage, Material> _pStage;

        int _passNumber = 0;
        bool _hasMaterialChanged;

        internal MaterialInputStage(GraphicsPipe pipe) : base(pipe)
        {
            _vStage = CreateStep<VertexShader, VertexShaderStage>(pipe.Context.VertexShader, (stage, composition) => stage.Set(composition.RawShader));
            _gStage = CreateStep<GeometryShader, GeometryShaderStage>(pipe.Context.GeometryShader, (stage, composition) => stage.Set(composition.RawShader));
            _hStage = CreateStep<HullShader, HullShaderStage>(pipe.Context.HullShader, (stage, composition) => stage.Set(composition.RawShader));
            _dStage = CreateStep<DomainShader, DomainShaderStage>(pipe.Context.DomainShader, (stage, composition) => stage.Set(composition.RawShader));
            _pStage = CreateStep<PixelShader, PixelShaderStage>(pipe.Context.PixelShader, (stage, composition) => stage.Set(composition.RawShader));
        }

        private void _pStage_OnSetShader(Material shader, ShaderComposition<PixelShader> composition, PixelShaderStage shaderStage)
        {
            shaderStage.Set(composition.RawShader);
        }

        private void _dStage_OnSetShader(Material shader, ShaderComposition<DomainShader> composition, DomainShaderStage shaderStage)
        {
            shaderStage.Set(composition.RawShader);
        }

        private void _hStage_OnSetShader(Material shader, ShaderComposition<HullShader> composition, HullShaderStage shaderStage)
        {
            shaderStage.Set(composition.RawShader);
        }

        private void _gStage_OnSetShader(Material shader, ShaderComposition<GeometryShader> composition, GeometryShaderStage shaderStage)
        {
            shaderStage.Set(composition.RawShader);
        }

        private void _vStage_OnSetShader(Material shader, ShaderComposition<VertexShader> composition, VertexShaderStage shaderStage)
        {
            shaderStage.Set(composition.RawShader);
        }

        internal void Refresh(int passID, StateConditions conditions)
        {
            // Reset pass number to 0 if the shader just changed.
            _hasMaterialChanged = _shader.Bind();

            if (_shader.BoundValue != null)
            {
                MaterialPass pass = _shader.BoundValue.Passes[passID];

                // Update samplers with those of the current pass
                int maxSamplers = Math.Min(_shader.BoundValue.SamplerVariables.Length, pass.Samplers.Length);
                for(int i = 0; i < maxSamplers; i++)
                    _shader.BoundValue.SamplerVariables[i].Value = pass.Samplers[i][conditions];

                // Refresh shader stages
                _vStage.Refresh(_shader.Value, pass.VertexShader);
                _gStage.Refresh(_shader.Value, pass.GeometryShader);
                _hStage.Refresh(_shader.Value, pass.HullShader);
                _dStage.Refresh(_shader.Value, pass.DomainShader);
                _pStage.Refresh(_shader.Value, pass.PixelShader);
            }
        }

        internal MaterialPass CurrentPass => _shader.BoundValue?.Passes[_passNumber];

        /// <summary>Gets whether or not the material was changed during the last refresh call.</summary>
        internal bool HasMaterialChanged => _hasMaterialChanged;
    }
}
