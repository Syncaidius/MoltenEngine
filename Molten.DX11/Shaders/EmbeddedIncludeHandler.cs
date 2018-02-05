﻿using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Molten.IO;

namespace Molten.Graphics.Shaders
{
    internal class EmbeddedIncludeHandler : Include
    {
        Stream _stream;
        IDisposable _disposable;
        Assembly _assembly;
        string _namespace;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="nSpace">The namespace from which the include handler will load files out of.</param>
        public EmbeddedIncludeHandler(Assembly assembly, string nSpace)
        {
            _assembly = assembly;
            _namespace = nSpace;
        }

        public void Close(System.IO.Stream stream)
        {
            _stream.Close();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            string embeddedName = _namespace + "." + fileName;
            _stream = EmbeddedResourceReader.GetStream(embeddedName, _assembly);

            // Try again, but this time attempt to load the embedded include from the engine DLL.
            if (_stream == null && _assembly != null)
            {
                embeddedName = "StoneBolt.Assets." + fileName;
                _stream = EmbeddedResourceReader.GetStream(embeddedName, null);
            }

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

            _assembly = null;
        }
    }
}
