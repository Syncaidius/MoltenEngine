using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class HlslIncludeHandler : Include
    {
        Stream _stream;
        IDisposable _disposable;
        string _directory;

        internal HlslIncludeHandler(string directory)
        {
            _directory = directory ?? "";
        }

        public void Close(Stream stream)
        {
            _stream.Close();
        }

        public Stream Open(IncludeType type, string filename, Stream parentStream)
        {
            _stream = new FileStream(Path.Combine(_directory, filename), FileMode.Open, FileAccess.Read, FileShare.Read);
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
