using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class ContentProcessor
    {
        /// <summary>Invoked when a content retrieval request was called with metadata tags (prefixed with @) or when multiple objects are available which were loaded from the same file.</summary>
        /// <param name="t">The type of content being loaded.</param>
        /// <param name="metadata">An array of metadata strings that were attached to the request.</param>
        /// <param name="groupContent">A list of viable objects which match the requested filename</param>
        /// <returns></returns>
        public virtual object OnGet(Engine engine, Type t, Dictionary<string, string> metadata, List<object> groupContent) { return groupContent[0]; }

        public abstract void OnRead(Engine engine, Logger log, Type t, Stream stream, Dictionary<string, string> metadata, FileInfo file, ContentResult output);

        public abstract void OnWrite(Engine engine, Logger log, Type t, Stream stream, FileInfo file);

        /// <summary>Gets a list of accepted </summary>
        public abstract Type[] AcceptedTypes { get; protected set; }
    }
}
