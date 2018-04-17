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
            _vStage = CreateStep<VertexShader, VertexShaderStage>(pipe.Context.VertexShader);
            _vStage.OnSetShader += _vStage_OnSetShader;

            _gStage = CreateStep<GeometryShader, GeometryShaderStage>(pipe.Context.GeometryShader);
            _gStage.OnSetShader += _gStage_OnSetShader;

            _hStage = CreateStep<HullShader, HullShaderStage>(pipe.Context.HullShader);
            _hStage.OnSetShader += _hStage_OnSetShader; ;

            _dStage = CreateStep<DomainShader, DomainShaderStage>(pipe.Context.DomainShader);
            _dStage.OnSetShader += _dStage_OnSetShader; ;

            _pStage = CreateStep<PixelShader, PixelShaderStage>(pipe.Context.PixelShader);
            _pStage.OnSetShader += _pStage_OnSetShader;
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

        internal override void Refresh()
        {
            // Reset pass number to 0 if the shader just changed.
            _hasMaterialChanged = _shader.Bind();

            if (_shader.BoundValue != null)
            {
                MaterialPass pass = _shader.BoundValue.Passes[_passNumber];
                _vStage.Refresh(_shader.Value, pass.VertexShader);
                _gStage.Refresh(_shader.Value, pass.GeometryShader);
                _hStage.Refresh(_shader.Value, pass.HullShader);
                _dStage.Refresh(_shader.Value, pass.DomainShader);
                _pStage.Refresh(_shader.Value, pass.PixelShader);
            }
        }

        /// <summary>Gets or sets the current number of completed passes on the current material.</summary>
        internal int PassNumber
        {
            get => _passNumber;
            set => _passNumber = value;
        }

        internal MaterialPass CurrentPass => _shader.BoundValue != null ? _shader.BoundValue.Passes[_passNumber] : null;

        /// <summary>Gets whether or not the material was changed during the last refresh call.</summary>
        internal bool HasMaterialChanged => _hasMaterialChanged;
    }
}
