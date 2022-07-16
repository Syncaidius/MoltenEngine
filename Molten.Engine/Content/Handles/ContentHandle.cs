using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>
    /// A handle for a content asset.
    /// </summary>
    public abstract class ContentHandle
    {
        object _asset;

        internal ContentHandle(ContentManager manager, Type contentType)
        {
            Manager = manager;
            ContentType = contentType;
        }

        internal void Complete()
        {
            OnComplete();
            Status = ContentHandleStatus.Completed;
        }

        internal bool Process()
        {
            Status = ContentHandleStatus.Processing;
            return OnProcess();
        }

        protected abstract void OnComplete();

        protected abstract bool OnProcess();

        /// <summary>
        /// The status of the current <see cref="ContentHandle"/>.
        /// </summary>
        public ContentHandleStatus Status { get; protected set; }

        /// <summary>
        /// The <see cref="ContentManager"/> that the currnet <see cref="ContentHandle"/> is bound to.
        /// </summary>
        public ContentManager Manager { get; }

        public Type ContentType { get; }

        internal FileInfo Info { get; }

        /// <summary>
        /// Gets the path of the asset file that the current <see cref="ContentHan"/> represents.
        /// </summary>
        public string Path => Info.Name;

        /// <summary>
        /// Gets a reference to the asset <see cref="object"/> to be processed.
        /// </summary>
        internal ref object Asset => ref _asset;
    }
}
