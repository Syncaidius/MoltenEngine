using Silk.NET.DXGI;

namespace Molten.Graphics.Dxgi;

public class DisplayModeDXGI : IDisplayMode
{
    public ModeDesc1 Description;

    public DisplayModeDXGI(ref ModeDesc1 desc)
    {
        Description = desc;
    }

    public uint Width
    {
        get => Description.Width;
        set => Description.Width = value;
    }

    public uint Height
    {
        get => Description.Height;
        set => Description.Height = value;
    }

    public uint RefreshRate => Description.RefreshRate.Numerator;

    public GpuResourceFormat Format => Description.Format.FromApi();

    public DisplayScalingMode Scaling => Description.Scaling.FromApi();

    public ModeScanlineOrder ScanLineOrdering => Description.ScanlineOrdering;

    public bool StereoPresent => Description.Stereo > 0;
}
