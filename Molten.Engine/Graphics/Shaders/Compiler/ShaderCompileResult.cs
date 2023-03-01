namespace Molten.Graphics
{
    public sealed class ShaderCompileResult : EngineObject
    {
        public Dictionary<ShaderClassType, List<HlslFoundation>> ShaderGroups = new Dictionary<ShaderClassType, List<HlslFoundation>>();

        protected override void OnDispose() { }

        public void AddResult(ShaderClassType classType, List<HlslFoundation> result)
        {
            if (result.Count > 0)
            {
                List<HlslFoundation> group = null;
                if (!ShaderGroups.TryGetValue(classType, out group))
                {
                    group = new List<HlslFoundation>();
                    ShaderGroups.Add(classType, group);
                }

                group.AddRange(result);
            }
        }

        public List<HlslFoundation> this[ShaderClassType cType]
        {
            get => ShaderGroups[cType];
        }

        public HlslFoundation this[ShaderClassType cType, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslFoundation> group))
                    return group[index];
                else
                    return null;
            }
        }

        public HlslFoundation this[ShaderClassType cType, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<HlslFoundation> group))
                {
                    foreach (HlslFoundation shader in group)
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
