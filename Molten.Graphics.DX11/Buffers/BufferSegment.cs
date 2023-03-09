using System.Runtime.InteropServices;
using Molten.Collections;
using Molten.IO;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class BufferSegment : ContextBindableResource<ID3D11Buffer>, IPoolable, IGraphicsBufferSegment
    {
        /// <summary>The previous segment (if any) within the mapped buffer.</summary>
        internal BufferSegment Previous;

        /// <summary>The next segment (if any) within the mapped buffer.</summary>
        internal BufferSegment Next;

        internal override ID3D11Buffer* ResourcePtr => _buffer.ResourcePtr;

        public override unsafe ID3D11Resource* NativePtr => _buffer.NativePtr;

        internal Format DataFormat;

        /// <summary>If true, the segment is not used.</summary>
        internal bool IsFree;

        GraphicsBuffer _buffer;

        internal BufferSegment(DeviceDX11 device) : base(device, GraphicsBindTypeFlags.None) { }

        public void SetVertexFormat<T>() where T: struct, IVertexType
        {
            VertexFormat = (Buffer.Device as DeviceDX11).VertexFormatCache.Get<T>();
        }

        internal void SetVertexFormat(Type vertexType)
        {
            VertexFormat = (Buffer.Device as DeviceDX11).VertexFormatCache.Get(vertexType);
        }

        public void SetIndexFormat(IndexBufferFormat format)
        {
            switch (format)
            {
                case IndexBufferFormat.Unsigned32Bit:
                    DataFormat = Format.FormatR32Uint;
                    break;

                case IndexBufferFormat.Unsigned16Bit:
                    DataFormat = Format.FormatR16Uint;
                    break;
            }
        }

        /// <summary>Copies element data into the buffer.</summary>
        /// <param name="data">The source of elements to copy into the buffer.</param>
        /// <param name="completionCallback">The callback to invoke when the set-data operation has been completed.</param>
        public void SetData<T>(GraphicsPriority priority, T[] data, Action completionCallback = null) where T : unmanaged
        {
            SetData(priority, data, 0, (uint)data.Length, 0, null, completionCallback);
        }

        /// <summary>Copies element data into the buffer.</summary>
        /// <param name="data">The source of elements to copy into the buffer.</param>
        /// <param name="count">The number of elements to copy from the source array, beginning at the start index.</param>
        /// <param name="completionCallback">The callback to invoke when the set-data operation has been completed.</param>
        public void SetData<T>(GraphicsPriority priority, T[] data, uint count, Action completionCallback = null)
            where T : unmanaged
        {
            SetData(priority, data, 0, count, 0, null, completionCallback);
        }

        /// <summary>Copies element data into the buffer.</summary>
        /// <param name="data">The source of elements to copy into the buffer.</param>
        /// <param name="startIndex">The index in the data array at which to start copying from.</param>
        /// <param name="count">The number of elements to copy from the source array, beginning at the start index.</param>
        /// <param name="elementOffset">The number of elements from the beginning of the <see cref="BufferSegment"/> to offset the destination of the provided data.
        /// The number of bytes the data is offset is based on the <see cref="Stride"/> value of the buffer segment.</param>
        /// <param name="completionCallback">The callback to invoke when the set-data operation has been completed.</param>
        public void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint count, uint elementOffset = 0, IStagingBuffer staging = null, Action completionCallback = null) 
            where T : unmanaged
        {
            uint tStride = (uint)sizeof(T);
            uint dataSize = tStride * count;
            uint writeOffset = elementOffset * tStride;
            uint finalBufferPos = dataSize + writeOffset + ByteOffset;
            uint segmentBounds = ByteOffset + ByteCount;

            // Ensure the buffer can fit the provided data.
            if (finalBufferPos > segmentBounds)
                throw new OverflowException($"Provided data's final byte position {finalBufferPos} would exceed the segment's bounds (byte {segmentBounds})");

            BufferSetOperation<T> op = new BufferSetOperation<T>()
            {
                DataStride = tStride,
                ByteOffset = ByteOffset + writeOffset,
                StartIndex = startIndex,
                Count = count,
                DestinationSegment = this,
                CompletionCallback = completionCallback,
                Staging = staging,
            };

            if(priority == GraphicsPriority.Immediate)
            {
                op.Data = data;
                op.Process(Device.Cmd);
            }
            else
            {
                // Clone the data for deferred operation.
                op.Data = new T[count];
                Array.Copy(data, startIndex, op.Data, 0, count);
                _buffer.QueueOperation(op);
            }
        }

        public void GetStream(GraphicsPriority priority, Action<IGraphicsBuffer, RawStream> callback, IStagingBuffer staging = null)
        {
            uint curByteOffset = ByteOffset;
            uint curStride = Stride;

            if (priority == GraphicsPriority.Immediate)
            {
                _buffer.GetStream(Device.Cmd as CommandQueueDX11, ByteOffset, Stride * ElementCount, (buffer, stream) =>
                {
                    if (Buffer.Mode == BufferMode.DynamicRing)
                        ByteOffset = (uint)stream.Position;

                    callback(buffer, stream);
                }, staging as StagingBuffer);
            }
            else
            {
                _buffer.QueueOperation(new BufferGetStreamOperation()
                {
                    Segment = this,
                    StreamCallback = callback
                });
            }

            // Do we need to update version due to data changes?
            if (curByteOffset != ByteOffset || curStride != Stride)
                Version++;
        }

        public void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint elementOffset, Action<T[]> completionCallback)
            where T : unmanaged
        {
            uint tStride = (uint)sizeof(T);
            uint writeOffset = elementOffset * tStride;

            BufferGetOperation<T> op = new BufferGetOperation<T>()
            {
                ByteOffset = ByteOffset + writeOffset,
                DestinationArray = destination,
                DestinationIndex = startIndex,
                Count = count,
                DataStride = (uint)sizeof(T),
                CompletionCallback = completionCallback,
                SourceSegment = this,
            };

            if (priority == GraphicsPriority.Immediate)
                op.Process(Device.Cmd);
            else
                _buffer.QueueOperation(op);
        }

        internal void CopyTo(CommandQueueDX11 cmd, uint sourceByteOffset, BufferSegment destination, uint destByteOffset, uint count, bool isImmediate = false, Action completionCallback = null)
        {
            uint bytesToCopy = Stride * count;
            uint totalOffset = ByteOffset + sourceByteOffset;
            uint lastByte = totalOffset + bytesToCopy;

            uint destLastByte = destination.ByteOffset + destination.ByteCount;
            if (lastByte > destLastByte)
                throw new OverflowException("specified copy region would exceed the bounds of the destination segment.");

            Box sourceRegion = new Box()
            {
                Left = totalOffset,
                Right = lastByte,
                Back = 1,
                Bottom = 1,
            };

            if (isImmediate)
            {
                _buffer.CopyTo(cmd, destination._buffer, sourceRegion, destination.ByteOffset + destByteOffset);
            }
            else
            {
                _buffer.QueueOperation(new BufferCopyOperation()
                {
                    CompletionCallback = completionCallback,
                    SourceBuffer = _buffer,
                    DestinationBuffer = destination._buffer,
                    DataStride = Stride,
                    DestinationByteOffset = destination.ByteOffset + destByteOffset,
                    SourceRegion = sourceRegion,
                });
            }
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            Buffer.Apply(cmd);
        }

        /// <summary>Releases the buffer space reserved by the segment.</summary>
        public void Release()
        {
            UAV.Release();
            SRV.Release();
            Buffer.Deallocate(this);
        }

        public override void GraphicsRelease()
        {
            Release();
            IsDisposed = false;

            base.OnDispose();
        }

        /// <summary>Clears segment's internal data.</summary>
        public void ClearForPool()
        {
            Previous = null;
            Next = null;
            VertexFormat = null;
            Version = 0;
            SetIndexFormat(IndexBufferFormat.Unsigned32Bit);

            UAV.Release();
            SRV.Release();
        }

        /// <summary>Sets the next segment to the one specified and also sets it's previous to the current segment.</summary>
        /// <param name="next">The next segment.</param>
        internal void LinkNext(BufferSegment next)
        {
            Next = next;
            if(next != null)
                next.Previous = this;
        }

        /// <summary>Sets the previous segment to the one specified and also sets it's next to the current segment.</summary>
        /// <param name="previous">The previous segment.</param>
        internal void LinkPrevious(BufferSegment previous)
        {
            Previous = previous;
            if(previous != null)
                previous.Next = this;
        }

        /// <summary>Take's bytes off the previous segment and adds them to the current.</summary>
        /// <param name="bytesToTake">The bytes to take.</param>
        internal void TakeFromPrevious(uint bytesToTake)
        {
            ByteOffset -= bytesToTake;
            ByteCount += bytesToTake;
            Previous.ByteCount -= bytesToTake;

            // If all of the previous was consumed, recycle it.
            if(Previous.ByteCount == 0)
            {
                LinkPrevious(Previous.Previous);
                (Device as DeviceDX11).RecycleBufferSegment(Previous);
            }
        }

        /// <summary>Decreases the size of the current segment by the specified amount of bytes, from the front, then adds  it to the next segment's back.</summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="bytesToTake">The bytes to take.</param>
        internal void TakeFromNext(uint bytesToTake)
        {
            ByteCount += bytesToTake;
            Next.ByteOffset += bytesToTake;
            Next.ByteCount -= bytesToTake;

            // If all of the previous was consumed, recycle it.
            if (Next.ByteCount == 0)
            {
                LinkNext(Next.Next);
                (Device as DeviceDX11).RecycleBufferSegment(Next);
            }
        }

        internal BufferSegment SplitFromBack(uint bytesToTake)
        {
            BufferSegment seg = (Device as DeviceDX11).GetBufferSegment();
            seg.LinkNext(this);
            seg.LinkPrevious(Previous);

            seg.BindFlags = BindFlags;
            seg.ByteCount = bytesToTake;
            seg.ByteOffset = ByteOffset;
            seg.Buffer = Buffer;

            ByteOffset += bytesToTake;
            ByteCount -= bytesToTake;

            return seg;
        }

        internal BufferSegment SplitFromFront(uint bytesToTake)
        {
            BufferSegment seg = (Device as DeviceDX11).GetBufferSegment();
            seg.LinkNext(Next);
            seg.LinkPrevious(this);

            ByteCount -= bytesToTake;
            seg.BindFlags = BindFlags;
            seg.ByteCount = bytesToTake;
            seg.ByteOffset = ByteOffset + ByteCount;
            seg.Buffer = Buffer;
            return seg;
        }

        /// <summary>
        /// The <see cref="GraphicsBuffer"/> that contains the current <see cref="BufferSegment"/>
        /// </summary>
        public IGraphicsBuffer Buffer
        {
            get => _buffer;
            internal set => _buffer = value as GraphicsBuffer;
        }

        /// <summary>The size of the segment in bytes. This is <see cref="ElementCount"/> multiplied by <see cref="Stride"/>.</summary>
        public uint ByteCount { get; internal set; }

        /// <summary>The number of elements that the segment can hold.</summary>
        public uint ElementCount { get; internal set; }

        /// <summary>
        /// The size of each element within the buffer, in bytes.
        /// </summary>
        public uint Stride { get; internal set; }

        /// <summary>
        /// The byte offset within the <see cref="Buffer"/> <see cref="GraphicsBuffer"/>.
        /// </summary>
        public uint ByteOffset { get; internal set; }

        public VertexFormat VertexFormat { get; internal set; }
    }
}
