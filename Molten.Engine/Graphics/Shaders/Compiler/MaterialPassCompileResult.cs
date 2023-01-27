namespace Molten.Graphics
{
    public unsafe class MaterialPassCompileResult
    {
        public MaterialPassCompileResult(MaterialPass pass)
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

        public Dictionary<ShaderType, ShaderClassResult> Results { get; }

        public MaterialPass Pass { get; }
    }
}
