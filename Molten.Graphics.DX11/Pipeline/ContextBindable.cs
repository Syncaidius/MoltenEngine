using Silk.NET.Core.Native;
using System.Reflection;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a DX11 bindable pipeline object.
    /// </summary>
    public abstract class ContextBindable : GraphicsObject
    {
        internal ContextBindable(DeviceDX11 device, GraphicsBindTypeFlags bindFlags) : base(device)
        {
            BoundTo = new List<ContextSlot>();
            BindFlags = bindFlags;
            LastUsedFrameID = device.Cmd.Profiler.FrameID;
            NativeDevice = device;
        }

        /// <summary>
        /// Invoked when the current <see cref="ContextBindable"/> should apply any changes before being bound to a GPU context.
        /// </summary>
        /// <param name="cmd">The <see cref="CommandQueueDX11"/> that the current <see cref="ContextBindable"/> is to be bound to.</param>
        internal void Apply(CommandQueueDX11 cmd)
        {
            LastUsedFrameID = cmd.Profiler.FrameID;
            OnApply(cmd);
        }

        protected abstract void OnApply(CommandQueueDX11 context);

        /// <summary>
        /// Gets the instance-specific version of the current <see cref="ContextBindable"/>. Any change which will require a device
        /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
        /// </summary>
        internal protected uint Version { get; protected set; }

        /// <summary>
        /// Gets a list of slots that the current <see cref="ContextBindable"/> is bound to.
        /// </summary>
        internal List<ContextSlot> BoundTo { get; }

        /// <summary>
        /// Gets the current binding ID.
        /// </summary>
        internal uint BindID { get; set; }

        internal GraphicsBindTypeFlags BindFlags { get; set; }

        /// <summary>
        /// Gets the ID of the frame that the current <see cref="ContextBindable"/> was applied.
        /// </summary>
        internal uint LastUsedFrameID { get; private set; }

        internal DeviceDX11 NativeDevice { get; }
    }

    public unsafe abstract class ContextBindable<T> : ContextBindable
        where T : unmanaged
    {
        internal ContextBindable(DeviceDX11 device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {

        }

        /// <summary>
        /// Gets the native pointer of the current <see cref="ContextBindable{T}"/>, as a <typeparamref name="T"/> pointer.
        /// </summary>
        internal abstract T* NativePtr { get; }

        /// <summary>
        /// Gets the native pointer of the current <see cref="ContextBindable{T}"/>,as a <see cref="void"/> pointer.
        /// </summary>
        internal void* RawNative => NativePtr;


        public static implicit operator T*(ContextBindable<T> bindable)
        {
            return bindable.NativePtr;
        }

        /// <summary>Queries the underlying texture's interface.</summary>
        /// <typeparam name="T">The type of object to request in the query.</typeparam>
        /// <returns></returns>
        public Q* QueryInterface<Q>() where Q : unmanaged
        {
            if (NativePtr != null)
            {
                IUnknown* ptr = (IUnknown*)RawNative;
                Type t = typeof(Q);
                FieldInfo mInfo = t.GetField("Guid");

                if (mInfo == null)
                    throw new Exception("");

                void* result = null;
                Guid guid = (Guid)mInfo.GetValue(null);
                ptr->QueryInterface(&guid, &result);
                return (Q*)result;
            }

            return null;
        }
    }
}
