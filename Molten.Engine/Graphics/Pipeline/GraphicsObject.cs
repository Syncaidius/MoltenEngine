using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsObject : EngineObject
    {
        protected GraphicsObject(GraphicsDevice device)
        {
            Device = device;
        }

        protected override void OnDispose()
        {
            Device.MarkForRelease(this);
        }

        public abstract void GraphicsRelease();

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> that the current <see cref="GraphicsObject"/> is bound to.
        /// </summary>
        public GraphicsDevice Device { get; }
    }

    public abstract class GraphicsObject<D> : GraphicsObject
        where D : GraphicsDevice
    {
        protected GraphicsObject(D device) : base(device)
        {
            NativeDevice = device;
        }

        /// <summary>
        /// Gets the platform or API-specific implementation of <see cref="GraphicsDevice"/>.
        /// </summary>
        public D NativeDevice { get; }

    }
}
