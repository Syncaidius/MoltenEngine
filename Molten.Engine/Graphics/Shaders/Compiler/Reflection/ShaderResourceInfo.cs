namespace Molten.Graphics
{
    /// <summary>
    /// Reflection information about a shader resource binding.
    /// </summary>
    public class ShaderResourceInfo
    {
        public string Name;

        public uint BindPoint;

        public uint BindCount;

        public ShaderInputType Type;

        public ShaderReturnType ResourceReturnType;

        public ShaderResourceDimension Dimension;

        public uint NumSamples;

        public ShaderInputFlags Flags;

        public bool HasInputFlags(ShaderInputFlags flags)
        {
            return (Flags & flags) == flags;
        }
    }
}
