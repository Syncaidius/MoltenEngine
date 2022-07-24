using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Molten.Threading;

namespace Molten
{
    /// <summary>
    /// A handle for a content asset.
    /// </summary>
    public abstract class ContentHandle : WorkerTask, IDisposable
    {
        object _asset;
        static FileInfo _exePath;

        static ContentHandle()
        {
            string exePath = Assembly.GetEntryAssembly().Location;
            _exePath = new FileInfo(exePath);
        }

        internal ContentHandle(ContentManager manager, string path, Type contentType, ContentHandleType handleType) :
            this(manager, path, contentType, null, null, handleType)
        { }

        internal ContentHandle(ContentManager manager, string path, Type contentType, IContentProcessor processor, IContentParameters parameters, ContentHandleType handleType)
        {
            Info = new FileInfo(path);
            Manager = manager;
            ContentType = contentType;
            Processor = processor;
            Parameters = parameters;
            HandleType = handleType;
            RelativePath = System.IO.Path.GetRelativePath(_exePath.Directory.FullName, Info.Directory.FullName);
            RelativePath = $"{RelativePath}\\{Info.Name}";
        }

        /// <summary>
        /// Dispatches the current <see cref="ContentHandle"/> to it's parent <see cref="ContentManager"/> for processing.
        /// </summary>
        public void Dispatch()
        {
            Manager.Workers.QueueTask(this);
        }

        protected override sealed bool OnRun()
        {
            Status = ContentHandleStatus.Processing;
            ValidateParameters();
            try
            {
                if (OnProcess())
                {
                    Manager.Log.WriteLine($"[CONTENT] [{HandleType}] {RelativePath}: {Asset.GetType().FullName}");
                    OnComplete();
                    Status = ContentHandleStatus.Completed;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Manager.Log.Error($"[CONTENT] [{HandleType}] {RelativePath}: {ex.Message}");
                Manager.Log.Error(ex, true);
            }

            return false;
        }

        protected override void OnFree() { }

        private void ValidateParameters()
        {
            if (Processor == null)
                return;

            Type pExpectedType = Processor.GetParameterType();

            if (Parameters != null)
            {
                Type pType = Parameters.GetType();

                if (!pExpectedType.IsAssignableFrom(pType))
                    Manager.Log.Warning($"[CONTENT] {Info}: Invalid parameter type provided. Expected '{pExpectedType.Name}' but received '{pType.Name}'. Using defaults instead.");
                else
                    return;
            }

            Parameters = Activator.CreateInstance(pExpectedType) as IContentParameters;
        }

        protected abstract void OnComplete();

        protected abstract bool OnProcess();

        /// <summary>
        /// The status of the current <see cref="ContentHandle"/>.
        /// </summary>
        public ContentHandleStatus Status { get; internal set; }

        /// <summary>
        /// The <see cref="ContentManager"/> that the currnet <see cref="ContentHandle"/> is bound to.
        /// </summary>
        public ContentManager Manager { get; }

        public Type ContentType { get; }

        public FileInfo Info { get; }

        /// <summary>
        /// The handle's relative path to the current application executable.
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets a reference to the asset <see cref="object"/> to be processed.
        /// </summary>
        internal ref object Asset => ref _asset;

        internal IContentProcessor Processor { get; }

        internal IContentParameters Parameters { get; private set; }

        public ContentHandleType HandleType { get;  }
    }
}
