using Molten.Collections;

namespace Molten;

internal class ContentWatcher : IDisposable
{
    /// <summary>
    /// Gets the <see cref="ContentManager"/> which owns the current <see cref="ContentWatcher"/>.
    /// </summary>
    public ContentManager Manager { get; }

    internal ThreadedList<ContentLoadHandle> Handles { get; } = new ThreadedList<ContentLoadHandle>();

    DirectoryInfo _directory;
    ContentManager _manager;

    internal ContentWatcher(ContentManager manager, DirectoryInfo dInfo)
    {
        _manager = manager;
        _directory = dInfo;
    }

    internal void CheckForChanges()
    {
        Handles.For(0, (index, handle) =>
        {
            DateTime writeTime = File.GetLastWriteTimeUtc(handle.Info.FullName);

            if (writeTime != handle.LastWriteTime)
            {
                handle.LastWriteTime = writeTime;

                // We only want to reload if it has already finished (re)loading beforehand.
                if (handle.Status == ContentHandleStatus.Completed)
                    _manager.Workers.QueueTask(handle);
            }

            return false;
        });
    }

    public void Dispose() { }
}
