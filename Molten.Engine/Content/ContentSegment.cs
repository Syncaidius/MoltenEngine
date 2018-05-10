using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class ContentSegment
    {
        internal ThreadedList<object> Objects;

        internal string FilePath;

        internal Type ObjectType;

        internal ContentSegment(Type objType, string filePath)
        {
            Objects = new ThreadedList<object>();
            FilePath = filePath;
            ObjectType = objType;
        }
    }
}
