using System.Security.Cryptography;

namespace Molten;

public delegate void ContentLoadCallbackHandler<T>(T asset, bool isReload, ContentHandle handle);

public class ContentLoadHandle : ContentHandle
{
    public class Multipart
    {
        /// <summary>
        /// Gets a list of handles for each part of the asset that the current <see cref="ContentLoadHandle"/> represents.
        /// </summary>
        public ContentLoadHandle[] Handles { get; }

        /// <summary>
        /// Gets whether or not the current <see cref="ContentLoadHandle"/> represents a multi-part asset.
        /// </summary>
        public bool IsPart { get; internal set; }

        /// <summary>
        /// Gets or sets whether or not the current <see cref="ContentLoadHandle"/> should keep its parts loaded. Depending on the content, this may or may not be useful.
        /// </summary>
        public bool KeepPartsLoaded { get; set; }

        internal Multipart(int partCount)
        {
            Handles = new ContentLoadHandle[partCount];
            KeepPartsLoaded = false;
            IsPart = false;
        }
    }

    bool _canHotReload;
    ContentWatcher _watcher;
    internal event ContentLoadCallbackHandler<object> Callbacks;

    internal ContentLoadHandle(
        ContentManager manager,
        string path,
        int partCount,
        Type contentType,
        IContentProcessor processor,
        ContentParameters parameters,
        ContentLoadCallbackHandler<object> callback,
        bool canHotReload) :
        base(manager, path, contentType, processor, parameters, ContentHandleType.Load)
    {
        Callbacks += callback;
        _canHotReload = canHotReload;
        partCount = Math.Max(partCount, 1); // Asset must always have at least 1 part.
        PartInfo = new Multipart(partCount);
    }

    /// <summary>
    /// Check if the asset source file was altered by doing an integrity check.
    /// </summary>
    /// <param name="hashAlgorithm">A <see cref="HashAlgorithm"/> to generate and compare a checksum hash.</param>
    /// <param name="updateChecksum">If true, the current <see cref="Checksum"/> will be updated.</param>
    /// <returns>True if the file has not been altered since it's last (re)load</returns>
    public bool IntegrityCheck(HashAlgorithm hashAlgorithm, bool updateChecksum)
    {
        // Perform a checksum comparison
        byte[] hashBytes = null;
        using (FileStream stream = Info.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            hashBytes = hashAlgorithm.ComputeHash(stream);

        string checksum = BitConverter.ToString(hashBytes);
        if (checksum != Checksum)
        {
            if(updateChecksum)
                Checksum = checksum;

            return false;
        }

        return true;
    }

    protected override sealed ContentHandleStatus OnProcess()
    {
        bool reload = Asset != null;
        bool reinstantiate = true;

        if (reload)
        {
            Type assetType = Asset.GetType();
            object[] att = assetType.GetCustomAttributes(typeof(ContentReloadAttribute), true);
            if (att.Length > 0)
            {
                // Can we reuse the existing asset, or reinstantiate it?
                if (att[0] is ContentReloadAttribute attReload)
                {
                    reinstantiate = attReload.Recreate;

                    // Reinstantiating, so dispose of existing asset if possible.
                    if (Asset is IDisposable disposable)
                        disposable.Dispose();
                }
            }
        }

        return OnRead(reinstantiate);
    }

    protected virtual ContentHandleStatus OnRead(bool reinstantiate)
    {
        // If the current asset (or asset-part) is made up of multiple parts, we'll queue them up
        if (PartInfo.Handles.Length > 1)
        {
            string filename = Info.Name.Replace(Info.Extension, "");
            filename = RelativePath.Replace(Info.Name, $"{filename}_{{0}}{Info.Extension}");

            ContentParameters partParameters = (ContentParameters)Parameters.Clone();
            partParameters.PartCount = 1;

            for (int i = 0; i < PartInfo.Handles.Length; i++)
            {
                string partPath = string.Format(filename, (i + 1));
                PartInfo.Handles[i] = Manager.Load(Processor.PartType, partPath, PartLoadComplete, partParameters, CanHotReload, false);
                PartInfo.Handles[i].PartInfo.IsPart = true;
                PartInfo.Handles[i].Dispatch();
            }

            if (AllPartsCompleted())
                return ContentHandleStatus.Completed;
            else
                return ContentHandleStatus.AwaitingParts;
        }
        else
        {
            if (File.Exists(RelativePath))
            {

                bool success = false;
                using (FileStream stream = new FileStream(RelativePath, FileMode.Open, FileAccess.Read))
                    success = Processor.ReadPart(this, stream);

                if (success)
                {
                    PartInfo.Handles[0] = this;
                    if (Processor.BuildAsset(this))
                    {
                        Complete();
                        return ContentHandleStatus.Completed;
                    }
                }
                else
                {
                    LogError($"Failed to read asset");
                }
            }
            else
            {
                LogError($"File does not exist");
            }
        }

        return ContentHandleStatus.Failed;
    }

    private void PartLoadComplete(object part, bool isReload, ContentHandle handle)
    {
        // Build asset
        if (AllPartsCompleted())
        {
            if (Processor.BuildAsset(this))
                Complete();
        }
    }

    private bool AllPartsCompleted()
    {
        for (int i = 0; i < PartInfo.Handles.Length; i++)
        {
            if (PartInfo.Handles[i] == null ||
                PartInfo.Handles[i].Status != ContentHandleStatus.Completed)
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateWatcher()
    {
        // Delete the watcher if we have one to prevent reloads.
        if (!_canHotReload)
        {
            if (_watcher != null)
                Manager.StopWatching(_watcher, this);
        }
        else
        {
            if (_watcher == null)
                _watcher = Manager.StartWatching(this);
        }
    }

    protected void Complete()
    {
        Status = ContentHandleStatus.Completed;
        Callbacks?.Invoke(Asset, IsLoaded, this);
        IsLoaded = true;
        UpdateWatcher();
    }

    public bool HasAsset()
    {
        return Asset != null;
    }

    /// <summary>
    /// Gets whether or not the current <see cref="ContentLoadHandle"/> has completed it's first-time load.
    /// </summary>
    public bool IsLoaded { get; protected set; }

    /// <summary>
    /// Gets or sets whether the asset is allowed to hot-reload when changes occur to it's file.
    /// </summary>
    public bool CanHotReload
    {
        get => _canHotReload;
        set
        {
            if (_canHotReload != value)
            {
                _canHotReload = value;

                foreach (ContentLoadHandle partHandle in PartInfo.Handles)
                    partHandle.CanHotReload = _canHotReload;

                UpdateWatcher();
            }
        }
    }

    /// <summary>
    /// Gets or sets the last-write timestamp of the asset. This is especially useful for tracking if/when changes happened on hot-reloaded assets.
    /// </summary>
    internal DateTime LastWriteTime { get; set; }

    /// <summary>
    /// Gets the checksum string generated by the latest <see cref="IntegrityCheck(HashAlgorithm, bool)"/> call, where the checksum was allowed to update.
    /// </summary>
    public string Checksum { get; private set; }

    /// <summary>
    /// Gets information about the multi-part configuration of the current <see cref="ContentLoadHandle"/>.
    /// </summary>
    public Multipart PartInfo { get; }
}
