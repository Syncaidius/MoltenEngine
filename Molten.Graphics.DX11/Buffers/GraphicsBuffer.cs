using Molten.Collections;
using Molten.IO;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    internal unsafe partial class GraphicsBuffer : ContextBindableResource<ID3D11Buffer>
    {
        ID3D11Buffer* _native;
        uint _ringPos;

        internal BufferDesc Description;

        ThreadedQueue<IBufferOperation> _pendingChanges;
        BufferSegment _firstSegment;
        List<BufferSegment> _freeSegments;

        internal GraphicsBuffer(DeviceDX11 device,
            BufferMode mode,
            BindFlag bindFlags,
            uint byteCapacity,
            ResourceMiscFlag optionFlags = 0, 
            StagingBufferFlags stagingType = StagingBufferFlags.None, 
            uint structuredStride = 0, 
            Array initialData = null) : base(device,
                ((bindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess ? ContextBindTypeFlags.Output : ContextBindTypeFlags.None) |
                ((bindFlags & BindFlag.ShaderResource) == BindFlag.ShaderResource ? ContextBindTypeFlags.Input : ContextBindTypeFlags.None))
        {
            _freeSegments = new List<BufferSegment>();
            Mode = mode;
            _pendingChanges = new ThreadedQueue<IBufferOperation>();

            BuildDescription(bindFlags, optionFlags, stagingType, byteCapacity, structuredStride);
            InitializeBuffer(initialData);
        }

        internal void QueueOperation(IBufferOperation op)
        {
            _pendingChanges.Enqueue(op);
        }

        internal void Defragment()
        {
            throw new NotImplementedException("Needs to move data around in the underlying GPU buffers when defragmenting. Run every few frames, if needed.");
            // TODO also consider removing map sectors from the end of the list if we end up with more than 1 that is completely empty after defragmentation.
            // TODO consider running based off the number of segments in _freeSegments list.
        }

        private void BuildDescription(
            BindFlag flags, 
            ResourceMiscFlag opFlags, 
            StagingBufferFlags stageMode, 
            uint byteCapacity,
            uint structureByteStride)
        {
            Description = new BufferDesc();
            Description.Usage = Usage.Default;
            Description.BindFlags = (uint)flags;
            Description.MiscFlags = (uint)opFlags;
            Description.ByteWidth = byteCapacity;

            // Buffer mode.
            switch (Mode)
            {
                case BufferMode.Default:
                    Description.Usage = Usage.Default;
                    Description.CPUAccessFlags = 0;
                    break;

                case BufferMode.DynamicDiscard:
                case BufferMode.DynamicRing:
                    Description.Usage = Usage.Dynamic;
                    Description.CPUAccessFlags = (uint)CpuAccessFlag.Write;
                    break;


                case BufferMode.Immutable:
                    Description.Usage = Usage.Immutable;
                    Description.CPUAccessFlags = 0;
                    break;
            }

            // Staging mode
            if (stageMode != StagingBufferFlags.None)
            {
                Description.BindFlags = 0;
                Description.MiscFlags = 0;
                Description.Usage = Usage.Staging;
                Description.CPUAccessFlags = (uint)CpuAccessFlag.Read;
                Description.StructureByteStride = 0;

                if ((stageMode & StagingBufferFlags.Read) == StagingBufferFlags.Read)
                    Description.CPUAccessFlags = (uint)CpuAccessFlag.Read;

                if ((stageMode & StagingBufferFlags.Write) == StagingBufferFlags.Write)
                    Description.CPUAccessFlags = (uint)CpuAccessFlag.Write;
            }

            // Ensure structured buffers get the stride info.
            if (Description.MiscFlags == (uint)ResourceMiscFlag.BufferStructured)
                Description.StructureByteStride = structureByteStride;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialDataPtr">A pointer to data that the buffer should initially be populated with.</param>
        protected virtual void InitializeBuffer(Array initialData)
        {
            if (Mode == BufferMode.Immutable && initialData == null)
                throw new ArgumentNullException("Initial data cannot be null when buffer mode is Immutable.");

            uint numBytes = Description.ByteWidth;


            if (initialData != null)
            {
                EngineUtil.PinObject(initialData, (ptr) =>
                {
                    SubresourceData srd = new SubresourceData(null, numBytes, numBytes);
                    srd.PSysMem = ptr.ToPointer();
                    NativeDevice.Ptr->CreateBuffer(ref Description, ref srd, ref _native);
                });
            }
            else
            {
                NativeDevice.Ptr->CreateBuffer(ref Description, null, ref _native);
            }

            Device.AllocateVRAM(numBytes);

            // Allocate the first segment.
            _firstSegment = NativeDevice.GetBufferSegment();
            _firstSegment.BindFlags = BindFlags;
            _firstSegment.Buffer = this;
            _firstSegment.ByteOffset = 0;
            _firstSegment.ByteCount = numBytes;
            _firstSegment.IsFree = true;

            _freeSegments.Add(_firstSegment);
        }

        protected virtual void OnValidateAllocationStride(uint stride)
        {
            if (((BindFlag)Description.BindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess)
            {
                if (stride != Description.StructureByteStride)
                    throw new GraphicsBufferException("Buffer is structured. Stride must match that of the structured buffer.");
            }
        }

        /// <summary>Copies all the data in the current <see cref="GraphicsBuffer"/> to the destination <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="context">The <see cref="CommandQueueDX11"/> that will perform the copy.</param>
        /// <param name="destination">The <see cref="GraphicsBuffer"/> to copy to.</param>
        internal void CopyTo(CommandQueueDX11 context, GraphicsBuffer destination)
        {
            if (destination.Description.ByteWidth < Description.ByteWidth)
                throw new Exception("The destination buffer is not large enough.");

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == Usage.Staging)
                ApplyChanges(context);

            ValidateCopyBufferUsage(destination);
            context.Native->CopyResource(this, destination);
        }

        internal void CopyTo(CommandQueueDX11 context, GraphicsBuffer destination, Box sourceRegion, uint destByteOffset = 0)
        {
            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == Usage.Staging)
                ApplyChanges(context);

            ValidateCopyBufferUsage(destination);
            context.CopyResourceRegion(this, 0, ref sourceRegion, destination, 0, new Vector3UI(destByteOffset,0,0));
            context.Profiler.Current.CopySubresourceCount++;
        }

        private void ValidateCopyBufferUsage(GraphicsBuffer destination)
        {
            if (Description.Usage != Usage.Default && 
                Description.Usage != Usage.Immutable)
                throw new Exception("The current buffer must have a usage flag of Default or Immutable. Only these flags allow the GPU read access for copying/reading data from the buffer.");

            if (destination.Description.Usage != Usage.Default)
                throw new Exception("The destination buffer must have a usage flag of Staging or Default. Only these two allow the GPU write access for copying/writing data to the destination.");
        }

        internal void GetStream(CommandQueueDX11 context, 
            uint byteOffset, 
            uint dataSize, 
            Action<GraphicsBuffer, RawStream> callback, 
            GraphicsBuffer staging = null)
        {
            // Check buffer type.
            bool isDynamic = Description.Usage == Usage.Dynamic;
            bool isStaged = Description.Usage == Usage.Staging &&
                (Description.CPUAccessFlags & (uint)CpuAccessFlag.Write) == (uint)CpuAccessFlag.Write;

            RawStream stream;

            // Check if the buffer is a dynamic-writable
            if (isDynamic || isStaged)
            {
                switch (Mode)
                {
                    case BufferMode.DynamicDiscard:
                        context.MapResource(NativePtr, 0, Map.WriteDiscard, 0, out stream);
                        stream.Position = byteOffset;
                        context.Profiler.Current.MapDiscardCount++;
                        break;

                    case BufferMode.DynamicRing:
                        // NOTE: D3D11_MAP_WRITE_NO_OVERWRITE is only valid on vertex and index buffers. 
                        // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476181(v=vs.85).aspx
                        if (HasFlags(BindFlag.VertexBuffer) || HasFlags(BindFlag.IndexBuffer))
                        {
                            if (_ringPos > 0 && _ringPos + dataSize < Description.ByteWidth)
                            {
                                context.MapResource(NativePtr, 0, Map.WriteNoOverwrite, 0, out stream);
                                context.Profiler.Current.MapNoOverwriteCount++;
                                stream.Position = _ringPos;
                                _ringPos += dataSize;
                            }
                            else
                            {                                
                                context.MapResource(NativePtr, 0, Map.WriteDiscard, 0, out stream);
                                context.Profiler.Current.MapDiscardCount++;
                                stream.Position = 0;
                                _ringPos = dataSize;
                            }
                        }
                        else
                        {
                            context.MapResource(NativePtr, 0, Map.WriteDiscard, 0, out stream);
                            context.Profiler.Current.MapDiscardCount++;
                            stream.Position = byteOffset;
                        }
                        break;

                    default:
                        context.MapResource(NativePtr, 0, Map.Write, 0, out stream);
                        context.Profiler.Current.MapWriteCount++;
                        break;
                }     
                
                callback(this, stream);
                context.UnmapResource(NativePtr, 0);
            }
            else
            {
                // Write to the provided staging buffer instead.
                if (staging == null)
                    throw new GraphicsBufferException("Staging buffer required. Non-dynamic/staged buffers require a staging buffer to access data.");

                isDynamic = staging.Description.Usage == Usage.Dynamic;
                isStaged = staging.Description.Usage == Usage.Staging;

                if (!isDynamic && !isStaged)
                    throw new GraphicsBufferException("The provided staging buffer is invalid. Must be either dynamic or staged.");

                if (staging.Description.ByteWidth < dataSize)
                    throw new GraphicsBufferException($"The provided staging buffer is not large enough ({staging.Description.ByteWidth} bytes) to fit the provided data ({dataSize} bytes).");

                // Write updated data into buffer
                if (isDynamic) // Always discard staging buffer data, since the old data is no longer needed after it's been copied to it's target resource.
                {
                    context.MapResource(staging.NativePtr, 0, Map.WriteDiscard, 0, out stream);
                    context.Profiler.Current.MapDiscardCount++;
                }
                else
                {
                    context.MapResource(staging.NativePtr, 0, Map.Write, 0, out stream);
                    context.Profiler.Current.MapWriteCount++;
                }

                callback(staging, stream);
                context.UnmapResource(staging.NativePtr, 0);

                Box stagingRegion = new Box()
                {
                    Left = 0,
                    Right = dataSize,
                    Back = 1,
                    Bottom = 1,
                };
                context.CopyResourceRegion(staging, 0, ref stagingRegion, 
                    this, 0, new Vector3UI(byteOffset, 0, 0));
                context.Profiler.Current.CopySubresourceCount++;
            }
        }

        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        internal void Set<T>(
            CommandQueueDX11 context,
            T[] data,
            uint startIndex,
            uint count,
            uint dataStride = 0,
            uint byteOffset = 0,
            StagingBuffer staging = null)
            where T : unmanaged
        {
            if (dataStride == 0)
                dataStride = (uint)sizeof(T);

            uint dataSize = count * dataStride;

            GetStream(context, byteOffset, dataSize, (buffer, stream) =>
            {
                stream.WriteRange<T>(data, startIndex, count);
            }, staging);
        }

        /// <summary>Retrieves data from a <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="context">The <see cref="CommandQueueDX11"/> that will perform the 'get' operation.</param>
        /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
        /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
        /// <param name="count">The number of elements to retrieve</param>
        /// <param name="dataStride">The size of the data being retrieved. The default value is 0. 
        /// A value of 0 will force the stride of <see cref="{T}"/> to be automatically calculated, which may cause a tiny performance hit.</param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        internal void Get<T>(CommandQueueDX11 context, T[] destination, uint startIndex, uint count, uint dataStride, uint byteOffset = 0)
            where T : unmanaged
        {
            uint readOffset = startIndex * dataStride;

            if ((Description.CPUAccessFlags & (uint)CpuAccessFlag.Read) != (uint)CpuAccessFlag.Read)
                throw new InvalidOperationException("Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            //now set the structured variable's data
            RawStream stream = null;
            MappedSubresource dataBox = context.MapResource(NativePtr, 0, Map.Read, 0, out stream);
            context.Profiler.Current.MapReadCount++;
            stream.Position = byteOffset;
            stream.ReadRange<T>(destination, readOffset, count);

            // Unmap
            context.UnmapResource(NativePtr, 0);
        }

        /// <summary>Applies any pending changes onto the buffer.</summary>
        /// <param name="context">The graphics pipe to use when process changes.</param>
        /// <param name="forceInitialize">If set to true, the buffer will be initialized if not done so already.</param>
        protected void ApplyChanges(CommandQueueDX11 context)
        {
            if (_pendingChanges.Count > 0)
            {
                IBufferOperation op = null;
                while (_pendingChanges.TryDequeue(out op))
                    op.Process(context);
            }
        }

        internal void Clear()
        {
            _pendingChanges.Clear();
        }

        internal bool HasFlags(BindFlag flag)
        {
            return ((BindFlag)Description.BindFlags & flag) == flag;
        }

        internal bool HasFlag(CpuAccessFlag flag)
        {
            return ((CpuAccessFlag)Description.CPUAccessFlags & flag) == flag;
        }

        protected override void OnApply(CommandQueueDX11 context)
        {
            ApplyChanges(context);
        }

        public override void GraphicsRelease()
        {
            base.GraphicsRelease();

            if (NativePtr != null)
            {
                SilkUtil.ReleasePtr(ref _native);
                Device.DeallocateVRAM(Description.ByteWidth);
            }
        }

        protected virtual void CreateResources(uint stride, uint byteOffset, uint elementCount,
            SRView srv, UAView uav)
        {
            if (HasFlags(BindFlag.ShaderResource))
            {
                srv.Desc = new ShaderResourceViewDesc1()
                {
                    Buffer = new BufferSrv()
                    {
                        NumElements = elementCount,
                        FirstElement = byteOffset,
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                    Format = Format.FormatUnknown,
                };

                srv.Create(this);
            }

            if (HasFlags(BindFlag.UnorderedAccess))
            {
                uav.Desc = new UnorderedAccessViewDesc1()
                {
                    Format = Format.FormatUnknown,
                    ViewDimension = UavDimension.Buffer,
                    Buffer = new BufferUav()
                    {
                        NumElements = elementCount,
                        FirstElement = byteOffset / Description.StructureByteStride,
                        Flags = 0,
                    }
                };
                uav.Create(this);
            }
        }

        private void CreateResources(uint stride, uint byteOffset, uint elementCount)
        {
            CreateResources(stride, byteOffset, elementCount, SRV, UAV);
        }

        private void CreateResources(BufferSegment segment)
        {
            CreateResources(segment.Stride, segment.ByteOffset, segment.ElementCount, segment.SRV, segment.UAV);
        }

        internal BufferSegment Allocate<T>(uint count) where T : unmanaged
        {
            uint stride = (uint)sizeof(T);
            OnValidateAllocationStride(stride);

            BufferSegment seg;
            uint byteCount = count * stride;

            // iterate backwards
            for (int i = _freeSegments.Count - 1; i >= 0; i--)
            {
                seg = _freeSegments[i];
                if (seg.IsFree)
                {
                    // Downsize the current segment and insert a new allocated segment in front of it.
                    if (seg.ByteCount > byteCount)
                    {
                        // Create a new segment to match the requested size
                        BufferSegment newSeg = seg.SplitFromFront(byteCount);
                        newSeg.ElementCount = count;
                        newSeg.Stride = stride;
                        CreateResources(newSeg);
                        return newSeg;
                    }
                    else if (seg.ByteCount == byteCount) // Perfect. Use it without any resizing.
                    {
                        seg.IsFree = false;
                        seg.ElementCount = count;
                        seg.Stride = stride;

                        _freeSegments.Remove(seg);
                        CreateResources(seg);
                        return seg;
                    }
                }
            }

            // Unable to allocate.
            return null;
        }


        /// <summary>Release the buffer space held by the specified segment.</summary>
        /// <param name="segment">The segment.</param>
        internal void Deallocate(BufferSegment segment)
        {
            bool mergePrev = segment.Previous != null && segment.Previous.IsFree;
            bool mergeNext = segment.Next != null && segment.Next.IsFree;

            if (mergePrev && mergeNext)
            {
                // Merge the current and next into the previous. This removes two segments by merging a total of 3 together.
                segment.Previous.ByteCount += segment.ByteCount;
                segment.Previous.ByteCount += segment.Next.ByteCount;
                segment.Previous.LinkNext(segment.Next.Next);

                // The next one will be listed in _freeSegments, so removei t.
                _freeSegments.Remove(segment.Next);

                NativeDevice.RecycleBufferSegment(segment.Next);
                NativeDevice.RecycleBufferSegment(segment);
            }
            else if (mergePrev)
            {
                segment.Previous.ByteCount += segment.ByteCount;
                segment.Previous.LinkNext(segment.Next);
                NativeDevice.RecycleBufferSegment(segment);
            }
            else if (mergeNext)
            {
                segment.ByteCount += segment.Next.ElementCount;
                segment.LinkNext(segment.Next.Next);
                segment.IsFree = true;

                NativeDevice.RecycleBufferSegment(segment.Next);
                _freeSegments.Add(segment);
            }
        }

        /// <summary>Updates or resizes the allocation.</summary>
        /// <param name="existing">The existing segment to be updated or reallocated if neccessary.</param>
        /// <param name="count">The new element count.</param>
        /// <returns></returns>
        internal BufferSegment UpdateAllocation<T>(BufferSegment existing, uint count) 
            where T : unmanaged
        {
            uint newStride = (uint)sizeof(T);
            OnValidateAllocationStride(newStride);

            uint oldBytes = existing.ByteCount;
            uint newBytes = newStride * count;
            uint oldCount = existing.ElementCount;

            if (oldBytes == newBytes)
            {
                return existing;
            }
            else if (oldBytes > newBytes) // Downsize
            {
                uint dif = oldBytes - newBytes;

                BufferSegment freed = existing.SplitFromFront(dif);
                freed.IsFree = true;
                existing.Stride = newStride;
                existing.ElementCount = count;
                _freeSegments.Add(freed);
            }
            else if (newBytes > oldBytes) // Upsize
            {
                uint dif = newBytes - oldBytes;
                bool canMergeNext = existing.Next != null && existing.Next.IsFree;
                bool canMergePrev = existing.Previous != null && existing.Previous.IsFree;

                if (canMergeNext && existing.Next.ByteCount >= dif)
                {
                    existing.ElementCount = count;
                    existing.Stride = newStride;
                    existing.TakeFromNext(dif);
                }
                else if (canMergePrev && existing.Previous.ByteCount >= dif)
                {
                    existing.ElementCount = count;
                    existing.Stride = newStride;
                    existing.TakeFromPrevious(dif);

                    // TODO queue copy change to move the data backward to the new byte offset.
                }
                else
                {
                    if ((canMergeNext && canMergePrev) && ((existing.Previous.ByteCount + existing.Next.ByteCount) >= dif))
                    {
                        dif -= existing.Previous.ByteCount;

                        existing.ElementCount = count;
                        existing.Stride = newStride;
                        existing.TakeFromPrevious(existing.Previous.ByteCount);
                        existing.TakeFromNext(dif);

                        // TODO queue copy change to move the data backward to the new byte offset.
                    }
                }
            }

            // If we've reached this far, there's not enough free space nearby. Find a new location.
            if (existing.ByteCount != newBytes)
            {
                BufferSegment newSeg = Allocate<T>(count);

                // TODO queue copy to move data from old to new segment (if allowed).

                Deallocate(existing);
                return newSeg;
            }
            else
            {
                SegmentResized(existing, oldBytes, newBytes, oldCount, count);
                return existing;
            }
        }

        private void SegmentResized(BufferSegment segment, uint oldByteCount, uint newByteCount, uint oldElementCount, uint newElementCount)
        {
            CreateResources(segment);
            OnSegmentResized(segment, oldByteCount, newByteCount, oldElementCount, newElementCount);
        }

        protected virtual void OnSegmentResized(BufferSegment segment, uint oldByteCount, uint newByteCount, uint oldElementCount, uint newElementCount) { }

        /// <summary>Gets the structured stride which <see cref="BufferSegment"/> instances must adhere to if they belong to the current <see cref="GraphicsBuffer"/>. 
        /// This is ignored and unused if the <see cref="GraphicsBuffer"/> does not carry the <see cref="ResourceOptionFlags.BufferStructured"/> flag.</summary>
        public uint StructuredStride => Description.StructureByteStride;

        /// <summary>Gets the capacity of a single section within the buffer, in bytes.</summary>
        public uint ByteCapacity => Description.ByteWidth;

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public BufferMode Mode { get; }

        /// <summary>Gets the bind flags associated with the buffer.</summary>
        public BindFlag BufferBindFlags => (BindFlag)Description.BindFlags;

        /// <summary>Gets the underlying DirectX 11 buffer. </summary>
        internal override ID3D11Buffer* ResourcePtr => _native;

        internal override unsafe ID3D11Resource* NativePtr => (ID3D11Resource*)_native;

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        public ResourceMiscFlag ResourceFlags => (ResourceMiscFlag)Description.MiscFlags;

        /// <summary>
        /// Gets a value indicating whether the current buffer is a shader resource.
        /// </summary>
        public bool IsShaderResource =>((BindFlag)Description.BindFlags & BindFlag.ShaderResource) == BindFlag.ShaderResource;

        /// <summary>
        /// Gets a value indicating whether the current buffer has unordered access.
        /// </summary>
        public bool IsUnorderedAccess => ((BindFlag)Description.BindFlags & BindFlag.UnorderedAccess) == BindFlag.UnorderedAccess;
    }
}
