using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public abstract class ContentSaveHandle : ContentHandle
    {
        internal Action<FileInfo> _completionCallback;

        internal ContentSaveHandle(
            ContentManager manager, 
            object asset,
            IContentProcessor processor, 
            IContentParameters parameters, 
            Action<FileInfo> completionCallback) : 
            base(manager, asset.GetType(), processor, parameters, ContentHandleType.Save)
        {
            _completionCallback = completionCallback;
            Asset = asset;
        }

        protected override void OnComplete()
        {
            _completionCallback?.Invoke(new FileInfo(Info.FullName));
        }

        protected override bool OnProcess()
        {
            if (!Info.Directory.Exists)
                Info.Directory.Create();

            return Processor.Write(this, Asset);
        }
    }
}
