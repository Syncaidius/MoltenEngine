using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe abstract class HlslIncluder : PipeObject
    {
        IDxcIncludeHandler* _handler;
        IDxcIncludeHandler* _defaultHandler;

        internal HlslIncluder(HlslCompiler compiler) : base(compiler.Device)
        {
            Utils = compiler.Utils;
            Utils->CreateDefaultIncludeHandler(ref _defaultHandler);
            Utils->CreateDefaultIncludeHandler(ref _handler);

            SilkInterop.OverrideFunc(this.GetType(), "LoadSource", _handler->LpVtbl, 3);
        }

        public unsafe abstract int LoadSource(char* pFilename, IDxcBlob** ppIncludeSource);

        internal override void PipelineDispose()
        {
            ReleaseSilkPtr(ref _handler);
            ReleaseSilkPtr(ref _defaultHandler);
        }

        /// <summary>
        /// Gets the DXC utils instance bound to the current <see cref="HlslIncluder"/>.
        /// </summary>
        protected IDxcUtils* Utils { get; }

        /// <summary>
        /// Gets the default DXC include handler for the current <see cref="HlslIncluder"/>.
        /// </summary>
        protected IDxcIncludeHandler* DefaultHandler => _defaultHandler;

        public static implicit operator IDxcIncludeHandler*(HlslIncluder includer)
        {
            return includer._handler;
        }
    }
}
