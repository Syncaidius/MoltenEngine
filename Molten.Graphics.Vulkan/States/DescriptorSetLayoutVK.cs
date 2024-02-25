using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class DescriptorSetLayoutVK : GraphicsObject<DeviceVK>, IEquatable<DescriptorSetLayoutVK>, IEquatable<DescriptorSetLayoutCreateInfo>
{
    DescriptorSetLayoutBinding* _ptrBindings;
    DescriptorSetLayout _handle;
    DescriptorSetLayoutCreateInfo _info;
    List<DescriptorSetLayoutBinding> _layoutBindings;

    public DescriptorSetLayoutVK(DeviceVK device, ShaderPassVK pass) :
        base(device)
    {
        _layoutBindings = new List<DescriptorSetLayoutBinding>();
        ShaderBindManager bindings = pass.Parent.Bindings;

        for(int i = 0; i < bindings.Resources.Length; i++)
        {
            ref ShaderBindPoint<ShaderResourceVariable>[] variable = ref bindings.Resources[i];
            for(int j = 0; j < variable.Length; j++)
            {
                ref ShaderBindPoint<ShaderResourceVariable> bp = ref variable[j];
                DescriptorSetLayoutBinding binding = new DescriptorSetLayoutBinding()
                {
                    DescriptorType = GetDescriptorType((ShaderBindType)i, bp.Object),
                    Binding = bp.BindPoint,
                    DescriptorCount = 1,
                    PImmutableSamplers = null,
                    StageFlags = GetShaderStageFlags(pass, bp.Object),
                };

                _layoutBindings.Add(binding);
            }
        }

        _ptrBindings = EngineUtil.AllocArray<DescriptorSetLayoutBinding>((uint)_layoutBindings.Count);
        for (int i = 0; i < _layoutBindings.Count; i++)
            _ptrBindings[i] = _layoutBindings[i];

        _info = new DescriptorSetLayoutCreateInfo()
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            Flags = DescriptorSetLayoutCreateFlags.None,
            BindingCount = (uint)_layoutBindings.Count,
            PBindings = _ptrBindings,
            PNext = null,
        };

        Result r = Result.Success;
        fixed (DescriptorSetLayout* ptrLayout = &_handle)
            r = device.VK.CreateDescriptorSetLayout(device, _info, null, ptrLayout);

        if (r != Result.Success)
            r.Throw(device, () => $"Failed to create descriptor set layout for shader pass '{pass.Name}' of shader '{pass.Parent.Name}'");
    }

    public bool Equals(DescriptorSetLayoutVK other) => Equals(other._info);

    public bool Equals(DescriptorSetLayoutCreateInfo other)
    {
        bool equal = _info.BindingCount == other.BindingCount &&
            _info.Flags == other.Flags &&
            _info.PNext == other.PNext;

        if (!equal)
            return false;

        // Check binding equality.
        for (int i = 0; i < _info.BindingCount; i++)
        {
            if (!BindingsEqual(ref _info.PBindings[i], ref other.PBindings[i]))
                return false;
        }

        return true;
    }

    private bool BindingsEqual(ref DescriptorSetLayoutBinding a, ref DescriptorSetLayoutBinding b)
    {
        return a.Binding == b.Binding &&
            a.DescriptorCount == b.DescriptorCount &&
            a.DescriptorType == b.DescriptorType &&
            a.PImmutableSamplers == b.PImmutableSamplers &&
            a.StageFlags == b.StageFlags;

        // TODO check PImmutableSampler values, if any. Need to store count to iterate.
    }

    private ShaderStageFlags GetShaderStageFlags(ShaderPassVK pass, ShaderResourceVariable variable)
    {
        ShaderStageFlags flags = ShaderStageFlags.None;

        foreach(ShaderStageType type in PipelineStateVK.ShaderTypes)
        {
            pass[type].Bindings.OnFind(variable, (ref ShaderBindPoint<ShaderResourceVariable> v) =>
                flags |= PipelineStateVK.ShaderStageLookup[type]);
        }

        return flags;
    }

    private DescriptorType GetDescriptorType(ShaderBindType bindType, ShaderVariable variable)
    {
        switch (variable)
        {
            case ShaderResourceVariable<ITexture1D>:
            case ShaderResourceVariable<ITexture2D>:
            case ShaderResourceVariable<ITexture3D>:
            case ShaderResourceVariable<ITextureCube>:
                return DescriptorType.SampledImage;

            case ShaderResourceVariable<GraphicsBuffer>:
            case RWVariable<GraphicsBuffer>:
                return DescriptorType.StorageBuffer;

            case RWVariable<ITexture1D>:
            case RWVariable<ITexture2D>:
            case RWVariable<ITexture3D>:
            case RWVariable<ITextureCube>:
                return DescriptorType.StorageImage;

            default:
                throw new NotSupportedException($"The provided shader variable type is not supported: {variable.GetType().Name}");
        }
    }

    protected override void OnGraphicsRelease()
    {
        if (_handle.Handle != 0)
        {
            Device.VK.DestroyDescriptorSetLayout(Device, _handle, null);
            _handle = new DescriptorSetLayout();
        }

        EngineUtil.Free(ref _ptrBindings);
    }

    public DescriptorSetLayout Handle => _handle;

    public IReadOnlyList<DescriptorSetLayoutBinding> Bindings => _layoutBindings;
}
