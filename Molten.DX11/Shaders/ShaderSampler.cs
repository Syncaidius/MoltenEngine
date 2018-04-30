using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public class ShaderSampler : PipelineObject, IShaderSampler
    {
        SamplerState _state;
        SamplerStateDescription _description;
        bool _isDirty;
        bool _isComparison;

        TextureAddressMode _wrapU;
        TextureAddressMode _wrapV;
        TextureAddressMode _wrapW;
        Comparison _comparison;
        Filter _filter;

        internal ShaderSampler(ShaderSampler source)
        {
            _description = source._description;
            _isDirty = true;

            _wrapU = _description.AddressU;
            _wrapV = _description.AddressV;
            _wrapW = _description.AddressW;
        }

        internal ShaderSampler()
        {
            _description = SamplerStateDescription.Default();
            _isDirty = true;

            _wrapU = _description.AddressU;
            _wrapV = _description.AddressV;
            _wrapW = _description.AddressW;
        }

        internal override void Refresh(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            base.Refresh(pipe, slot);

            if (_isDirty)
            {
                // If the sampler was actually dirty, recreate it.
                if (_isDirty)
                {
                    int fVal = (int)_description.Filter;

                    // Dispose the old sampler
                    DisposeObject(ref _state);

                    _state = new SamplerState(pipe.Device.D3d, _description);
                }

                _isDirty = false;
            }
        }

        protected override void OnDispose()
        {
            if(_state != null)
                _state.Dispose();
        }

        /// <summary>Sets the entire sampler state description.</summary>
        /// <param name="description">The description to apply to the state.</param>
        internal void SetDescription(SamplerStateDescription description)
        {
            _description = description;
            _isDirty = true;
        }

        /// <summary>Gets or sets the method to use for resolving a U texture coordinate that is outside the 0 to 1 range.</summary>
        public SamplerAddressMode AddressU
        {
            get { return _wrapU.FromApi(); }
            set
            {
                _wrapU = value.ToApi();
                _description.AddressU = _wrapU;
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the method to use for resolving a V texture coordinate that is outside the 0 to 1 range.</summary>
        public SamplerAddressMode AddressV
        {
            get { return _wrapV.FromApi(); }
            set
            {
                _wrapV = value.ToApi();
                _description.AddressV = _wrapV;
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the method to use for resolving a W texture coordinate that is outside the 0 to 1 range.</summary>
        public SamplerAddressMode AddressW
        {
            get { return _wrapW.FromApi(); }
            set
            {
                _wrapW = value.ToApi();
                _description.AddressW = _wrapW;
                _isDirty = true;
            }
        }

        /// <summary>Border color to use if SharpDX.Direct3D11.TextureAddressMode.Border is specified 
        /// for AddressU, AddressV, or AddressW. Range must be between 0.0 and 1.0 inclusive.</summary>
        public Color4 BorderColor
        {
            get { return _description.BorderColor.FromRawApi(); }
            set
            {
                _description.BorderColor = value.ToApi();
                _isDirty = true;
            }
        }

        /// <summary>A function that compares sampled data against existing sampled data. 
        /// The function options are listed in SharpDX.Direct3D11.Comparison.</summary>
        public ComparisonMode ComparisonFunc
        {
            get { return _comparison.FromApi(); }
            set
            {
                _comparison = value.ToApi();
                _description.ComparisonFunction = _comparison;
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the filtering method to use when sampling a texture (see SharpDX.Direct3D11.Filter).</summary>
        public SamplerFilter Filter
        {
            get { return _filter.FromApi(); }
            set
            {
                _filter = value.ToApi();
                _description.Filter = _filter;

                int fVal = (int)_description.Filter;
                _isComparison = fVal >= 128 && fVal <= 213;
                _isDirty = true;
            }
        }

        /// <summary>Clamping value used if SharpDX.Direct3D11.Filter.Anisotropic or SharpDX.Direct3D11.Filter.ComparisonAnisotropic 
        /// is specified in SamplerFilter. Valid values are between 1 and 16.</summary>
        public int MaxAnisotropy
        {
            get { return _description.MaximumAnisotropy; }
            set
            {
                _description.MaximumAnisotropy = value;
                _isDirty = true;
            }
        }
        /// <summary>Upper end of the mipmap range to clamp access to, where 0 is the largest
        ///     and most detailed mipmap level and any level higher than that is less detailed.
        ///     This value must be greater than or equal to MinLOD. To have no upper limit
        ///     on LOD set this to a large value such as D3D11_FLOAT32_MAX.</summary>
        public float MaxMipMapLod
        {
            get { return _description.MaximumLod; }
            set
            {
                _description.MaximumLod = value;
                _isDirty = true;
            }
        }

        /// <summary>Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level 
        /// and any level higher than that is less detailed.</summary>
        public float MinMipMapLod
        {
            get { return _description.MinimumLod; }
            set
            {
                _description.MinimumLod = value;
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the offset from the calculated mipmap level. For example, if Direct3D calculates 
        /// that a texture should be sampled at mipmap level 3 and MipLODBias is 2, then 
        /// the texture will be sampled at mipmap level 5.</summary>
        public float LodBias
        {
            get { return _description.MipLodBias; }
            set
            {
                _description.MipLodBias = value;
                _isDirty = true;
            }
        }

        /// <summary>Gets the underlying sampler state.</summary>
        internal SamplerState State { get { return _state; } }

        /// <summary>Gets whether or not the sampler a comparison sampler. This is determined by the <see cref="Filter"/> mode.</summary>
        public bool IsComparisonSampler => _isComparison;
    }
}
