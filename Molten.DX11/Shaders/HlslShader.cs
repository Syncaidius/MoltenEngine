using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class HlslShader : HlslFoundation, IShader
    {
        internal ShaderConstantBuffer[] ConstBuffers = new ShaderConstantBuffer[0];
        internal ShaderResourceVariable[] Resources = new ShaderResourceVariable[0];
        internal ShaderSamplerVariable[] SamplerVariables = new ShaderSamplerVariable[0];
        internal Dictionary<string, PipelineShaderObject> ResourcePool = new Dictionary<string, PipelineShaderObject>();
        internal Dictionary<string, IShaderValue> Variables = new Dictionary<string, IShaderValue>();
        
        internal IShaderResource[] DefaultResources;

        GraphicsDevice _device;
        string _filename;
        internal ShaderIOStructure InputStructure;
        Dictionary<string, string> _metadata;

        public string Description { get; internal set; }

        public string Author { get; internal set; }
        
        public string Filename => _filename;

        internal GraphicsDevice Device => _device;

        public Dictionary<string, string> Metadata => _metadata;

        internal HlslShader(GraphicsDevice device, string filename = null)
        {
            _filename = filename ?? "";
            _device = device;
            _metadata = new Dictionary<string, string>();
        }

        public void SetDefaultResource(IShaderResource resource, int slot)
        {
            if (slot >= DefaultResources.Length)
                throw new IndexOutOfRangeException($"The highest slot number must be less-or-equal to the highest slot number used in the shader source code ({DefaultResources.Length}).");

            Array.Resize(ref DefaultResources, slot + 1);
            DefaultResources[slot] = resource;
        }

        public IShaderResource GetDefaultResource(int slot)
        {
            if (slot >= DefaultResources.Length)
                throw new IndexOutOfRangeException($"The highest slot number must be less-or-equal to the highest slot number used in the shader source code ({DefaultResources.Length}).");
            else
                return DefaultResources[slot];
        }

        /// <summary>Gets or sets the value of a material parameter.</summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="varName">The varialbe name.</param>
        /// <returns></returns>
        public IShaderValue this[string varName]
        {
            set
            {
                //if the shader is invalid, skip applying data
                //if (!_isValid)
                //    return;

                IShaderValue varInstance = null;

                if (Variables.TryGetValue(varName, out varInstance))
                    varInstance.Value = value;
            }

            get
            {
                //if the shader is invalid, skip applying data
                //if (!_isValid)
                //    return null;

                IShaderValue varInstance = null;

                if (Variables.TryGetValue(varName, out varInstance))
                    return varInstance;
                else
                    return null;
            }
        }
    }
}
