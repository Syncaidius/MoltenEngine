using System.Collections.Generic;

namespace Molten.Graphics
{
    public class ShaderCompileResult<S>
        where S : IShader
    {
        public Dictionary<string, List<S>> ShaderGroups = new Dictionary<string, List<S>>();

        public void AddResult(string nodeName, List<S> result)
        {
            if (result.Count > 0)
            {
                List<S> group = null;
                if (!ShaderGroups.TryGetValue(nodeName, out group))
                {
                    group = new List<S>();
                    ShaderGroups.Add(nodeName, group);
                }

                group.AddRange(result);
            }
        }

        public List<S> this[string groupName]
        {
            get => ShaderGroups[groupName];
        }

        public IShader this[string groupName, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(groupName, out List<S> group))
                    return group[index];
                else
                    return null;
            }
        }

        public IShader this[string groupName, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(groupName, out List<S> group))
                {
                    foreach (IShader shader in group)
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
