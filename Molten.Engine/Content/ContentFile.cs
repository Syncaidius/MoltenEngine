using Molten.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Molten
{
    internal class ContentFile
    {
        FileInfo _file;

        internal ThreadedDictionary<Type, ContentSegment> Segments = new ThreadedDictionary<Type, ContentSegment>();

        internal FileInfo File
        {
            get => _file;
            set
            {
                _file = value;
                Path = _file?.ToString() ?? "";
            }
        }

        internal string Path { get; private set; }

        internal ContentRequestType Type;

        /// <summary>
        /// The content processor which loaded the file, if any.
        /// </summary>
        internal ContentProcessor OriginalProcessor;

        /// <summary>
        /// The content type that was requested when loading the file.
        /// </summary>
        internal Type OriginalContentType;
    }
}
