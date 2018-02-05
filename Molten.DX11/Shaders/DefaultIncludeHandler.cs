using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Shaders
{
    internal class DefaultIncludeHandler : Include
    {
        Stream _stream;
        IDisposable _disposable;

        public void Close(System.IO.Stream stream)
        {
            _stream.Close();
        }

        public Stream Open(IncludeType type, string filename, Stream parentStream)
        {
            _stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            return _stream;
        }

        public IDisposable Shadow
        {
            get
            {
                return _disposable;
            }
            set
            {
                _disposable = value;
            }
        }

        public void Dispose()
        {
            if(_stream != null)
                _stream.Dispose();
        }
    }
}
