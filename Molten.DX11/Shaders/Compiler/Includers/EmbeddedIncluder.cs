using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Molten.Graphics
{
    internal unsafe class EmbeddedIncluder : HlslIncluder
    {
        Assembly _assembly;
        string _namespace;

        // TODO wrap functionality of Silk.NET.Core.Native.ID3DInclude

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="nSpace">The namespace from which the include handler will load files out of.</param>
        public EmbeddedIncluder(HlslCompiler compiler, Assembly assembly, string nSpace = "Molten.Graphics.Assets") : base(compiler)
        {
            _assembly = assembly;
            _namespace = nSpace;
        }

        public override unsafe int LoadSource(char* pFilename, IDxcBlob** ppIncludeSource)
        {
            int result = 0;
            string fn = SilkMarshal.PtrToString((nint)pFilename, NativeStringEncoding.LPWStr);

            string embeddedName = _namespace + "." + fn;
            Stream stream = EmbeddedResource.GetStream(embeddedName, _assembly);

            // Try again, but this time attempt to load the embedded include from the engine DLL.
            if (stream == null && _assembly != null)
            {
                embeddedName = "Molten.Graphics.Assets." + fn;
                stream = EmbeddedResource.GetStream(embeddedName, null);
            }

            if (stream != null)
            {
                string hlslSrc = "";

                using (StreamReader reader = new StreamReader(stream))
                {
                    hlslSrc = reader.ReadToEnd();
                }

                fixed (void* ptr = hlslSrc)
                {
                    IDxcBlobEncoding* encoding = null;
                    result = Utils->CreateBlob(ptr, (uint)hlslSrc.Length * sizeof(char), DXC.CPUtf8, ref encoding);
                }

                stream.Dispose();
            }
            else
            {
                // Log missing include
            }

            return result;
        }
    }
}
