using Molten.Collections;
using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe abstract class PipeBuffer : PipeBindableResource<ID3D11Buffer>
    {
        ID3D11Buffer* _native;
        BufferDesc _desc;

        // TODO implement BufferedList (a non-threaded list with internal ThreadedLists for queuing additions/removals).
        ThreadedQueue<IBufferOperation> _pendingChanges;
        BufferSegment _firstSegment;
        List<BufferSegment> _freeSegments;

        internal PipeBuffer(BufferMode mode, PipeStageType canBindTo, PipeBindTypeFlags bindTypeFlags) : 
            base(canBindTo, bindTypeFlags)
        {
            Mode = mode;
            _freeSegments = new List<BufferSegment>();
            _pendingChanges = new ThreadedQueue<IBufferOperation>();

            if (mode == BufferMode.Immutable && initialData == null)
                throw new ArgumentNullException("Initial data cannot be null when buffer mode is Immutable.");

            _initialData = initialData;

            BuildDescription(bindFlags, optionFlags, stagingType, byteCapacity, structuredStride);

            // TODO initialize buffer.
        }

        private void BuildDescription(
            BindFlag flags,
            ResourceMiscFlag opFlags,
            StagingBufferFlags stageMode,
            uint byteCapacity,
            uint structureByteStride)
        {
            _desc = new BufferDesc();
            _desc.Usage = Usage.UsageDefault;
            _desc.BindFlags = (uint)flags;
            _desc.MiscFlags = (uint)opFlags;

            // Buffer mode.
            switch (Mode)
            {
                case BufferMode.Default:
                    _desc.Usage = Usage.UsageDefault;
                    _desc.CPUAccessFlags = 0;
                    break;

                case BufferMode.DynamicDiscard:
                case BufferMode.DynamicRing:
                    _desc.Usage = Usage.UsageDynamic;
                    _desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessWrite;
                    break;


                case BufferMode.Immutable:
                    _desc.Usage = Usage.UsageImmutable;
                    _desc.CPUAccessFlags = 0;
                    break;
            }

            // Staging mode
            if (stageMode != StagingBufferFlags.None)
            {
                _desc.BindFlags = 0;
                _desc.MiscFlags = 0;
                _desc.Usage = Usage.UsageStaging;
                _desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;
                _desc.StructureByteStride = 0;

                if ((stageMode & StagingBufferFlags.Read) == StagingBufferFlags.Read)
                    _desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;

                if ((stageMode & StagingBufferFlags.Write) == StagingBufferFlags.Write)
                    _desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessWrite;
            }

            // Ensure structured buffers get the stride info.
            if (_desc.MiscFlags == (uint)ResourceMiscFlag.ResourceMiscBufferStructured)
                _desc.StructureByteStride = structureByteStride;
        }

        internal override unsafe ID3D11Buffer* Native => _native;

        public BufferMode Mode { get; }
    }
}
