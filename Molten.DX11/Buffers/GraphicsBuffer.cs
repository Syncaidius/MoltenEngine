using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Molten.Collections;

namespace Molten.Graphics
{
    internal unsafe partial class GraphicsBuffer : PipelineShaderObject
    {
        protected uint _byteCapacity;
        protected uint _structuredStride = 0;
        protected Array _initialData;
        internal ID3D11Buffer* Native;
        
        BufferMode _mode;
        uint _ringPos;

        internal BufferDescription Description;
        internal BufferBinding VertexBinding;

        ThreadedQueue<IBufferOperation> _pendingChanges;
        BufferSegment _firstSegment;
        List<BufferSegment> _freeSegments;

        internal GraphicsBuffer(DeviceDX11 device,
            BufferMode mode,
            BindFlag bindFlags,
            uint byteCapacity,
            ResourceOptionFlags optionFlags = ResourceOptionFlags.None, 
            StagingBufferFlags stagingType = StagingBufferFlags.None, 
            uint structuredStride = 0, 
            Array initialData = null) : base(device)
        {
            _freeSegments = new List<BufferSegment>();
            _byteCapacity = byteCapacity;
            _mode = mode;
            _pendingChanges = new ThreadedQueue<IBufferOperation>();
            _structuredStride = structuredStride;

            if (mode == BufferMode.Immutable && initialData == null)
                throw new ArgumentNullException("Initial data cannot be null when buffer mode is Immutable.");

            _initialData = initialData;

            BuildDescription(bindFlags, optionFlags, stagingType);

            if (initialData != null)
            {
                EngineInterop.PinObject(initialData, (ptr) =>
                {
                    InitializeBuffer(ptr);
                });
            }
            else
            {
                InitializeBuffer(null);
            }
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

        private void BuildDescription(BindFlag flags, ResourceOptionFlags opFlags, StagingBufferFlags stageMode)
        {
            Description = new BufferDescription();
            Description.Usage = ResourceUsage.Default;
            Description.BindFlags = flags;
            Description.OptionFlags = opFlags;

            // Buffer mode.
            switch (_mode)
            {
                case BufferMode.Default:
                    Description.Usage = ResourceUsage.Default;
                    Description.CpuAccessFlags = CpuAccessFlags.None;
                    break;

                case BufferMode.DynamicDiscard:
                case BufferMode.DynamicRing:
                    Description.Usage = ResourceUsage.Dynamic;
                    Description.CpuAccessFlags = CpuAccessFlags.Write;
                    break;


                case BufferMode.Immutable:
                    Description.Usage = ResourceUsage.Immutable;
                    Description.CpuAccessFlags = CpuAccessFlags.None;
                    break;
            }

            // Staging mode
            if (stageMode != StagingBufferFlags.None)
            {
                Description.BindFlags = 0;
                Description.OptionFlags = ResourceOptionFlags.None;
                Description.Usage = ResourceUsage.Staging;
                Description.CpuAccessFlags = CpuAccessFlag.CpuAccessRead;
                Description.StructureByteStride = 0;

                if ((stageMode & StagingBufferFlags.Read) == StagingBufferFlags.Read)
                    Description.CpuAccessFlags = CpuAccessFlag.CpuAccessRead;

                if ((stageMode & StagingBufferFlags.Write) == StagingBufferFlags.Write)
                    Description.CpuAccessFlags = CpuAccessFlag.CpuAccessWrite;
            }
        }

        protected virtual void InitializeBuffer(IntPtr? initialDataPtr)
        {
            // Dispose of old static buffer
            if (Native != null)
            {
                Device.DeallocateVRAM(Native.Description.SizeInBytes);
                Native->Release();
                Native = null;
            }

            // Set correct buffer size.
            Description.SizeInBytes = _byteCapacity;

            // Ensure structured buffers get the stride info.
            if (Description.OptionFlags == ResourceOptionFlags.BufferStructured)
                Description.StructureByteStride = _structuredStride;

            if (initialDataPtr != null)
                Native = new ID3D11Buffer(Device.Native, initialDataPtr.Value, Description);
            else
                Native = new ID3D11Buffer(Device.Native, Description);

            Device.AllocateVRAM(Description.SizeInBytes);

            _firstSegment = Device.GetBufferSegment();
            _firstSegment.Buffer = this;
            _firstSegment.ByteOffset = 0;
            _firstSegment.ByteCount = _byteCapacity;
            _firstSegment.IsFree = true;

            _freeSegments.Add(_firstSegment);
        }

        protected virtual void OnValidateAllocationStride(uint stride)
        {
            if ((Description.BindFlags & BindFlag.BindUnorderedAccess) == BindFlag.BindUnorderedAccess)
            {
                if (stride != _structuredStride)
                    throw new GraphicsBufferException("Buffer is structured. Stride must match that of the structured buffer.");
            }
        }

        /// <summary>Copies all the data in the current <see cref="GraphicsBuffer"/> to the destination <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="pipe">The <see cref="PipeDX11"/> that will perform the copy.</param>
        /// <param name="destination">The <see cref="GraphicsBuffer"/> to copy to.</param>
        internal void CopyTo(PipeDX11 pipe, GraphicsBuffer destination)
        {
            if (destination.Description.SizeInBytes < Description.SizeInBytes)
                throw new Exception("The destination buffer is not large enough.");

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == ResourceUsage.Staging)
                ApplyChanges(pipe);

            ValidateCopyBufferUsage(destination);
            pipe.Context->CopyResource((ID3D11Resource*)Native, (ID3D11Resource*)destination.Native);
        }

