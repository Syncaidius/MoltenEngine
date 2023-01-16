using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsDevice : EngineObject
    {
        /// <summary>
        /// Creates a new instance of <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="settings">The <see cref="GraphicsSettings"/> to bind to the device.</param>
        /// <param name="log">The <see cref="Logger"/> to use for outputting information.</param>
        protected GraphicsDevice(GraphicsSettings settings, Logger log)
        {
            Settings = settings;
            Log = log;
        }

        long _allocatedVRAM;

        /// <summary>Track a VRAM allocation.</summary>
        /// <param name="bytes">The number of bytes that were allocated.</param>
        public void AllocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, bytes);
        }

        /// <summary>Track a VRAM deallocation.</summary>
        /// <param name="bytes">The number of bytes that were deallocated.</param>
        public void DeallocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, -bytes);
        }

        /// <summary>
        /// Gets the amount of VRAM that has been allocated on the current <see cref="GraphicsDevice"/>. 
        /// <para>For a software or integration device, this may be system memory (RAM).</para>
        /// </summary>
        internal long AllocatedVRAM => _allocatedVRAM;

        /// <summary>
        /// Gets the <see cref="Logger"/> that is bound to the current <see cref="GraphicsDevice"/> for outputting information.
        /// </summary>
        public Logger Log { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsSettings"/> bound to the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsSettings Settings { get; }

        /// <summary>
        /// Gets the <see cref="IDisplayAdapter"/> that the current <see cref="GraphicsDevice"/> is bound to.
        /// </summary>
        public abstract IDisplayAdapter Adapter { get; }
    }

    /// <summary>
    /// A more advanced version of <see cref="GraphicsDevice"/> which manages the allocation and releasing of an unsafe object pointer, exposed via <see cref="Ptr"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe abstract class GraphicsDevice<T> : GraphicsDevice
        where T : unmanaged
    {
        T* _ptr;

        protected GraphicsDevice(GraphicsSettings settings, Logger log, bool allocate) :
            base(settings, log)
        {
            if (allocate)
                _ptr = EngineUtil.Alloc<T>();
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(GraphicsDevice<T> device)
        {
            return *device.Ptr;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(GraphicsDevice<T> device)
        {
            return device._ptr;
        }

        /// <summary>
        /// The underlying, native device pointer.
        /// </summary>
        public T* Ptr => _ptr;

        /// <summary>
        /// Gets a protected reference to the underlying device pointer.
        /// </summary>
        protected ref T* PtrRef => ref _ptr;
    }
}
