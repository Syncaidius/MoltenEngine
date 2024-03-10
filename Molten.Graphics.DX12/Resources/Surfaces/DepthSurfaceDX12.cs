using Silk.NET.Direct3D12;
using System.Drawing;

namespace Molten.Graphics.DX12;

public class DepthSurfaceDX12 : Texture2DDX12, IDepthStencilSurface
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="device"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="flags"></param>
    /// <param name="format"></param>
    /// <param name="mipCount"></param>
    /// <param name="arraySize"></param>
    /// <param name="aaLevel"></param>
    /// <param name="msaa"></param>
    /// <param name="name"></param>
    /// <param name="protectedSession"></param>
    internal DepthSurfaceDX12(DeviceDX12 device,
        uint width,
        uint height,
        GpuResourceFlags flags = GpuResourceFlags.GpuWrite,
        DepthFormat format = DepthFormat.R24G8,
        uint mipCount = 1,
        uint arraySize = 1,
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality msaa = MSAAQuality.Default,
        string name = "surface", 
        ProtectedSessionDX12 protectedSession = null) :
        base(device, width, height, flags, format.ToGraphicsFormat(), mipCount, arraySize, aaLevel, msaa, name, protectedSession)
    {
        DepthFormat = format;

        name ??= "surface";
        Name = $"depth_{name}";

        UpdateViewport();
    }

    /*protected override ClearValue GetClearValue()
    {
        // TODO Correctly implement to avoid: D3D12_CLEAR_VALUE::Format cannot be a typeless format. A fully qualified format must be supplied. Format = R24G8_TYPELESS.
        
        ClearValue val = new();
        val.Format = DxgiFormat;
        val.DepthStencil = new DepthStencilValue(1, 0);
        return val;
    }*/

    private DsvFlags GetReadOnlyFlags()
    {
        switch (DepthFormat)
        {
            default:
            case DepthFormat.R24G8:
                return DsvFlags.ReadOnlyDepth | DsvFlags.ReadOnlyStencil;
            case DepthFormat.R32:
                return DsvFlags.ReadOnlyDepth;
        }
    }

    protected unsafe override ID3D12Resource1* OnCreateTexture()
    {
        UpdateViewport();
        return base.OnCreateTexture();
    }

    private void UpdateViewport()
    {
        Viewport = new ViewportF(0, 0, Desc.Width, Desc.Height);
    }

    public void Clear(GpuPriority priority, DepthClearFlags flags, float depthValue = 1, byte stencilValue = 0)
    {
        if (priority == GpuPriority.Immediate)
        {
            Apply(Device.Queue);
            Device.Queue.Clear(this, depthValue, stencilValue, flags);
        }
        else
        {
            DepthClearTaskDX12 task = Device.Tasks.Get<DepthClearTaskDX12>();
            task.DepthValue = depthValue;
            task.StencilValue = stencilValue;
            task.ClearFlags = flags;
            Device.Tasks.Push(priority, this, task);
        }
    }

    protected override unsafe ResourceHandleDX12 OnCreateHandle(ID3D12Resource1* ptr)
    {
        DepthStencilViewDesc desc = new()
        {
            Format = DepthFormat.ToDepthViewFormat().ToApi(),
            Flags = DsvFlags.None
        };

        if (MultiSampleLevel >= AntiAliasLevel.X2)
        {
            desc.ViewDimension = DsvDimension.Texture2Dmsarray;;
            desc.Texture2DMSArray = new Tex2DmsArrayDsv()
            {
                ArraySize = ArraySize,
                FirstArraySlice = 0,
            };
        }
        else
        {
            desc.ViewDimension = DsvDimension.Texture2Darray;
            desc.Texture2DArray = new Tex2DArrayDsv()
            {
                ArraySize = ArraySize,
                FirstArraySlice = 0,
                MipSlice = 0,
            };
        }

        DSHandleDX12 dsv = new (this, ptr);
        dsv.DSV.Initialize(ref desc);

        desc.Flags = GetReadOnlyFlags();
        dsv.ReadOnlyDSV.Initialize(ref desc);

        return dsv;
    }

    protected override void SetSRVDescription(ref ShaderResourceViewDesc desc)
    {
        base.SetSRVDescription(ref desc);
        desc.Format = DepthFormat.ToSRVFormat().ToApi();
    }

    public DepthFormat DepthFormat { get; }

    public ViewportF Viewport { get; private set; }
}