        internal void CopyTo(PipeDX11 pipe, GraphicsBuffer destination, ResourceRegion sourceRegion, uint destByteOffset = 0)
        {
            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == ResourceUsage.Staging)
                ApplyChanges(pipe);

            ValidateCopyBufferUsage(destination);
            pipe.Context->CopySubresourceRegion(Native, 0, sourceRegion, destination.Native, 0, destByteOffset);
            pipe.Profiler.Current.CopySubresourceCount++;
        }

        private void ValidateCopyBufferUsage(GraphicsBuffer destination)
        {
            if (Description.Usage != ResourceUsage.Default && Description.Usage != ResourceUsage.Immutable)
                throw new Exception("The current buffer must have a usage flag of Default or Immutable. Only these flags allow the GPU read access for copying/reading data from the buffer.");

            if (destination.Description.Usage != ResourceUsage.Default)
                throw new Exception("The destination buffer must have a usage flag of Staging or Default. Only these two allow the GPU write access for copying/writing data to the destination.");
        }

        internal void Map(PipeDX11 pipe, uint byteOffset, uint dataSize, Action<GraphicsBuffer, DataStream> callback, GraphicsBuffer staging = null)
        {
            // Check buffer type.
            bool isDynamic = Description.Usage == ResourceUsage.Dynamic;
            bool isStaged = Description.Usage == ResourceUsage.Staging &&
                (Description.CpuAccessFlags & CpuAccessFlag.CpuAccessWrite) == CpuAccessFlag.CpuAccessWrite;

            DataStream mappedData;

            // Check if the buffer is a dynamic-writable
            if (isDynamic || isStaged)
            {
                switch (_mode)
                {
                    case BufferMode.DynamicDiscard:
                        pipe.Context.MapSubresource(Native, MapMode.WriteDiscard, 0, out mappedData);
                        mappedData.Position = byteOffset;
                        pipe.Profiler.Current.MapDiscardCount++;
                        break;

                    case BufferMode.DynamicRing:
                        // NOTE: D3D11_MAP_WRITE_NO_OVERWRITE is only valid on vertex and index buffers. 
                        // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476181(v=vs.85).aspx
                        if (HasFlags(BindFlag.BindVertexBuffer) || HasFlags(BindFlag.BindIndexBuffer))
                        {
                            if (_ringPos > 0 && _ringPos + dataSize < _byteCapacity)
                            {
                                pipe.Context.MapSubresource(Native, MapMode.WriteNoOverwrite, 0, out mappedData);
                                pipe.Profiler.Current.MapNoOverwriteCount++;
                                mappedData.Position = _ringPos;
                                _ringPos += dataSize;
                            }
                            else
                            {                                
                                pipe.Context.MapSubresource(Native, MapMode.WriteDiscard, 0, out mappedData);
                                pipe.Profiler.Current.MapDiscardCount++;
                                mappedData.Position = 0;
                                _ringPos = dataSize;
                            }
                        }
                        else
                        {
                            pipe.Context.MapSubresource(Native, MapMode.WriteDiscard, 0, out mappedData);
                            pipe.Profiler.Current.MapDiscardCount++;
                            mappedData.Position = byteOffset;
                        }
                        break;

                    default:
                        pipe.Context.MapSubresource(Native, MapMode.Write, 0, out mappedData);
                        pipe.Profiler.Current.MapWriteCount++;
                        break;
                }     
                
                callback(this, mappedData);
                pipe.Context.UnmapSubresource(Native, 0);
            }
            else
            {
                // Write to the provided staging buffer instead.
                if (staging == null)
                    throw new GraphicsBufferException("Staging buffer required. Non-dynamic/staged buffers require a staging buffer to 'set' data.");

                isDynamic = staging.Description.Usage == ResourceUsage.Dynamic;
                isStaged = staging.Description.Usage == ResourceUsage.Staging;

                if (!isDynamic && !isStaged)
                    throw new GraphicsBufferException("The provided staging buffer is invalid. Must be either dynamic or staged.");

                if (staging.Description.SizeInBytes < dataSize)
                    throw new GraphicsBufferException($"The provided staging buffer is not large enough ({staging.Description.SizeInBytes} bytes) to fit the provided data ({dataSize} bytes).");

                // Write updated data into buffer
                if (isDynamic) // Always discard staging buffer data, since the old data is no longer needed after it's been copied to it's target resource.
                {
                    pipe.Context.MapSubresource(staging.Native, MapMode.WriteDiscard, 0, out mappedData);
                    pipe.Profiler.Current.MapDiscardCount++;
                }
                else
                {
                    pipe.Context.MapSubresource(staging.Native, MapMode.Write, 0, out mappedData);
                    pipe.Profiler.Current.MapWriteCount++;
                }

                callback(staging, mappedData);
                pipe.Context.UnmapSubresource(staging.Native, 0);

                ResourceRegion stagingRegion = new ResourceRegion()
                {
                    Left = 0,
                    Right = dataSize,
                    Back = 1,
                    Bottom = 1,
                };

                pipe.Context.CopySubresourceRegion(staging.Native, 0, stagingRegion, Native, 0, byteOffset);
                pipe.Profiler.Current.CopySubresourceCount++;
            }
        }

        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        internal void Set<T>(PipeDX11 pipe, T[] data, uint startIndex, uint count, uint dataStride = 0, uint byteOffset = 0, StagingBuffer staging = null) 
            where T : struct
        {
            if (dataStride == 0)
                dataStride = (uint)Marshal.SizeOf<T>();

            uint writeOffset = startIndex * dataStride;
            uint dataSize = count * dataStride;

            Map(pipe, byteOffset, dataSize, (buffer, stream) =>
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

            if ((Description.CpuAccessFlags & CpuAccessFlag.CpuAccessRead) != CpuAccessFlag.CpuAccessRead)
                throw new InvalidOperationException("Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            //now set the structured variable's data
            DataStream stream = null;
            DataBox dataBox = pipe.Context.MapSubresource(Native, 0, MapMode.Read, 0, out stream);
            pipe.Profiler.Current.MapReadCount++;
            stream.Position = byteOffset;
            stream.ReadRange<T>(destination, readOffset, count);

            // Unmap
            pipe.Context.UnmapSubresource(Native, 0);
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
            return (Description.BindFlags & flag) == flag;
        }

        internal bool HasFlag(CpuAccessFlag flag)
        {
            return (Description.CpuAccessFlags & flag) == flag;
        }

        internal override void Refresh(PipeDX11 pipe, PipelineBindSlot<DeviceDX11, PipeDX11> slot)
        {
            ApplyChanges(pipe);
        }

        private protected override void OnPipelineDispose()
        {
            if (Native != null)
                Device.DeallocateVRAM(_byteCapacity);

            base.OnPipelineDispose();
        }

        internal virtual void CreateResources(int stride, int byteoffset, int elementCount)
        {
            if (HasFlags(BindFlag.BindShaderResource))
            {
                SRV = new ShaderResourceView(Device.Native, Native, new ShaderResourceViewDescription()
                {
                    Buffer = new ShaderResourceViewDescription.BufferResource()
                    {
                        ElementCount = elementCount,
                        FirstElement = byteoffset,
                    },
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                    Format = Format.FormatUnknown,
                });
            }

            if (HasFlags(BindFlag.BindUnorderedAccess))
            {
                UAV = new UnorderedAccessView(Device.Native, Native, new UnorderedAccessViewDescription()
                {
                    Format = Format.FormatUnknown,
                    Dimension = UnorderedAccessViewDimension.Buffer,
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        ElementCount = elementCount,
                        FirstElement = byteoffset / _structuredStride,
                        Flags = UnorderedAccessViewBufferFlags.None,
                    }
                });
            }
        }

        private void CreateResources(BufferSegment segment)
        {
            segment.SRV?.Dispose();
            segment.UAV?.Dispose();

            if (HasFlags(BindFlag.BindVertexBuffer))
                segment.Binding = new VertexBufferBinding(segment.Buffer, segment.Stride, segment.ByteOffset);

            if (HasFlags(BindFlag.BindShaderResource))
            {
                segment.SRV = new ShaderResourceView(Device.Native, Native, new ShaderResourceViewDescription()
                {
                    Buffer = new ShaderResourceViewDescription.BufferResource()
                    {
                        ElementCount = segment.ElementCount,
                        FirstElement = segment.ByteOffset,
                    },
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                    Format = Format.FormatUnknown,
                });
            }

            if (HasFlags(BindFlag.BindUnorderedAccess))
            {
                segment.UAV = new UnorderedAccessView(Device.Native, Native, new UnorderedAccessViewDescription()
                {
                    Format = Format.FormatUnknown,
                    Dimension = UnorderedAccessViewDimension.Buffer,
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        ElementCount = segment.ElementCount,
                        FirstElement = _structuredStride,
                        Flags = UnorderedAccessViewBufferFlags.None,
                    }
                });
            }
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
            segment.UAV?.Dispose();
            segment.SRV?.Dispose();
            CreateResources(segment);
            OnSegmentResized(segment, oldByteCount, newByteCount, oldElementCount, newElementCount);
        }

