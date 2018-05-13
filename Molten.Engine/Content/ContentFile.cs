using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten
{
    internal class ContentFile
    {
        internal ThreadedDictionary<Type, ContentSegment> Segments = new ThreadedDictionary<Type, ContentSegment>();

        internal string Path;

        internal ContentRequestType Type;

        internal ContentFile(string path, ContentRequestType type)
        {
            Path = path;
            Type = type;
        }
    }
}
