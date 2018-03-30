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

        Dictionary<string, MaterialPass> _passesByName;

        public int PassCount => Passes.Length;

        internal Material(GraphicsDevice device, string filename) : base(device, filename)
        {
            _passesByName = new Dictionary<string, MaterialPass>();
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

        internal bool HasCommonConstants { get; set; }

        internal bool HasObjectConstants { get; set; }

        internal bool HasGBufferTextures { get; set; }
    }
}
