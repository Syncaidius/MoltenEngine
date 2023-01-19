using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal unsafe class MaterialPassCompileResult
    {
        internal MaterialPassCompileResult(MaterialPass pass)
        {
            Pass = pass;
            Results = new Dictionary<ShaderType, ShaderClassResult>();
        }

        public ShaderClassResult this[ShaderType type]
        {
            get
            {
                if (Results.TryGetValue(type, out ShaderClassResult result))
                    return result;
                else
                    return null;
            }

            set => Results[type] = value;
        }

        internal Dictionary<ShaderType, ShaderClassResult> Results { get; } 

        internal MaterialPass Pass { get; }
    }
}
