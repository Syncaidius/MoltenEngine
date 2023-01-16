using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public unsafe class ShaderSampler : ContextBindable<ID3D11SamplerState>, IShaderSampler
    {
        internal override unsafe ID3D11SamplerState* NativePtr => _native;

        ID3D11SamplerState* _native;
        SamplerDesc _desc;
        Color4 _borderColor;
        bool _isDirty;

        internal ShaderSampler(DeviceDX11 device, ShaderSampler source) : base(device, ContextBindTypeFlags.Input)
        {
            _desc = source._desc;
            _isDirty = true;
        }

        internal ShaderSampler(DeviceDX11 device) : base(device, ContextBindTypeFlags.Input)
        {
            // See for defaults: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_sampler_desc
            _desc = new SamplerDesc()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                MinLOD = float.MinValue,
                MaxLOD = float.MaxValue,
                MipLODBias = 0f,
                MaxAnisotropy = 1,
                ComparisonFunc = Silk.NET.Direct3D11.ComparisonFunc.Never
            };

            BorderColor = Color4.White;

            CheckIfComparisonSampler();
            _isDirty = true;
        }

        private void CheckIfComparisonSampler()
        {
            IsComparisonSampler = _desc.Filter >= Filter.ComparisonMinMagMipPoint &&
                    _desc.Filter <= Filter.ComparisonAnisotropic;
        }

        protected override void OnApply(CommandQueueDX11 pipe)
        {
            // If the sampler was actually dirty, recreate it.
            if (_isDirty)
            {
                int fVal = (int)_desc.Filter;
                PipelineRelease();

                pipe.DXDevice.Ptr->CreateSamplerState(ref _desc, ref _native);
                _isDirty = false;
                Version++;
            }
        }

        internal override void PipelineRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        /// <summary>Sets the entire sampler state description.</summary>
        /// <param name="description">The description to apply to the state.</param>
        internal void SetDescription(ref SamplerDesc description)
        {
            _desc = description;
            CheckIfComparisonSampler();
            _isDirty = true;
        }

        /// <summary>Gets or sets the method to use for resolving a U texture coordinate that is outside the 0 to 1 range.</summary>
        public SamplerAddressMode AddressU
        {
            get { return _desc.AddressU.FromApi(); }
            set
            {
                _desc.AddressU = value.ToApi();
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the method to use for resolving a V texture coordinate that is outside the 0 to 1 range.</summary>
        public SamplerAddressMode AddressV
        {
            get { return _desc.AddressV.FromApi(); }
            set
            {
                _desc.AddressV = value.ToApi();
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the method to use for resolving a W texture coordinate that is outside the 0 to 1 range.</summary>
        public SamplerAddressMode AddressW
        {
            get { return _desc.AddressW.FromApi(); }
            set
            {
                _desc.AddressW = value.ToApi();
                _isDirty = true;
            }
        }

        /// <summary>Border color to use if SharpDX.Direct3D11.TextureAddressMode.Border is specified 
        /// for AddressU, AddressV, or AddressW. Range must be between 0.0 and 1.0 inclusive.</summary>
        public Color4 BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                _desc.BorderColor[0] = _borderColor.R;
                _desc.BorderColor[1] = _borderColor.G;
                _desc.BorderColor[2] = _borderColor.B;
                _desc.BorderColor[3] = _borderColor.A;
                _isDirty = true;
            }
        }

        /// <summary>A function that compares sampled data against existing sampled data. 
        /// The function options are listed in SharpDX.Direct3D11.Comparison.</summary>
        public ComparisonMode ComparisonFunc
        {
            get { return _desc.ComparisonFunc.FromApi(); }
            set
            {
                _desc.ComparisonFunc = value.ToApi();
                CheckIfComparisonSampler();
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the filtering method to use when sampling a texture (see SharpDX.Direct3D11.Filter).</summary>
        public SamplerFilter FilterMode
        {
            get { return _desc.Filter.FromApi(); }
            set
            {
                _desc.Filter = value.ToApi();
                IsComparisonSampler = _desc.Filter >= Filter.ComparisonMinMagMipPoint && 
                    _desc.Filter <= Filter.ComparisonAnisotropic;
                _isDirty = true;
            }
        }

        /// <summary>Clamping value used if SharpDX.Direct3D11.Filter.Anisotropic or SharpDX.Direct3D11.Filter.ComparisonAnisotropic 
        /// is specified in SamplerFilter. Valid values are between 1 and 16.</summary>
        public uint MaxAnisotropy
        {
            get { return _desc.MaxAnisotropy; }
            set
            {
                _desc.MaxAnisotropy = value;
                _isDirty = true;
            }
        }
        /// <summary>Upper end of the mipmap range to clamp access to, where 0 is the largest
        ///     and most detailed mipmap level and any level higher than that is less detailed.
        ///     This value must be greater than or equal to MinLOD. To have no upper limit
        ///     on LOD set this to a large value such as D3D11_FLOAT32_MAX.</summary>
        public float MaxMipMapLod
        {
            get { return _desc.MaxLOD; }
            set
            {
                _desc.MaxLOD = value;
                _isDirty = true;
            }
        }

        /// <summary>Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level 
        /// and any level higher than that is less detailed.</summary>
        public float MinMipMapLod
        {
            get { return _desc.MinLOD; }
            set
            {
                _desc.MinLOD = value;
                _isDirty = true;
            }
        }

        /// <summary>Gets or sets the offset from the calculated mipmap level. For example, if Direct3D calculates 
        /// that a texture should be sampled at mipmap level 3 and MipLODBias is 2, then 
        /// the texture will be sampled at mipmap level 5.</summary>
        public float LodBias
        {
            get { return _desc.MipLODBias; }
            set
            {
                _desc.MipLODBias = value;
                _isDirty = true;
            }
        }

        /// <summary>Gets whether or not the sampler a comparison sampler. This is determined by the <see cref="FilterMode"/> mode.</summary>
        public bool IsComparisonSampler { get; private set; }
    }
}
