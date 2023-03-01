namespace Molten.Graphics
{
    public sealed class ShaderCompileResult : EngineObject
    {
        public Dictionary<ShaderClassType, List<HlslElement>> ShaderGroups = new Dictionary<ShaderClassType, List<HlslElement>>();

        protected override void OnDispose() { }

        public void AddResult(ShaderClassType classType, List<HlslElement> result)
        {
            if (result.Count > 0)
            {
                List<HlslElement> group = null;
                if (!ShaderGroups.TryGetValue(classType, out group))
                {
                    group = new List<HlslElement>();
                    ShaderGroups.Add(classType, group);
                }

                group.AddRange(result);
            }
        }

        public List<HlslElement> this[ShaderClassType cType]
        {
            get => ShaderGroups[cType];
        }

        public HlslElement this[ShaderClassType cType, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslElement> group))
                    return group[index];
                else
                    return null;
            }
        }

        public HlslElement this[ShaderClassType cType, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslElement> group))
                {
                    foreach (HlslElement shader in group)
                    {
                        if (shader.Name == shaderName)
                            return shader;
                    }
                }

                return null;
            }
        }
    }
}
