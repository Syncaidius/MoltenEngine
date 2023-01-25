using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;

namespace Molten.Graphics
{
    public abstract class GraphicsObject : EngineObject
    {
        protected GraphicsObject(GraphicsDevice device, GraphicsBindTypeFlags bindFlags)
        {
            Device = device; 
            BoundTo = new List<GraphicsSlot>();
            BindFlags = bindFlags;
            LastUsedFrameID = device.Cmd.Profiler.FrameID;
        }

        /// <summary>
        /// Invoked when the current <see cref="GraphicsObject"/> should apply any changes before being bound to a GPU context.
        /// </summary>
        /// <param name="cmd">The <see cref="GraphicsCommandQueue"/> that the current <see cref="GraphicsObject"/> is to be bound to.</param>
        public void Apply(GraphicsCommandQueue cmd)
        {
            LastUsedFrameID = cmd.Profiler.FrameID;
            OnApply(cmd);
        }

        protected abstract void OnApply(GraphicsCommandQueue context);

        protected override void OnDispose()
        {
            Device.MarkForRelease(this);
        }

        public abstract void GraphicsRelease();

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> that the current <see cref="GraphicsObject"/> is bound to.
        /// </summary>
        public GraphicsDevice Device { get; }

        /// <summary>
        /// Gets the instance-specific version of the current <see cref="GraphicsObject"/>. Any change which will require a device
        /// update should increase this value. E.g. Resizing a texture, recompiling a shader/material, etc.
        /// </summary>
        internal protected uint Version { get; protected set; }

        /// <summary>
        /// Gets a list of slots that the current <see cref="GraphicsObject"/> is bound to.
        /// </summary>
        internal List<GraphicsSlot> BoundTo { get; }

        /// <summary>
        /// Gets the current binding ID.
        /// </summary>
        internal uint BindID { get; set; }

        /// <summary>
        /// Gets or sets the slot bind type of the current <see cref="GraphicsObject"/>.
        /// </summary>
        public GraphicsBindTypeFlags BindFlags { get; set; }

        /// <summary>
        /// Gets the ID of the frame that the current <see cref="GraphicsObject"/> was applied.
        /// </summary>
        internal uint LastUsedFrameID { get; private set; }
    }

    public unsafe abstract class GraphicsObject<T> : GraphicsObject
    where T : unmanaged
    {
        protected GraphicsObject(GraphicsDevice device, GraphicsBindTypeFlags bindFlags) :
            base(device, bindFlags)
        {

        }

        /// <summary>
        /// Gets the native pointer of the current <see cref="GraphicsObject{T}"/>, as a <typeparamref name="T"/> pointer.
        /// </summary>
        public abstract T* NativePtr { get; }

        /// <summary>
        /// Gets the native pointer of the current <see cref="GraphicsObject{T}"/>,as a <see cref="void"/> pointer.
        /// </summary>
        public void* RawNative => NativePtr;


        public static implicit operator T*(GraphicsObject<T> bindable)
        {
            return bindable.NativePtr;
        }


        /// <summary>Queries the underlying texture's interface.</summary>
        /// <typeparam name="Q">The type of object to request in the query.</typeparam>
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
