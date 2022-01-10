using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class StagingBuffer : GraphicsBuffer
    {
        StagingBufferFlags _stageType;

        /// <summary>Creates a new instance of <see cref="StagingBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stride">The expected size of 1 element, in bytes.</param>
        /// <param name="capacity">The number of elements the buffer should be able to hold.</param>
        internal StagingBuffer(DeviceDX11 device, StagingBufferFlags bufferType, uint capacity)
            : base(device, BufferMode.Default, 0, capacity, 0, bufferType)
        {
            _stageType = bufferType;
        }

        /// <summary>Gets the staging mode of the buffer.</summary>
        public StagingBufferFlags StagingType
        {
            get { return _stageType; }
        }
    }
}
