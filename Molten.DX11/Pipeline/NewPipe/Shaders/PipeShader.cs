using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class PipeShader : PipeBindable, IShader
    {
        public PipeShader(PipeStageType canBindTo, PipeBindTypeFlags bindTypeFlags) : 
            base(canBindTo, bindTypeFlags)
        {
        }

        public IShaderValue this[string key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public string Description => throw new NotImplementedException();

        public string Author => throw new NotImplementedException();

        public string Filename => throw new NotImplementedException();

        public Dictionary<string, string> Metadata => throw new NotImplementedException();

        public int SortKey => throw new NotImplementedException();

        protected override void OnBind(PipeBindSlot slot, PipeDX11 pipe)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            throw new NotImplementedException();
        }
    }
}
