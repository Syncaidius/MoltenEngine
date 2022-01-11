using Silk.NET.Core.Native;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Dxgi
{
    public unsafe class DisplayOutputDXGI : EngineObject, IDisplayOutput
    {
        internal IDXGIOutput1* Native;
        OutputDesc* _desc;
        DisplayAdapterDXGI _adapter;

        internal DisplayOutputDXGI(DisplayAdapterDXGI adapter, IDXGIOutput1* output)
        {
            _adapter = adapter;
            Native = output;
            _desc = EngineUtil.Alloc<OutputDesc>();
            Native->GetDesc(_desc);

            Name = SilkMarshal.PtrToString((nint)_desc->DeviceName);
            Name = Name.Replace("\0", string.Empty);
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _desc);
            SilkUtil.ReleasePtr(ref Native);
        }

        public DisplayMode[] GetSupportedModes(Format format)
        {
            uint flags = DXGI.EnumModesInterlaced | DXGI.EnumModesScaling;
            uint* modeCount = (uint*)0;
            ModeDesc1* modeDescs = (ModeDesc1*)0;

            Native->GetDisplayModeList1(format, flags, modeCount, modeDescs);
            ModeDesc1[] m = new ModeDesc1[(int)modeCount];
            DisplayMode[] modes = new DisplayMode[m.Length];

            // Build a list of all valid display modes
            for (int i = 0; i < m.Length; i++)
                modes[i] = new DisplayMode(ref m[i]);

            return modes;
        }

        /// <summary>Gets the resolution/size of the dekstop bound to the output, if any.</summary>
        public Rectangle DesktopBounds => _desc->DesktopCoordinates.FromApi();

        /// <summary>Gets whether or not the output is bound to a desktop.</summary>
        public bool IsBoundToDesktop { get { return _desc->AttachedToDesktop > 0; } }

        /// <summary>Gets the orientation of the current <see cref="IDisplayOutput" />.</summary>
        public DisplayOrientation Orientation => (DisplayOrientation)_desc->Rotation;

        /// <summary>Gets the adapter that the display device is connected to.</summary>
        public IDisplayAdapter Adapter => _adapter;
    }
}
