using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;
using SharpDX.Direct3D11;

namespace Molten.Graphics
{
    internal class SegmentedBuffer : GraphicsBuffer
    {
        BufferSegment _firstSegment;
        List<BufferSegment> _freeSegments;

        internal static ObjectPool<BufferSegment> SegmentPool = new ObjectPool<BufferSegment>(() => new BufferSegment());

        internal SegmentedBuffer(GraphicsDevice device, 
            BufferMode mode, 
            BindFlags bindFlags, 
            int byteCapacity, 
            ResourceOptionFlags optionFlags = ResourceOptionFlags.None, 
            StagingBufferFlags stagingType = StagingBufferFlags.None, 
            int structuredStride = 0, 
            Array initialData = null) :
            base(device, mode, bindFlags, byteCapacity, optionFlags, stagingType, structuredStride, initialData)
        {
            
        }

        protected override void InitializeBuffer(IntPtr? initialDataPtr)
        {
            base.InitializeBuffer(initialDataPtr);

            _freeSegments = new List<BufferSegment>();
            _firstSegment = new BufferSegment()
            {
                Parent = this,
                ByteOffset = 0,
                ByteCount = _byteCapacity,
                IsFree = true,
            };

            _freeSegments.Add(_firstSegment);
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

                        CreateSegmentUav(newSeg);
                        SegmentAllocated(newSeg, allocatedType);
                        return newSeg;
                    }
                    else if (seg.ElementCount == byteCount) // Perfect. Use it without any resizing.
                    {
                        seg.IsFree = false;
                        seg.ElementCount = count;
                        seg.Stride = stride;

                        _freeSegments.Remove(seg);

                        CreateSegmentUav(seg);
                        SegmentAllocated(seg, allocatedType);
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
                SegmentPool.Recycle(segment.Next);
                SegmentPool.Recycle(segment);
            }
            else if (mergePrev)
            {
                segment.Previous.ByteCount += segment.ByteCount;
                segment.Previous.LinkNext(segment.Next);
                SegmentPool.Recycle(segment);
            }
            else if (mergeNext)
            {
                segment.ByteCount += segment.Next.ElementCount;
                segment.LinkNext(segment.Next.Next);
                segment.IsFree = true;

                SegmentPool.Recycle(segment.Next);
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

        private void SegmentAllocated(BufferSegment segment, Type allocatedType)
        {
            CreateResources(segment);
            OnSegmentAllocated(segment, allocatedType);
        }

        private void SegmentDeallocated(BufferSegment segment)
        {
            OnSegmentDeallocated(segment);
        }

        private void SegmentResized(BufferSegment segment, int oldByteCount, int newByteCount, int oldElementCount, int newElementCount)
        {
            segment.UAV?.Dispose();
            segment.SRV?.Dispose();
            CreateResources(segment);
            OnSegmentResized(segment, oldByteCount, newByteCount, oldElementCount, newElementCount);
        }

        private void CreateResources(BufferSegment segment)
        {
            if (HasFlags(BindFlags.VertexBuffer))
                segment.VertexBinding = new VertexBufferBinding(segment.Buffer, segment.Stride, segment.ByteOffset);

            if (HasFlags(BindFlags.ShaderResource))
            {
                segment.SRV = new ShaderResourceView(_device.D3d, _buffer, new ShaderResourceViewDescription()
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
                segment.UAV = new UnorderedAccessView(_device.D3d, _buffer, new UnorderedAccessViewDescription()
                {
                    Format = SharpDX.DXGI.Format.Unknown,
                    Dimension = UnorderedAccessViewDimension.Buffer,
                    Buffer = new UnorderedAccessViewDescription.BufferResource()
                    {
                        ElementCount = segment.ElementCount,
                        FirstElement = segment.ByteOffset / segment.Parent.StructuredStride,
                        Flags = UnorderedAccessViewBufferFlags.None,
                    }
                });
            }
        }

        protected virtual void OnSegmentAllocated(BufferSegment segment, Type allocatedType) { }

        protected virtual void OnSegmentDeallocated(BufferSegment segment) { }

        protected virtual void OnSegmentResized(BufferSegment segment, int oldByteCount, int newByteCount, int oldElementCount, int newElementCount) { }        
        
        /// <summary>Gets the buffer's first segment. This always exists.</summary>
        internal BufferSegment FirstSegment => _firstSegment;
    }
}
