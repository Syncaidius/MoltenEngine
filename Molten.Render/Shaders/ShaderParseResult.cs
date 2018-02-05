using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderParseResult
    {
        public Dictionary<string, List<IShader>> ShaderGroups = new Dictionary<string, List<IShader>>();

        public List<string> Errors = new List<string>();

        public List<string> Warnings = new List<string>();

        public void AddResult(string nodeName, ShaderCompileResult result)
        {
            if (result.Shaders.Count > 0)
            {
                List<IShader> group = null;
                if (!ShaderGroups.TryGetValue(nodeName, out group))
                {
                    group = new List<IShader>();
                    ShaderGroups.Add(nodeName, group);
                }

                group.AddRange(result.Shaders);
            }

            Errors.AddRange(result.Errors);
            Warnings.AddRange(result.Warnings);
        }

        public List<IShader> this[string groupName]
        {
            get => ShaderGroups[groupName];
        }

        public IShader this[string groupName, int index]
        {
            get
            {
                if (ShaderGroups.TryGetValue(groupName, out List<IShader> group))
                    return group[index];
                else
                    return null;
            }
        }

        public IShader this[string groupName, string shaderName]
        {
            get
            {
                if (ShaderGroups.TryGetValue(groupName, out List<IShader> group))
                {
                    foreach(IShader shader in group)
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
