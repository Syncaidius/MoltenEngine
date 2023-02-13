using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RenderDataBatch
    {
        public IReadOnlyList<ObjectRenderData> Data { get; }

        List<ObjectRenderData> _data;
        ObjectRenderData _last;


        internal RenderDataBatch()
        {
            _data = new List<ObjectRenderData>();
            Data = _data.AsReadOnly();
        }

        public void Add(ObjectRenderData data)
        {
            _data.Add(data);
            _last = data;
            DirtyFlags |= RenderBatchDirtyFlags.End;
        }

        public void Remove(ObjectRenderData data)
        {
            DirtyFlags |= RenderBatchDirtyFlags.Removed;

            if (data == _last)
                DirtyFlags |= RenderBatchDirtyFlags.End;

            _data.Remove(data);
        }

        public bool HasFlags(RenderBatchDirtyFlags flags)
        {
            return (DirtyFlags & flags) == flags;
        }

        internal RenderBatchDirtyFlags DirtyFlags { get; set; }
    }

    [Flags]
    public enum RenderBatchDirtyFlags
    {
        None = 0,

        End = 1,

        Removed = 2,
    }
}
