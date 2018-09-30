using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    using Collections;
    using Buffer = SharpDX.Direct3D11.Buffer;

    internal partial class GraphicsBuffer : PipelineShaderObject
    {
        protected int _byteCapacity;
        protected int _structuredStride = 0;
        protected Array _initialData;
        protected Buffer _buffer;
        
        BufferMode _mode;
        int _ringPos;

        internal BufferDescription Description;
        internal VertexBufferBinding VertexBinding;

        ThreadedQueue<IBufferOperation> _pendingChanges;
        BufferSegment _firstSegment;
        List<BufferSegment> _freeSegments;

        internal GraphicsBuffer(GraphicsDeviceDX11 device,
            BufferMode mode,
            BindFlags bindFlags,
            int byteCapacity,
            ResourceOptionFlags optionFlags = ResourceOptionFlags.None, 
            StagingBufferFlags stagingType = StagingBufferFlags.None, 
            int structuredStride = 0, 
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

        private void BuildDescription(BindFlags flags, ResourceOptionFlags opFlags, StagingBufferFlags stageMode)
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
                Description.BindFlags = BindFlags.None;
                Description.OptionFlags = ResourceOptionFlags.None;
                Description.Usage = ResourceUsage.Staging;
                Description.CpuAccessFlags = CpuAccessFlags.Read;
                Description.StructureByteStride = 0;

                if ((stageMode & StagingBufferFlags.Read) == StagingBufferFlags.Read)
                    Description.CpuAccessFlags = CpuAccessFlags.Read;

                if ((stageMode & StagingBufferFlags.Write) == StagingBufferFlags.Write)
                    Description.CpuAccessFlags = CpuAccessFlags.Write;
            }
        }

        protected virtual void InitializeBuffer(IntPtr? initialDataPtr)
        {
            // Dispose of old static buffer
            if (_buffer != null)
            {
                Device.DeallocateVRAM(_buffer.Description.SizeInBytes);
                _buffer.Dispose();
            }

            // Set correct buffer size.
            Description.SizeInBytes = _byteCapacity;

            // Ensure structured buffers get the stride info.
            if (Description.OptionFlags == ResourceOptionFlags.BufferStructured)
                Description.StructureByteStride = _structuredStride;

            if (initialDataPtr != null)
                _buffer = new Buffer(Device.D3d, initialDataPtr.Value, Description);
            else
                _buffer = new Buffer(Device.D3d, Description);

            Device.AllocateVRAM(Description.SizeInBytes);

            _firstSegment = Device.GetBufferSegment();
            _firstSegment.Parent = this;
            _firstSegment.ByteOffset = 0;
            _firstSegment.ByteCount = _byteCapacity;
            _firstSegment.IsFree = true;

            _freeSegments.Add(_firstSegment);
        }

        protected virtual void OnValidateAllocationStride(int stride)
        {
            if ((Description.BindFlags & BindFlags.UnorderedAccess) == BindFlags.UnorderedAccess)
            {
                if (stride != _structuredStride)
                    throw new GraphicsBufferException("Buffer is structured. Stride must match that of the structured buffer.");
            }
        }

        /// <summary>Copies all the data in the current <see cref="GraphicsBuffer"/> to the destination <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="pipe">The <see cref="GraphicsPipe"/> that will perform the copy.</param>
        /// <param name="destination">The <see cref="GraphicsBuffer"/> to copy to.</param>
        internal void CopyTo(GraphicsPipe pipe, GraphicsBuffer destination)
        {
            if (destination.Description.SizeInBytes < Description.SizeInBytes)
                throw new Exception("The destination buffer is not large enough.");

            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == ResourceUsage.Staging)
                ApplyChanges(pipe);

            ValidateCopyBufferUsage(destination);
            pipe.Context.CopyResource(_buffer, destination._buffer);
        }

        internal void CopyTo(GraphicsPipe pipe, GraphicsBuffer destination, ResourceRegion sourceRegion, int destByteOffset = 0)
        {
            // If the current buffer is a staging buffer, initialize and apply all its pending changes.
            if (Description.Usage == ResourceUsage.Staging)
                ApplyChanges(pipe);

            ValidateCopyBufferUsage(destination);
            pipe.Context.CopySubresourceRegion(_buffer, 0, sourceRegion, destination._buffer, 0, destByteOffset);
            pipe.Profiler.Current.CopySubresourceCount++;
        }

        private void ValidateCopyBufferUsage(GraphicsBuffer destination)
        {
            if (Description.Usage != ResourceUsage.Default && Description.Usage != ResourceUsage.Immutable)
                throw new Exception("The current buffer must have a usage flag of Default or Immutable. Only these flags allow the GPU read access for copying/reading data from the buffer.");

            if (destination.Description.Usage != ResourceUsage.Default)
                throw new Exception("The destination buffer must have a usage flag of Staging or Default. Only these two allow the GPU write access for copying/writing data to the destination.");
        }

        internal void Map(GraphicsPipe pipe, int byteOffset, int dataSize, Action<GraphicsBuffer, DataStream> callback, GraphicsBuffer staging = null)
        {
            // Check buffer type.
            bool isDynamic = Description.Usage == ResourceUsage.Dynamic;
            bool isStaged = Description.Usage == ResourceUsage.Staging &&
                (Description.CpuAccessFlags & CpuAccessFlags.Write) == CpuAccessFlags.Write;

            DataStream mappedData;

            // Check if the buffer is a dynamic-writable
            if (isDynamic || isStaged)
            {
                switch (_mode)
                {
                    case BufferMode.DynamicDiscard:
                        pipe.Context.MapSubresource(_buffer, MapMode.WriteDiscard, MapFlags.None, out mappedData);
                        mappedData.Position = byteOffset;
                        pipe.Profiler.Current.MapDiscardCount++;
                        break;

                    case BufferMode.DynamicRing:
                        // NOTE: D3D11_MAP_WRITE_NO_OVERWRITE is only valid on vertex and index buffers. 
                        // See: https://msdn.microsoft.com/en-us/library/windows/desktop/ff476181(v=vs.85).aspx
                        if (HasFlags(BindFlags.VertexBuffer) || HasFlags(BindFlags.IndexBuffer))
                        {
                            if (_ringPos > 0 && _ringPos + dataSize < _byteCapacity)
                            {
                                pipe.Context.MapSubresource(_buffer, MapMode.WriteNoOverwrite, MapFlags.None, out mappedData);
                                pipe.Profiler.Current.MapNoOverwriteCount++;
                                mappedData.Position = _ringPos;
                                _ringPos += dataSize;
                            }
                            else
                            {                                
                                pipe.Context.MapSubresource(_buffer, MapMode.WriteDiscard, MapFlags.None, out mappedData);
                                pipe.Profiler.Current.MapDiscardCount++;
                                mappedData.Position = 0;
                                _ringPos = dataSize;
                            }
                        }
                        else
                        {
                            pipe.Context.MapSubresource(_buffer, MapMode.WriteDiscard, MapFlags.None, out mappedData);
                            pipe.Profiler.Current.MapDiscardCount++;
                            mappedData.Position = byteOffset;
                        }
                        break;

                    default:
                        pipe.Context.MapSubresource(_buffer, MapMode.Write, MapFlags.None, out mappedData);
                        pipe.Profiler.Current.MapWriteCount++;
                        break;
                }     
                
                callback(this, mappedData);
                pipe.Context.UnmapSubresource(_buffer, 0);
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
                    pipe.Context.MapSubresource(staging._buffer, MapMode.WriteDiscard, MapFlags.None, out mappedData);
                    pipe.Profiler.Current.MapDiscardCount++;
                }
                else
                {
                    pipe.Context.MapSubresource(staging._buffer, MapMode.Write, MapFlags.None, out mappedData);
                    pipe.Profiler.Current.MapWriteCount++;
                }

                callback(staging, mappedData);
                pipe.Context.UnmapSubresource(staging._buffer, 0);

                ResourceRegion stagingRegion = new ResourceRegion()
                {
                    Left = 0,
                    Right = dataSize,
                    Back = 1,
                    Bottom = 1,
                };

                pipe.Context.CopySubresourceRegion(staging._buffer, 0, stagingRegion, _buffer, 0, byteOffset);
                pipe.Profiler.Current.CopySubresourceCount++;
            }
        }

        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        internal void Set<T>(GraphicsPipe pipe, T[] data, int startIndex, int count, int dataStride = 0, int byteOffset = 0, StagingBuffer staging = null) 
            where T : struct
        {
            if (dataStride == 0)
                dataStride = Marshal.SizeOf<T>();

            int writeOffset = startIndex * dataStride;
            int dataSize = count * dataStride;

            Map(pipe, byteOffset, dataSize, (buffer, stream) =>
            {
                stream.WriteRange(data, startIndex, count);
            }, staging);
        }

        /// <summary>Retrieves data from a <see cref="GraphicsBuffer"/>.</summary>
        /// <param name="pipe">The <see cref="GraphicsPipe"/> that will perform the 'get' operation.</param>
        /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
        /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
        /// <param name="count">The number of elements to retrieve</param>
        /// <param name="dataStride">The size of the data being retrieved. The default value is 0. 
        /// A value of 0 will force the stride of <see cref="{T}"/> to be automatically calculated, which may cause a tiny performance hit.</param>
        /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
        internal void Get<T>(GraphicsPipe pipe, T[] destination, int startIndex, int count, int dataStride, int byteOffset = 0)
            where T : struct
        {
            int readOffset = startIndex * dataStride;

            if ((Description.CpuAccessFlags & CpuAccessFlags.Read) != CpuAccessFlags.Read)
                throw new InvalidOperationException("Cannot use GetData() on a non-readable buffer.");

            if (destination.Length < count)
                throw new ArgumentException("The provided destination array is not large enough.");

            //now set the structured variable's data
            DataStream stream = null;
            DataBox dataBox = pipe.Context.MapSubresource(_buffer, 0, MapMode.Read, MapFlags.None, out stream);
            pipe.Profiler.Current.MapReadCount++;
            stream.Position = byteOffset;
            stream.ReadRange<T>(destination, readOffset, count);

            // Unmap
            pipe.Context.UnmapSubresource(_buffer, 0);
        }

        /// <summary>Applies any pending changes onto the buffer.</summary>
        /// <param name="pipe">The graphics pipe to use when process changes.</param>
        /// <param name="forceInitialize">If set to true, the buffer will be initialized if not done so already.</param>
        protected void ApplyChanges(GraphicsPipe pipe)
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

        internal bool HasFlags(BindFlags flag)
        {
            return (Description.BindFlags & flag) == flag;
        }

        internal bool HasFlag(CpuAccessFlags flag)
        {
            return (Description.CpuAccessFlags & flag) == flag;
        }

        internal override void Refresh(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            ApplyChanges(pipe);
        }

        private protected override void OnPipelineDispose()
        {
            if (_buffer != null)
                Device.DeallocateVRAM(_byteCapacity);

            base.OnPipelineDispose();
        }

        internal virtual void CreateResources(int stride, int byteoffset, int elementCount)
        {
            if (HasFlags(BindFlags.VertexBuffer))
                VertexBinding = new VertexBufferBinding(_buffer, stride, byteoffset);

            if (HasFlags(BindFlags.ShaderResource))
            {
                SRV = new ShaderResourceView(Device.D3d, _buffer, new ShaderResourceViewDescription()
                {
                    Buffer = new ShaderResourceViewDescription.BufferResource()
                    {
                        ElementCount = elementCount,
                        FirstElement = byteoffset,
                    },
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                    Format = SharpDX.DXGI.Format.Unknown,
                });
            }

            if (HasFlags(BindFlags.UnorderedAccess))
            {
                UAV = new UnorderedAccessView(Device.D3d, _buffer, new UnorderedAccessViewDescription()
                {
                    Format = SharpDX.DXGI.Format.Unknown,
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

            if (HasFlags(BindFlags.VertexBuffer))
                segment.VertexBinding = new VertexBufferBinding(segment.Buffer, segment.Stride, segment.ByteOffset);

            if (HasFlags(BindFlags.ShaderResource))
            {
                segment.SRV = new ShaderResourceView(Device.D3d, _buffer, new ShaderResourceViewDescription()
                {
                    Buffer = new ShaderResourceViewDescription.BufferResource()
                    {
                        ElementCount = segment.ElementCount,
                        FirstElement = segment.ByteOffset,
                    },
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Buffer,
                    Format = SharpDX.DXGI.Format.Unknown,
                });
            }

            if (HasFlags(BindFlags.UnorderedAccess))
            {
                segment.UAV = new UnorderedAccessView(Device.D3d, _buffer, new UnorderedAccessViewDescription()
                {
                    Format = SharpDX.DXGI.Format.Unknown,
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

        internal BufferSegment Allocate<T>(int count)
where T : struct
        {
            Type allocatedType = typeof(T);
            int stride = Marshal.SizeOf(allocatedType);
            OnValidateAllocationStride(stride);

            BufferSegment seg;
            int byteCount = count * stride;

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
        internal BufferSegment UpdateAllocation<T>(BufferSegment existing, int count) where T : struct
        {
            int newStride = Marshal.SizeOf(typeof(T));
            OnValidateAllocationStride(newStride);

            int oldBytes = existing.ByteCount;
            int newBytes = newStride * count;
            int oldCount = existing.ElementCount;

            if (oldBytes == newBytes)
            {
                return existing;
            }
            else if (oldBytes > newBytes) // Downsize
            {
                int dif = oldBytes - newBytes;

                BufferSegment freed = existing.SplitFromFront(dif);
                freed.IsFree = true;
                existing.Stride = newStride;
                existing.ElementCount = count;
                _freeSegments.Add(freed);
            }
            else if (newBytes > oldBytes) // Upsize
            {
                int dif = newBytes - oldBytes;
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
                    int oldOffset = existing.ByteOffset;
                    existing.ElementCount = count;
                    existing.Stride = newStride;
                    existing.TakeFromPrevious(dif);

                    // TODO queue copy change to move the data backward to the new byte offset.
                }
                else
                {
                    if ((canMergeNext && canMergePrev) && ((existing.Previous.ByteCount + existing.Next.ByteCount) >= dif))
                    {
                        int oldOffset = existing.ByteOffset;
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

        private void SegmentResized(BufferSegment segment, int oldByteCount, int newByteCount, int oldElementCount, int newElementCount)
        {
            segment.UAV?.Dispose();
            segment.SRV?.Dispose();
            CreateResources(segment);
            OnSegmentResized(segment, oldByteCount, newByteCount, oldElementCount, newElementCount);
        }

        protected virtual void OnSegmentResized(BufferSegment segment, int oldByteCount, int newByteCount, int oldElementCount, int newElementCount) { }

        /// <summary>Gets the buffer's first segment. This always exists.</summary>
        internal BufferSegment FirstSegment => _firstSegment;

        /// <summary>Gets the structured stride which <see cref="BufferSegment"/> instances must adhere to if they belong to the current <see cref="GraphicsBuffer"/>. 
        /// This is ignored and unused if the <see cref="GraphicsBuffer"/> does not carry the <see cref="ResourceOptionFlags.BufferStructured"/> flag.</summary>
        public int StructuredStride => _structuredStride;

        /// <summary>Gets the capacity of a single section within the buffer, in bytes.</summary>
        public int ByteCapacity => _byteCapacity;

        /// <summary>Gets the flags that were passed in to the buffer when it was created.</summary>
        public BufferMode Mode => _mode;

        /// <summary>Gets the bind flags associated with the buffer.</summary>
        public BindFlags BindFlags => Description.BindFlags;

        /// <summary>Gets the underlying DirectX 11 buffer. </summary>
        internal Buffer Buffer => _buffer;

        /// <summary>Gets the resource usage flags associated with the buffer.</summary>
        public ResourceOptionFlags ResourceFlags => Description.OptionFlags;

        /// <summary>
        /// Gets a value indicating whether the current buffer is a shader resource.
        /// </summary>
        public bool IsShaderResource => (Description.BindFlags & BindFlags.ShaderResource) == BindFlags.ShaderResource;

        /// <summary>
        /// Gets a value indicating whether the current buffer has unordered access.
        /// </summary>
        public bool IsUnorderedAccess => (Description.BindFlags & BindFlags.UnorderedAccess) == BindFlags.UnorderedAccess;
    }
}
