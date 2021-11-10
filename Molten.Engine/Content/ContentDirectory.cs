using System;
using System.Collections.Generic;
using System.IO;

namespace Molten
{
    internal delegate void ContentDirectoryHandler(ContentDirectory directory, FileSystemEventArgs e);

    internal class ContentDirectory : IDisposable
    {
        internal string Directory;

        internal List<ContentFile> Files = new List<ContentFile>();

        internal event ContentDirectoryHandler OnChanged;

        FileSystemWatcher _watcher;

        internal ContentDirectory(string directory)
        {
            Directory = directory;
            _watcher = new FileSystemWatcher(directory);
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
            _watcher.EnableRaisingEvents = true;
            _watcher.Changed += Watcher_Changed;
            _watcher.Created += Watcher_Changed;
            _watcher.Renamed += Watcher_Changed;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            OnChanged?.Invoke(this, e);
        }

        internal void AddFile(ContentFile file)
        {
            Files.Add(file);
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}
