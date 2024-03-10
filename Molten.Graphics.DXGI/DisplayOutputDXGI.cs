using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace Molten.Graphics.Dxgi;

public unsafe class DisplayOutputDXGI : EngineObject, IDisplayOutput
{
    internal IDXGIOutput6* Native;
    OutputDesc1* _desc;
    DeviceDXGI _device;

    internal DisplayOutputDXGI(DeviceDXGI device, IDXGIOutput6* output)
    {
        _device = device;
        Native = output;

        _desc = EngineUtil.Alloc<OutputDesc1>();
        Native->GetDesc1(_desc);

        Name = SilkMarshal.PtrToString((nint)_desc->DeviceName, NativeStringEncoding.LPWStr);
        Name = Name.Replace("\0", string.Empty);
    }

    protected override void OnDispose(bool immediate)
    {
        EngineUtil.Free(ref _desc);
        NativeUtil.ReleasePtr(ref Native);
    }

    public IReadOnlyList<IDisplayMode> GetModes(GpuResourceFormat format)
    {
        uint flags = DXGI.EnumModesInterlaced | DXGI.EnumModesScaling | DXGI.EnumModesStereo;

        uint modeCount = 0;
        Native->GetDisplayModeList1(format.ToApi(), flags, &modeCount, null);
        ModeDesc1* mDescs = EngineUtil.AllocArray<ModeDesc1>(modeCount);
        List<DisplayModeDXGI> modes = new List<DisplayModeDXGI>((int)modeCount);
        Native->GetDisplayModeList1(format.ToApi(), flags, &modeCount, mDescs);

        // Build a list of all valid display modes
        for (int i = 0; i < modeCount; i++)
            modes.Add(new DisplayModeDXGI(ref mDescs[i]));

        return modes;
    }

    /// <summary>Gets the resolution/size of the dekstop bound to the output, if any.</summary>
    public Rectangle DesktopBounds => _desc->DesktopCoordinates.FromApi();

    /// <summary>Gets whether or not the output is bound to a desktop.</summary>
    public bool IsBoundToDesktop => _desc->AttachedToDesktop > 0;

    /// <summary>Gets the orientation of the current <see cref="IDisplayOutput" />.</summary>
    public DisplayOrientation Orientation => (DisplayOrientation)_desc->Rotation;

    /// <summary>Gets the adapter that the display device is connected to.</summary>
    public GpuDevice Device => _device;
}
