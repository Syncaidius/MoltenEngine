namespace Molten.Graphics
{
    public unsafe class MaterialPassCompileResult
    {
        public MaterialPassCompileResult(MaterialPass pass)
        {
            Pass = pass;
            Results = new Dictionary<ShaderType, ShaderCodeResult>();
        }

        public ShaderCodeResult this[ShaderType type]
        {
            get
            {
                if (Results.TryGetValue(type, out ShaderCodeResult result))
                    return result;
                else
                    return null;
            }

            set => Results[type] = value;
        }

        public Dictionary<ShaderType, ShaderCodeResult> Results { get; }

        public MaterialPass Pass { get; }
    }
}
