using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class HlslShader : HlslFoundation, IShader
    {
        internal ShaderConstantBuffer[] ConstBuffers = new ShaderConstantBuffer[0];
        internal ShaderResourceVariable[] Resources = new ShaderResourceVariable[0];
        internal ShaderSamplerVariable[] SamplerVariables = new ShaderSamplerVariable[0];
        internal Dictionary<string, IShaderValue> Variables = new Dictionary<string, IShaderValue>();
        
        internal IShaderResource[] DefaultResources;

        string _filename;
        internal ShaderIOStructure InputStructure;
        Dictionary<string, string> _metadata;

        public string Description { get; internal set; }

        public string Author { get; internal set; }
        
        public string Filename => _filename;

        public Dictionary<string, string> Metadata => _metadata;

        static int _nextSortKey;

        internal HlslShader(DeviceDX11 device, string filename = null) : base(device)
        {
            SortKey = Interlocked.Increment(ref _nextSortKey);
            _filename = filename ?? "";
            _metadata = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} shader -- {Name}";
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
            get
            {
                if (Variables.TryGetValue(varName, out IShaderValue varInstance))
                    return varInstance;
                else
                    return null;
            }

            set
            {
                if (Variables.TryGetValue(varName, out IShaderValue varInstance))
                    varInstance.Value = value;
            }
        }

        /// <summary>
        /// Gets the sort key assigned to the current shader.
        /// </summary>
        public int SortKey { get; }
    }
}
