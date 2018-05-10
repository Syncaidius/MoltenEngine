using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class ContentDirectory : IDisposable
    {
        internal string Directory;

        internal List<ContentFile> Files = new List<ContentFile>();

        internal FileSystemWatcher Watcher;

        internal ContentDirectory(string directory)
        {
            Directory = directory;
            Watcher = new FileSystemWatcher(directory);
        }

        internal void AddFile(ContentFile file)
        {
            Files.Add(file);
        }

        public void Dispose()
        {
            Watcher.Dispose();
        }
    }
}
