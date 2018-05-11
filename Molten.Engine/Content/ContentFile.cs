using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten
{
    internal class ContentFile
    {
        internal Dictionary<Type, ContentSegment> Segments = new Dictionary<Type, ContentSegment>();

        internal string Path;

        internal ContentFile(string path)
        {
            Path = path;
        }
    }
}
