using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class ContentRequestElement : IPoolable
    {
        public ContentRequestType Type { get; internal set; }

        public Type ContentType { get; internal set; }

        public FileInfo Info { get; internal set; }

        public string FilePathString { get; internal set; }

        public Dictionary<string, string> Metadata { get; internal set; } = new Dictionary<string, string>();

        public ContentResult Result { get; internal set; } = new ContentResult();

        public void Clear()
        {
            Result.Objects.Clear();
            Metadata.Clear();
            Info = null;
            ContentType = null;
            FilePathString = null;
        }
    }
}
