using System.Runtime.CompilerServices;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    internal static class DX12Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlag(this DescriptorHeapFlags flags, DescriptorHeapFlags flag)
        {
            return (flags & flag) == flag;
        }
    }
}