        protected virtual void OnSegmentResized(BufferSegment segment, uint oldByteCount, uint newByteCount, uint oldElementCount, uint newElementCount) { }

        /// <summary>Gets the buffer's first segment. This always exists.</summary>
        internal BufferSegment FirstSegment => _firstSegment;

        /// <summary>Gets the structured stride which <see cref="BufferSegment"/> instances must adhere to if they belong to the current <see cref="GraphicsBuffer"/>. 
        /// This is ignored and unused if the <see cref="GraphicsBuffer"/> does not carry the <see cref="ResourceOptionFlags.BufferStructured"/> flag.</summary>
        public uint StructuredStride => _structuredStride;

        /// <summary>Gets the capacity of a single section within the buffer, in bytes.</summary>
        public uint ByteCapacity => _byteCapacity;

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public BufferMode Mode => _mode;

        /// <summary>Gets the bind flags associated with the buffer.</summary>
        public BindFlag BindFlags => Description.BindFlags;

        /// <summary>Gets the underlying DirectX 11 buffer. </summary>
        internal ID3D11Buffer* Buffer => Native;

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        public ResourceOptionFlags ResourceFlags => Description.OptionFlags;

        /// <summary>
        /// Gets a value indicating whether the current buffer is a shader resource.
        /// </summary>
        public bool IsShaderResource => (Description.BindFlag & BindFlag.BindShaderResource) == BindFlag.BindShaderResource;

        /// <summary>
        /// Gets a value indicating whether the current buffer has unordered access.
        /// </summary>
        public bool IsUnorderedAccess => (Description.BindFlag & BindFlag.BindUnorderedAccess) == BindFlag.BindUnorderedAccess;
    }
}
