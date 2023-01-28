using System.Collections.Concurrent;

namespace Molten
{
    /// <summary>
    /// Represents a file from which one or more <see cref="ContentHandle"/> instances are attached. One file may contain multiple assets or varying types.
    /// </summary>
    internal class ContentFile
    {
        internal ConcurrentDictionary<Type, ContentHandle> Handles { get; } = new ConcurrentDictionary<Type, ContentHandle>();

        internal ContentHandle GetHandle(Type contentType)
        {
            Handles.TryGetValue(contentType, out ContentHandle handle);
            return handle;
        }

        internal bool AddHandle(ContentHandle handle)
        {
            return Handles.TryAdd(handle.ContentType, handle);
        }

        internal bool Unload(ContentHandle handle)
        {
            return Handles.TryRemove(handle.ContentType, out handle);

        }
    }
}
