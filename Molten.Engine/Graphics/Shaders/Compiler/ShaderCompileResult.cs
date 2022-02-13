using System.Collections.Generic;

namespace Molten.Graphics
{
    public class ShaderCompileResult<S> : EngineObject
        where S : IShaderElement
    {
        public Dictionary<ShaderClassType, List<S>> ShaderGroups = new Dictionary<ShaderClassType, List<S>>();

        protected override void OnDispose() { }

        public void AddResult(ShaderClassType classType, List<S> result)
        {
            if (result.Count > 0)
            {
                List<S> group = null;
                if (!ShaderGroups.TryGetValue(classType, out group))
                {
                    group = new List<S>();
                    ShaderGroups.Add(classType, group);
                }

                group.AddRange(result);
            }
        }

        public List<S> this[ShaderClassType cType]
        {
            get => ShaderGroups[cType];
        }

        public IShaderElement this[ShaderClassType cType, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<S> group))
                    return group[index];
                else
                    return null;
            }
        }

        public IShaderElement this[ShaderClassType cType, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(cType, out List<S> group))
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
