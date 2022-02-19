using System.Collections.Generic;

namespace Molten.Graphics
{
    public sealed class ShaderCompileResult : EngineObject
    {
        public Dictionary<ShaderClassType, List<IShaderElement>> ShaderGroups = new Dictionary<ShaderClassType, List<IShaderElement>>();

        protected override void OnDispose() { }

        public void AddResult(ShaderClassType classType, List<IShaderElement> result)
        {
            if (result.Count > 0)
            {
                List<IShaderElement> group = null;
                if (!ShaderGroups.TryGetValue(classType, out group))
                {
                    group = new List<IShaderElement>();
                    ShaderGroups.Add(classType, group);
                }

                group.AddRange(result);
            }
        }

        public List<IShaderElement> this[ShaderClassType cType]
        {
            get => ShaderGroups[cType];
        }

        public IShaderElement this[ShaderClassType cType, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<IShaderElement> group))
                    return group[index];
                else
                    return null;
            }
        }

        public IShaderElement this[ShaderClassType cType, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<IShaderElement> group))
                {
                    foreach (IShaderElement shader in group)
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
