using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
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

        protected override ContentHandleStatus OnComplete()
        {
            _completionCallback?.Invoke(new FileInfo(Info.FullName));
            return ContentHandleStatus.Completed;
        }

        protected override bool OnProcess()
        {
            if (!Info.Directory.Exists)
                Info.Directory.Create();

            return Processor.Write(this, Asset);
        }
    }
}
