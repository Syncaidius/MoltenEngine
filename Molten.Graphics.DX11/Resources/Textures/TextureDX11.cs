using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11;

public delegate void TextureEvent(TextureDX11 texture);

public unsafe abstract partial class TextureDX11 : GraphicsTexture
{
    ResourceHandleDX11<ID3D11Resource> _handle;

    ShaderResourceViewDesc1 _srvDesc;
    UnorderedAccessViewDesc1 _uavDesc;

    internal TextureDX11(DeviceDX11 device, 
        TextureDimensions dimensions, 
        AntiAliasLevel aaLevel, 
        MSAAQuality sampleQuality, 
        GraphicsFormat format, 
        GraphicsResourceFlags flags, 
        string name) :
        base(device, dimensions, aaLevel, sampleQuality, format, flags | GraphicsResourceFlags.GpuRead, name)
    {
        Device = device;
    }

    protected void SetDebugName(ID3D11Resource* resource, string debugName)
    {
        if (!string.IsNullOrWhiteSpace(debugName))
        {
            void* ptrName = (void*)SilkMarshal.StringToPtr(debugName, NativeStringEncoding.LPStr);
            resource->SetPrivateData(ref RendererDX11.WKPDID_D3DDebugObjectName, (uint)debugName.Length, ptrName);
            SilkMarshal.FreeString((nint)ptrName, NativeStringEncoding.LPStr);
        }
    }

    protected BindFlag GetBindFlags()
    {
        BindFlag result = Flags.ToBindFlags();

        if (this is RenderSurface2DDX11)
            result |= BindFlag.RenderTarget;

        if (this is DepthSurfaceDX11)
            result |= BindFlag.DepthStencil;

        return result;
    }

    protected override void OnCreateResource()
    {
        _handle?.Dispose();

        if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
            SetSRVDescription(ref _srvDesc);

        if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
            SetUAVDescription(ref _srvDesc, ref _uavDesc);


        _handle = CreateTexture(Device);

        SetDebugName(_handle.NativePtr, $"{Name}");

        if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
        {
            _handle.SRV.Desc = _srvDesc;
            _handle.SRV.Create();
        }

        if (Flags.Has(GraphicsResourceFlags.UnorderedAccess))
        {
            _handle.UAV.Desc = _uavDesc;
            _handle.UAV.Create();
        }
    }

    protected abstract ResourceHandleDX11<ID3D11Resource> CreateTexture(DeviceDX11 device);

    protected override void OnResizeTexture(ref readonly TextureDimensions dimensions, GraphicsFormat format)
    {
        UpdateDescription(dimensions, format);
        Dimensions = dimensions;

        OnCreateResource();
    }

    protected SubresourceData* GetImmutableData(Usage usage)
    {
        SubresourceData* subData = null;

        // Check if we're passing initial data to the texture.
        // Render surfaces and depth-stencil buffers cannot be initialized with data.
        if (this is not IRenderSurface)
        {
            // We can only pass data for immutable textures.
            if (usage == Usage.Immutable)
            {
                subData = EngineUtil.AllocArray<SubresourceData>(MipMapCount * ArraySize);

                // Immutable textures expect data to be provided via SetData() before other operations are performed on them.
                for (uint a = 0; a < ArraySize; a++)
                {
                    for (uint m = 0; m < MipMapCount; m++)
                    {
                        if (!DequeueTaskIfType(out TextureSetTask task))
                            throw new GraphicsResourceException(this, "Immutable texture SetData() was not called or did not provide enough data.");

                        if (task.MipLevel != m || task.ArrayIndex != a)
                            throw new GraphicsResourceException(this, "The provided immutable texture subresource data was not correctly ordered.");

                        uint subIndex = (a * MipMapCount) + m;
                        subData[subIndex] = new SubresourceData(task.Data, task.Pitch, task.NumBytes);
                    }
                }
            }
        }

        return subData;
    }

    protected abstract void SetUAVDescription(ref ShaderResourceViewDesc1 srvDesc, ref UnorderedAccessViewDesc1 desc);

    protected abstract void SetSRVDescription(ref ShaderResourceViewDesc1 desc);

    protected override void OnGraphicsRelease()
    {
        _handle?.Dispose();
    }

    protected abstract void UpdateDescription(TextureDimensions dimensions, GraphicsFormat newFormat);

    /// <summary>Gets the format of the texture.</summary>
    public Format DxgiFormat => ResourceFormat.ToApi();

    public GraphicsFormat DataFormat => (GraphicsFormat)DxgiFormat;

    public override ResourceHandleDX11<ID3D11Resource> Handle => _handle;

    public new DeviceDX11 Device { get; }
}
