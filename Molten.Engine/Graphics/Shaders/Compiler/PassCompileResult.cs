namespace Molten.Graphics
{
    public unsafe class PassCompileResult
    {
        public PassCompileResult(HlslPass pass)
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

        public HlslPass Pass { get; }
    }
}
