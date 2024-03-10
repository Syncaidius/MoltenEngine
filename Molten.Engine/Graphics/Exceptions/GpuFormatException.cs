namespace Molten.Graphics;

public class GpuFormatException : Exception
{
    public GpuFormatException(GpuResourceFormat format, string msg) : base(msg) { }

    public GpuFormatException(GpuResourceFormat format) : base($"The provided format ({format}) is incompatible.") { }

    public GpuFormatException(IList<GpuResourceFormat> formats) : base($"The provided formats ({string.Join(", ", formats)}) is incompatible.") { }
}
