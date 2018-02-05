using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.D3DCompiler;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    internal class StagingBuffer : GraphicsBuffer
    {
        StagingBufferFlags _stageType;

        /// <summary>Creates a new instance of <see cref="StagingBuffer"/>.</summary>
        /// <param name="device">The graphics device to bind the buffer to.</param>
        /// <param name="stride">The expected size of 1 element, in bytes.</param>
        /// <param name="capacity">The number of elements the buffer should be able to hold.</param>
        internal StagingBuffer(GraphicsDevice device, StagingBufferFlags bufferType, int capacity)
            : base(device, BufferMode.Default, BindFlags.None, capacity, ResourceOptionFlags.None, bufferType)
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
