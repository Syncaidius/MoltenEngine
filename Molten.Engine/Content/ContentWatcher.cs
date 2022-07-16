using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;

namespace Molten
{
    internal class ContentWatcher : IDisposable
    {
        FileSystemWatcher _watcher;
        DirectoryInfo _directory;

        internal ContentWatcher(ContentManager manager, DirectoryInfo dInfo)
        {
            _directory = dInfo;
            _watcher = new FileSystemWatcher(dInfo.ToString());
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
            _watcher.Changed += _watcher_Changed;
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Handles.For(0, 1, (index, handle) =>
            {
                handle.Process();
                return false;
            });
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }

        /// <summary>
        /// Gets the <see cref="ContentManager"/> which owns the current <see cref="ContentWatcher"/>.
        /// </summary>
        public ContentManager Manager { get; }

        internal ThreadedList<ContentHandle> Handles { get; } = new ThreadedList<ContentHandle>();
    }
}
