namespace Molten;

public class ContentSaveHandle : ContentHandle
{
    Action<FileInfo> _completionCallback;

    internal ContentSaveHandle(
        ContentManager manager, 
        string path, 
        object asset,
        IContentProcessor processor, 
        ContentParameters parameters, 
        Action<FileInfo> completionCallback) : 
        base(manager, path, asset.GetType(), processor, parameters, ContentHandleType.Save)
    {
        _completionCallback = completionCallback;
        Asset = asset;
    }

    protected override ContentHandleStatus OnProcess()
    {
        if (!Info.Directory.Exists)
            Info.Directory.Create();

        bool success = false;

        using (FileStream stream = new FileStream(RelativePath, FileMode.Create, FileAccess.Write))
            success = Processor.Write(this, stream);

        if (success)
        {
            _completionCallback?.Invoke(new FileInfo(Info.FullName));
            return ContentHandleStatus.Completed;
        }

        return ContentHandleStatus.Failed;
    }
}
