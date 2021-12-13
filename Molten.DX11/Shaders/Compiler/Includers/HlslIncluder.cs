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

        internal HlslIncluder(DeviceDX11 device, IDxcUtils* utils) : base(device)
        {
            Utils = utils;
            Utils->CreateDefaultIncludeHandler(ref _defaultHandler);

            SilkInterop.OverrideFunc(this.GetType(), "LoadSource", _handler->LpVtbl, 3);
        }

        protected abstract IDxcIncludeHandler* CreateNativeHandler();

        public unsafe virtual int LoadSource(char* pFilename, IDxcBlob** ppIncludeSource)
        {
            return Utils->CreateDefaultIncludeHandler(ref _defaultHandler);
        }

        internal override void PipelineDispose()
        {
            _handler->Release();
        }

        protected IDxcUtils* Utils { get; }
    }
}
