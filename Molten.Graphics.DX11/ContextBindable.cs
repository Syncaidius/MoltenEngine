using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a DX11 bindable pipeline object.
    /// </summary>
    public abstract class ContextBindable : ContextObject
    {
        internal ContextBindable(Device device, ContextBindTypeFlags bindFlags) : base(device)
        {
            BoundTo = new HashSet<ContextSlot>();
            BindFlags = bindFlags;
        }

        /// <summary>
        /// Invoked right before the current <see cref="ContextBindable"/> is due to be bound to a <see cref="DeviceContext"/>.
        /// </summary>
        /// <param name="slot">The <see cref="PipeSlot"/> which contains the current <see cref="ContextBindable"/>.</param>
        /// <param name="pipe">The <see cref="DeviceContext"/> that the current <see cref="ContextBindable"/> is to be bound to.</param>
        internal abstract void Refresh(ContextSlot slot, DeviceContext pipe);

        /// <summary>
        /// Gets the instance-specific version of the current <see cref="ContextBindable"/>. Any change which will require a device
        /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
        /// </summary>
        internal protected uint Version { get; protected set; }

        /// <summary>
        /// Gets a list of slots that the current <see cref="ContextBindable"/> is bound to.
        /// </summary>
        internal HashSet<ContextSlot> BoundTo { get; }

        /// <summary>
        /// Gets the current binding ID.
        /// </summary>
        internal uint BindID { get; set; }

        internal ContextBindTypeFlags BindFlags { get; set; }
    }

    public unsafe abstract class PipeBindable<T> : ContextBindable
        where T : unmanaged
    {
        internal PipeBindable(Device device, ContextBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {

        }

        /// <summary>
        /// Gets the native pointer of the current <see cref="PipeBindable{T}"/>, as a <typeparamref name="T"/> pointer.
        /// </summary>
        internal abstract T* NativePtr { get; }

        /// <summary>
        /// Gets the native pointer of the current <see cref="PipeBindable{T}"/>,as a <see cref="void"/> pointer.
        /// </summary>
        internal void* RawNative => NativePtr;

        public static implicit operator T*(PipeBindable<T> bindable)
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
