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

        internal ContentHandle(ContentManager manager, Type contentType, ContentHandleType handleType) :
            this(manager, contentType, null, null, handleType)
        { }

        internal ContentHandle(ContentManager manager, Type contentType, IContentProcessor processor, IContentParameters parameters, ContentHandleType handleType)
        {
            Manager = manager;
            ContentType = contentType;
            Processor = processor;
            Parameters = parameters;
            HandleType = handleType;
        }

        internal bool Process()
        {
            Status = ContentHandleStatus.Processing;
            ValidateParameters();
            try
            {
                if (OnProcess())
                {
                    Manager.Log.WriteLine($"[CONTENT] [{HandleType}] {Path}: {Asset.GetType().FullName}");
                    OnComplete();
                    Status = ContentHandleStatus.Completed;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Manager.Log.Error($"[CONTENT] [{HandleType}] {Path}: {ex.Message}");
                Manager.Log.Error(ex, true);
            }

            return false;
        }

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
        public ContentHandleStatus Status { get; protected set; }

        /// <summary>
        /// The <see cref="ContentManager"/> that the currnet <see cref="ContentHandle"/> is bound to.
        /// </summary>
        public ContentManager Manager { get; }

        public Type ContentType { get; }

        internal FileInfo Info { get; }

        /// <summary>
        /// Gets the path of the asset file that the current <see cref="ContentHan"/> represents, relative to the executing directory.
        /// </summary>
        public string Path => Info.FullName;

        /// <summary>
        /// Gets a reference to the asset <see cref="object"/> to be processed.
        /// </summary>
        internal ref object Asset => ref _asset;

        internal IContentProcessor Processor { get; }

        internal IContentParameters Parameters { get; private set; }

        public ContentHandleType HandleType { get;  }
    }
}
