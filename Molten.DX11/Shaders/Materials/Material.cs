using SharpDX.Direct3D;
using Molten.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Material : HlslShader, IMaterial
    {
        internal MaterialPass[] Passes = new MaterialPass[0];
        internal byte[] InputStructureByteCode;
        internal IShaderResource[] DefaultResources;

        Dictionary<string, MaterialPass> _passesByName;

        public int PassCount => Passes.Length;

        internal Material(GraphicsDevice device, string filename) : base(device, filename)
        {
            _passesByName = new Dictionary<string, MaterialPass>();
            DefaultResources = new IShaderResource[0];
        }

        internal void AddPass(MaterialPass pass)
        {
            int id = 0;
            if (Passes == null)
            {
                Passes = new MaterialPass[1];
            }
            else
            {
                id = Passes.Length;
                Array.Resize(ref Passes, Passes.Length + 1);
            }

            Passes[id] = pass;
        }

        public IMaterialPass GetPass(int index)
        {
            return Passes[index];
        }

        public IMaterialPass GetPass(string name)
        {
            return _passesByName[name];
        }

        public void SetDefaultResource(IShaderResource resource, int slot)
        {
            if(slot >= Device.Features.MaxInputResourceSlots)
                throw new IndexOutOfRangeException("The maximum slot number must be less than the maximum supported by the graphics device.");

            if (slot >= DefaultResources.Length)
                Array.Resize(ref DefaultResources, slot + 1);

            DefaultResources[slot] = resource;
        }

        public IShaderResource GetDefaultResource(int slot)
        {
            if (slot >= DefaultResources.Length)
                return null;
            else
                return DefaultResources[slot];
        }

        internal bool HasCommonConstants { get; set; }

        internal bool HasObjectConstants { get; set; }

    }
}
