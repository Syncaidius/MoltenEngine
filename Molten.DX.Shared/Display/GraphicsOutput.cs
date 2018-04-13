using SharpDX;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A display device usually attached to a <see cref="GraphicsAdapter"/></summary>
    /// <seealso cref="Molten.IDisplayOutput" />
    /// <typeparam name="A">The DXGI Adapter type</typeparam>
    /// <typeparam name="D">The type of adapter description</typeparam>
    public abstract class GraphicsOutput : IDisplayOutput
    {
        IDisplayAdapter _adapter;

        internal GraphicsOutput(IDisplayAdapter adapter)
        {
            _adapter = adapter;
        }

        /// <summary>Gets the name of the output.</summary>
        public string Name { get; protected set; } = "";

        /// <summary>Gets the orientation of the current <see cref="T:Molten.IDisplayOutput" />.</summary>
        public abstract DisplayOrientation Orientation { get; }

        /// <summary>Gets the adapter that the display device is connected to.</summary>
        public IDisplayAdapter Adapter { get { return _adapter; } }
    }
}
