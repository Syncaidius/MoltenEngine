using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Molten.Collections;
using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    internal unsafe partial class GraphicsBuffer : PipeBindableResource<ID3D11Buffer>
    {
        ID3D11Buffer* _native;
        BufferDesc _desc;
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
            Array initialData = null) : base(device)
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
            Description.Usage = Usage.UsageDefault;
            Description.BindFlags = (uint)flags;
            Description.MiscFlags = (uint)opFlags;

            // Buffer mode.
            switch (Mode)
            {
                case BufferMode.Default:
                    Description.Usage = Usage.UsageDefault;
                    Description.CPUAccessFlags = 0;
                    break;

                case BufferMode.DynamicDiscard:
                case BufferMode.DynamicRing:
                    Description.Usage = Usage.UsageDynamic;
                    Description.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessWrite;
                    break;


                case BufferMode.Immutable:
                    Description.Usage = Usage.UsageImmutable;
                    Description.CPUAccessFlags = 0;
                    break;
            }

            // Staging mode
            if (stageMode != StagingBufferFlags.None)
            {
                Description.BindFlags = 0;
                Description.MiscFlags = 0;
                Description.Usage = Usage.UsageStaging;
                Description.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;
                Description.StructureByteStride = 0;

                if ((stageMode & StagingBufferFlags.Read) == StagingBufferFlags.Read)
                    Description.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;

                if ((stageMode & StagingBufferFlags.Write) == StagingBufferFlags.Write)
                    Description.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessWrite;
            }

            // Ensure structured buffers get the stride info.
            if (Description.MiscFlags == (uint)ResourceMiscFlag.ResourceMiscBufferStructured)
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

            uint byteCount = _desc.ByteWidth;

            SubresourceData ssd = new SubresourceData(null, byteCount, byteCount);
            if (initialData != null)
                EngineUtil.PinObject(initialData, (ptr) => ssd.PSysMem = ptr.ToPointer());
            else
                ssd = new SubresourceData(null, byteCount, byteCount);

            Device.Native->CreateBuffer(ref _desc, ref ssd, ref _native);

            Device.AllocateVRAM(byteCount);

            // Allocate the first segment.
            _firstSegment = Device.GetBufferSegment();
            _firstSegment.Buffer = this;
            _firstSegment.ByteOffset = 0;
            _firstSegment.ByteCount = byteCount;
            _firstSegment.IsFree = true;

            _freeSegments.Add(_firstSegment);
        }

        protected virtual void OnValidateAllocationStride(uint stride)
        {
            if (((BindFlag)Description.BindFlags & BindFlag.BindUnorderedAccess) == BindFlag.BindUnorderedAccess)
            {
                if (stride != Description.StructureByteStride)
                    throw new GraphicsBufferException("Buffer is structured. Stride must match that of the structured buffer.");
            }
        }

        /// <summary>Copies all the data in the current <see cref="GraphicsBuffer"/> to the destination <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="pipe">The <see cref="PipeDX11"/> that will perform the copy.</param>
        /// <param name="destination">The <see cref="GraphicsBuffer"/> to copy to.</param>
        internal void CopyTo(PipeDX11 pipe, GraphicsBuffer destination)
        {
            if (destination.Description.ByteWidth < Description.ByteWidth)
                throw new Exception("The destination buffer is not large enough.");

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == Usage.UsageStaging)
                ApplyChanges(pipe);

            ValidateCopyBufferUsage(destination);
            pipe.Context->CopyResource(this, destination);
        }

        internal void CopyTo(PipeDX11 pipe, GraphicsBuffer destination, Box sourceRegion, uint destByteOffset = 0)
        {
            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == Usage.UsageStaging)
                ApplyChanges(pipe);

            ValidateCopyBufferUsage(destination);
            pipe.CopyResourceRegion(this, 0, ref sourceRegion, destination, 0, new Vector3UI(destByteOffset,0,0));
            pipe.Profiler.Current.CopySubresourceCount++;
        }

        private void ValidateCopyBufferUsage(GraphicsBuffer destination)
        {
            if (Description.Usage != Usage.UsageDefault && 
                Description.Usage != Usage.UsageImmutable)
                throw new Exception("The current buffer must have a usage flag of Default or Immutable. Only these flags allow the GPU read access for copying/reading data from the buffer.");

            if (destination.Description.Usage != Usage.UsageDefault)
                throw new Exception("The destination buffer must have a usage flag of Staging or Default. Only these two allow the GPU write access for copying/writing data to the destination.");
        }

        internal void GetStream(PipeDX11 pipe, 
            uint byteOffset, 
            uint dataSize, 
            Action<GraphicsBuffer, ResourceStream> callback, 
            GraphicsBuffer staging = null)
        {
            // Check buffer type.
            bool isDynamic = Description.Usage == Usage.UsageDynamic;
            bool isStaged = Description.Usage == Usage.UsageStaging &&
                (Description.CPUAccessFlags & (uint)CpuAccessFlag.CpuAccessWrite) == (uint)CpuAccessFlag.CpuAccessWrite;

            ResourceStream stream;

            // Check if the buffer is a dynamic-writable
            if (isDynamic || isStaged)
            {
                switch (Mode)
                {
                    case BufferMode.DynamicDiscard:
                        pipe.MapResource(NativePtr, 0, Map.MapWriteDiscard, 0, out stream);
                        stream.Position = byteOffset;
                        pipe.Profiler.Current.MapDiscardCount++;
                        break;

                    case BufferMode.DynamicRing:
                        // NOTE: D3D11_MAP_WRITE_NO_OVERWRITE is only valid on vertex and index buffers. 
                        // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476181(v=vs.85).aspx
                        if (HasFlags(BindFlag.BindVertexBuffer) || HasFlags(BindFlag.BindIndexBuffer))
                        {
                            if (_ringPos > 0 && _ringPos + dataSize < Description.ByteWidth)
                            {
                                pipe.MapResource(NativePtr, 0, Map.MapWriteNoOverwrite, 0, out stream);
                                pipe.Profiler.Current.MapNoOverwriteCount++;
                                stream.Position = _ringPos;
                                _ringPos += dataSize;
                            }
                            else
                            {                                
                                pipe.MapResource(NativePtr, 0, Map.MapWriteDiscard, 0, out stream);
                                pipe.Profiler.Current.MapDiscardCount++;
                                stream.Position = 0;
                                _ringPos = dataSize;
                            }
                        }
                        else
                        {
                            pipe.MapResource(NativePtr, 0, Map.MapWriteDiscard, 0, out stream);
                            pipe.Profiler.Current.MapDiscardCount++;
                            stream.Position = byteOffset;
                        }
                        break;

                    default:
                        pipe.MapResource(NativePtr, 0, Map.MapWrite, 0, out stream);
                        pipe.Profiler.Current.MapWriteCount++;
                        break;
                }     
                
                callback(this, stream);
                pipe.UnmapResource(NativePtr, 0);
            }
            else
            {
                // Write to the provided staging buffer instead.
                if (staging == null)
                    throw new GraphicsBufferException("Staging buffer required. Non-dynamic/staged buffers require a staging buffer to access data.");

                isDynamic = staging.Description.Usage == Usage.UsageDynamic;
                isStaged = staging.Description.Usage == Usage.UsageStaging;

                if (!isDynamic && !isStaged)
                    throw new GraphicsBufferException("The provided staging buffer is invalid. Must be either dynamic or staged.");

                if (staging.Description.ByteWidth < dataSize)
                    throw new GraphicsBufferException($"The provided staging buffer is not large enough ({staging.Description.ByteWidth} bytes) to fit the provided data ({dataSize} bytes).");

                // Write updated data into buffer
                if (isDynamic) // Always discard staging buffer data, since the old data is no longer needed after it's been copied to it's target resource.
                {
                    pipe.MapResource(staging.NativePtr, 0, Map.MapWriteDiscard, 0, out stream);
                    pipe.Profiler.Current.MapDiscardCount++;
                }
                else
                {
                    pipe.MapResource(staging.NativePtr, 0, Map.MapWrite, 0, out stream);
                    pipe.Profiler.Current.MapWriteCount++;
                }

                callback(staging, stream);
                pipe.UnmapResource(staging.NativePtr, 0);

                Box stagingRegion = new Box()
                {
                    Left = 0,
                    Right = dataSize,
                    Back = 1,
                    Bottom = 1,
                };
                pipe.CopyResourceRegion(staging, 0, ref stagingRegion, 
                    this, 0, new Vector3UI(byteOffset, 0, 0));
                pipe.Profiler.Current.CopySubresourceCount++;
            }
        }

        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        internal void Set<T>(
            PipeDX11 pipe, 
            T[] data, 
            uint startIndex, 
            uint count,
            uint dataStride = 0, 
            uint byteOffset = 0, 
            StagingBuffer staging = null) 
            where T : struct
        {
            if (dataStride == 0)
                dataStride = (uint)Marshal.SizeOf<T>();

            uint dataSize = count * dataStride;

            GetStream(pipe, byteOffset, dataSize, (buffer, stream) =>
            {
                stream.WriteRange(data, startIndex, count);
            }, staging);
        }

        /// <summary>Retrieves data from a <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="pipe">The <see cref="PipeDX11"/> that will perform the 'get' operation.</param>
        /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
        /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
        /// <param name="count">The number of elements to retrieve</param>
        /// <param name="dataStride">The size of the data being retrieved. The default value is 0. 
        /// A value of 0 will force the stride of <see cref="{T}"/> to be automatically calculated, which may cause a tiny performance hit.</param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        internal void Get<T>(PipeDX11 pipe, T[] destination, uint startIndex, uint count, uint dataStride, uint byteOffset = 0)
            where T : struct
        {
            uint readOffset = startIndex * dataStride;

            if ((Description.CPUAccessFlags & (uint)CpuAccessFlag.CpuAccessRead) != (uint)CpuAccessFlag.CpuAccessRead)
                throw new InvalidOperationException("Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            //now set the structured variable's data
            ResourceStream stream = null;
            MappedSubresource dataBox = pipe.MapResource(NativePtr, 0, Map.MapRead, 0, out stream);
            pipe.Profiler.Current.MapReadCount++;
            stream.Position = byteOffset;
            stream.ReadRange<T>(destination, readOffset, count);

            // Unmap
            pipe.UnmapResource(NativePtr, 0);
        }

        /// <summary>Applies any pending changes onto the buffer.</summary>
        /// <param name="pipe">The graphics pipe to use when process changes.</param>
        /// <param name="forceInitialize">If set to true, the buffer will be initialized if not done so already.</param>
        protected void ApplyChanges(PipeDX11 pipe)
        {
            if (_pendingChanges.Count > 0)
            {
                IBufferOperation op = null;
                while (_pendingChanges.TryDequeue(out op))
                    op.Process(pipe);
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

        protected internal override void Refresh(PipeSlot slot, PipeDX11 pipe)
        {
            ApplyChanges(pipe);
        }

        internal override void PipelineDispose()
        {
            base.PipelineDispose();

            if (NativePtr != null)
            {
                SilkUtil.ReleasePtr(ref _native);
                Device.DeallocateVRAM(Description.ByteWidth);
            }
        }

        private void CreateResources(uint stride, uint byteOffset, uint elementCount,
            ref ID3D11ShaderResourceView* srv, ref ID3D11UnorderedAccessView* uav)
        {
            if (HasFlags(BindFlag.BindShaderResource))
            {
                SilkUtil.ReleasePtr(ref srv);

                ShaderResourceViewDesc srvDesc = new ShaderResourceViewDesc()
                {
                    Buffer = new BufferSrv()
                    {
                        NumElements = elementCount,
                        FirstElement = byteOffset,
                    },
                    ViewDimension = D3DSrvDimension.D3D11SrvDimensionBuffer,
                    Format = Format.FormatUnknown,
                };

                Device.Native->CreateShaderResourceView(this, ref srvDesc, ref srv);
            }

            if (HasFlags(BindFlag.BindUnorderedAccess))
            {
                SilkUtil.ReleasePtr(ref uav);

                UnorderedAccessViewDesc uavDesc = new UnorderedAccessViewDesc()
                {
                    Format = Format.FormatUnknown,
                    ViewDimension = UavDimension.UavDimensionBuffer,
                    Buffer = new BufferUav()
                    {
                        NumElements = elementCount,
                        FirstElement = byteOffset / Description.StructureByteStride,
                        Flags = 0,
                    }
                };
                Device.Native->CreateUnorderedAccessView(this, ref uavDesc, ref uav);
            }
        }

        internal virtual void CreateResources(uint stride, uint byteOffset, uint elementCount)
        {
            CreateResources(stride, byteOffset, elementCount, ref SRV, ref UAV);
        }

        private void CreateResources(BufferSegment segment)
        {
            CreateResources(segment.Stride, segment.ByteOffset, segment.ElementCount, ref segment.SRV, ref segment.UAV);
        }

        internal BufferSegment Allocate<T>(uint count) where T : struct
        {
            Type allocatedType = typeof(T);
            uint stride = (uint)Marshal.SizeOf(allocatedType);
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

                Device.RecycleBufferSegment(segment.Next);
                Device.RecycleBufferSegment(segment);
            }
            else if (mergePrev)
            {
                segment.Previous.ByteCount += segment.ByteCount;
                segment.Previous.LinkNext(segment.Next);
                Device.RecycleBufferSegment(segment);
            }
            else if (mergeNext)
            {
                segment.ByteCount += segment.Next.ElementCount;
                segment.LinkNext(segment.Next.Next);
                segment.IsFree = true;

                Device.RecycleBufferSegment(segment.Next);
                _freeSegments.Add(segment);
            }
        }

        /// <summary>Updates or resizes the allocation.</summary>
        /// <param name="existing">The existing segment to be updated or reallocated if neccessary.</param>
        /// <param name="count">The new element count.</param>
        /// <returns></returns>
        internal BufferSegment UpdateAllocation<T>(BufferSegment existing, uint count) where T : struct
        {
            uint newStride = (uint)Marshal.SizeOf(typeof(T));
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

        /// <summary>Gets the buffer's first segment. This always exists.</summary>
        internal BufferSegment FirstSegment => _firstSegment;

        /// <summary>Gets the structured stride which <see cref="BufferSegment"/> instances must adhere to if they belong to the current <see cref="GraphicsBuffer"/>. 
        /// This is ignored and unused if the <see cref="GraphicsBuffer"/> does not carry the <see cref="ResourceOptionFlags.BufferStructured"/> flag.</summary>
        public uint StructuredStride => Description.StructureByteStride;

        /// <summary>Gets the capacity of a single section within the buffer, in bytes.</summary>
        public uint ByteCapacity => Description.ByteWidth;

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public BufferMode Mode { get; }

        /// <summary>Gets the bind flags associated with the buffer.</summary>
        public BindFlag BindFlags => (BindFlag)Description.BindFlags;

        /// <summary>Gets the underlying DirectX 11 buffer. </summary>
        internal override ID3D11Buffer* ResourcePtr => _native;

        internal override unsafe ID3D11Resource* NativePtr => (ID3D11Resource*)_native;

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        public ResourceMiscFlag ResourceFlags => (ResourceMiscFlag)Description.MiscFlags;

        /// <summary>
        /// Gets a value indicating whether the current buffer is a shader resource.
        /// </summary>
        public bool IsShaderResource =>((BindFlag)Description.BindFlags & BindFlag.BindShaderResource) == BindFlag.BindShaderResource;

        /// <summary>
        /// Gets a value indicating whether the current buffer has unordered access.
        /// </summary>
        public bool IsUnorderedAccess => ((BindFlag)Description.BindFlags & BindFlag.BindUnorderedAccess) == BindFlag.BindUnorderedAccess;
    }
}
