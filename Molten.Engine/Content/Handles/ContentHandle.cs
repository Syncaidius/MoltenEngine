using Molten.Threading;

namespace Molten
{
    /// <summary>
    /// A handle for a content asset.
    /// </summary>
    public abstract class ContentHandle : WorkerTask, IDisposable
    {
        object _asset;

        internal ContentHandle(ContentManager manager, string path, Type contentType, ContentHandleType handleType) :
            this(manager, path, contentType, null, null, handleType)
        { }

        internal ContentHandle(ContentManager manager, string path, Type contentType, IContentProcessor processor, ContentParameters parameters, ContentHandleType handleType)
        {
            Info = new FileInfo(path);
            Manager = manager;
            ContentType = contentType;
            Processor = processor;
            Parameters = parameters;
            HandleType = handleType;
            RelativePath = Path.GetRelativePath(Manager.ExecutablePath.Directory.FullName, Info.Directory.FullName);
            RelativePath = $"{RelativePath}\\{Info.Name}";
        }

        public override string ToString()
        {
            return $"{GetType().Name}: {RelativePath} - {ContentType.Name}";
        }

        /// <summary>
        /// Dispatches the current <see cref="ContentHandle"/> to it's parent <see cref="ContentManager"/> for processing.
        /// </summary>
        public void Dispatch()
        {
            Status = ContentHandleStatus.Processing;
            Manager.Workers.QueueTask(this);
        }

        protected override sealed bool OnRun()
        {
            if (Status == ContentHandleStatus.Failed)
                return false;

            Status = ContentHandleStatus.Processing;

            try
            {
                Status = OnProcess();
                switch (Status)
                {
                    case ContentHandleStatus.Completed:
                        if (Asset == null)
                        {
                            LogError($"[{Thread.CurrentThread.Name}] Completed load, but no asset was provided");
                            return true;
                        }

                        LogMessage($"Loaded {Asset.GetType().FullName}");
                        return true;

                    case ContentHandleStatus.AwaitingParts:
                        if(this is ContentLoadHandle loadHandle)
                            LogMessage($"Awaiting {loadHandle.PartInfo.Handles.Length} parts for multi-part {ContentType.FullName}");
                        else
                            LogMessage($"Awaiting parts {ContentType.FullName}");
                        return true;

                    case ContentHandleStatus.Processing:
                        LogMessage($"Still processing {ContentType.FullName}");
                        return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            return false;
        }

        public void LogMessage(string msg)
        {
            Manager.Log.WriteLine($"[Content][{HandleType}] {RelativePath}: {msg}");
        }

        public void LogError(string msg)
        {
            Manager.Log.Error($"[Content][{HandleType}] {RelativePath}: {msg}");
        }

        public void LogWarning(string msg)
        {
            Manager.Log.Warning($"[Content][{HandleType}] {RelativePath}: {msg}");
        }

        public void LogError(Exception ex)
        {
            Manager.Log.Error($"[Content][{HandleType}] {RelativePath}: {ex.Message}");
            Manager.Log.Error(ex);
        }

        public T Get<T>()
        {
            return Asset != null ? (T)Asset : default(T);
        }

        protected override void OnFree() { }

        protected abstract ContentHandleStatus OnProcess();

        /// <summary>
        /// The status of the current <see cref="ContentHandle"/>.
        /// </summary>
        public ContentHandleStatus Status { get; internal set; }

        /// <summary>
        /// The <see cref="ContentManager"/> that the currnet <see cref="ContentHandle"/> is bound to.
        /// </summary>
        public ContentManager Manager { get; }

        public Type ContentType { get; }

        public FileInfo Info { get; internal set; }

        /// <summary>
        /// The handle's relative path to the current application executable.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets a reference to the asset <see cref="object"/> to be processed.
        /// </summary>
        internal ref object Asset => ref _asset;

        internal IContentProcessor Processor { get; }

        internal ContentParameters Parameters { get; set; }

        public ContentHandleType HandleType { get;  }
    }
}
