using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal unsafe class MaterialPassCompileResult
    {
        internal MaterialPassCompileResult(MaterialPass pass)
        {
            Pass = pass;
            Results = new Dictionary<ShaderType, FxcCompileResult>();
        }

        public FxcCompileResult this[ShaderType type]
        {
            get
            {
                if (Results.TryGetValue(type, out FxcCompileResult result))
                    return result;
                else
                    return null;
            }

            set => Results[type] = value;
        }

        internal Dictionary<ShaderType, FxcCompileResult> Results { get; } 

        internal MaterialPass Pass { get; }
    }
}
