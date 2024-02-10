using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Molten.Graphics.Shaders;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ShaderFormatLayout : IEquatable<ShaderFormatLayout>
{
    [FieldOffset(0)]
    public ulong FormatMask;

    [FieldOffset(0)]
    public fixed byte RawFormats[8];

    [FieldOffset(0)]
    public GraphicsFormat Target0;

    [FieldOffset(1)]
    public GraphicsFormat Target1;

    [FieldOffset(2)]
    public GraphicsFormat Target2;

    [FieldOffset(3)]
    public GraphicsFormat Target3;

    [FieldOffset(4)]
    public GraphicsFormat Target4;

    [FieldOffset(5)]
    public GraphicsFormat Target5;

    [FieldOffset(6)]
    public GraphicsFormat Target6;

    [FieldOffset(7)]
    public GraphicsFormat Target7;

    [FieldOffset(8)]
    public DepthFormat Depth;

    public bool Equals(ShaderFormatLayout other)
    {
        return FormatMask == other.FormatMask && Depth == other.Depth;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if(obj is ShaderFormatLayout other)
            return Equals(other);
        else
            return false;
    }

    /// <summary>
    /// Checks whether the provided surfaces match the expected format(s) of the current format layout.
    /// </summary>
    /// <param name="surfaces"></param>
    /// <returns></returns>
    public unsafe int ValidateFormats(params IRenderSurface[] surfaces)
    {
        for (int i = 0; i < surfaces.Length; i++)
        {
            if (surfaces[i] == null)
                continue;

            if (surfaces[i].ResourceFormat != (GraphicsFormat)RawFormats[i])
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Outputs the difference between the expected format layout and the given surfaces.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="surfaces"></param>
    public unsafe void LogDifference(Logger log, params IRenderSurface[] surfaces)
    {
        if(surfaces.Length > 8)
            log.Error("The number of surfaces provided exceeds the maximum of 8.");

        log.Error("Expected:");
        for(int i = 0; i < 8; i++)
            log.Error($"\t{i}: - {(GraphicsFormat)RawFormats[i]}");

        log.Error("Given:");
        for(int i = 0; i < surfaces.Length; i++)
            log.Error($"\t{i}: - {surfaces[i]?.ResourceFormat}");
    }
}
