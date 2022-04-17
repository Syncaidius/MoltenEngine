using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal unsafe class HlslInputBindDescription : IDisposable
    {
        internal ShaderInputBindDesc* Ptr;

        internal readonly string Name;

        internal HlslInputBindDescription(ID3D11ShaderReflection* reflection, uint rIndex)
        {
            Ptr = EngineUtil.Alloc<ShaderInputBindDesc>();
            reflection->GetResourceBindingDesc(rIndex, Ptr);
            Name = SilkMarshal.PtrToString((nint)Ptr->Name);
        }

        ~HlslInputBindDescription()
        {
            Dispose();
        }

        public void Dispose()
        {
            if(Ptr != null)
                EngineUtil.Free(ref Ptr);
        }
    }
}
