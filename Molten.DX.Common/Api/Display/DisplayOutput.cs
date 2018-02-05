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
    public class DisplayOutput<A,D, O> : GraphicsOutput
        where A : Adapter
        where D : struct
        where O : Output
    {
        O _output;
        OutputDescription _desc;
        GraphicsAdapter<A, D, O> _adapter;

        internal DisplayOutput(GraphicsAdapter<A, D, O> adapter, O output) : base(adapter)
        {
            _adapter = adapter;
            _output = output;
            _desc = _output.Description;
            Name = _desc.DeviceName.Replace("\0", string.Empty);
        }

        public DisplayMode[] GetSupportedModes(Format format)
        {
            ModeDescription[] m = _output.GetDisplayModeList(format, DisplayModeEnumerationFlags.Interlaced | DisplayModeEnumerationFlags.Scaling);
            DisplayMode[] modes = new DisplayMode[m.Length];

            //build a list of all valid display modes
            for (int i = 0; i < m.Length; i++)
                modes[i] = new DisplayMode(m[i]);

            return modes;
        }

        /// <summary>Gets the underlying output device that this display device instance represents.</summary>
        internal O Output => _output;

        /// <summary>Gets the resolution/size of the dekstop bound to the output, if any.</summary>
        public Rectangle DesktopBounds => _desc.DesktopBounds.FromRawApi();

        /// <summary>Gets whether or not the output is bound to a desktop.</summary>
        public bool IsBoundToDesktop { get { return _desc.IsAttachedToDesktop; } }

        /// <summary>Gets the orientation of the current <see cref="T:StoneBolt.IDisplayOutput" />.</summary>
        public override DisplayOrientation Orientation => (DisplayOrientation)_desc.Rotation;
    }
}
