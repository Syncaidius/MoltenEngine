using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    public class DisplayMode
    {
        public ModeDescription Description;

        public DisplayMode(ModeDescription desc)
        {
            Description = desc;
        }

        public int Width
        {
            get => Description.Width;
            set => Description.Width = value;
        }

        public int Height
        {
            get => Description.Height;
            set => Description.Height = value;
        }

        public Rational RefreshRate
        {
            get => Description.RefreshRate;
            set => Description.RefreshRate = value;
        }

        public Format Format { get { return Description.Format; } }

        public DisplayModeScaling Scaling { get { return Description.Scaling; } }

        public DisplayModeScanlineOrder ScanLineOrdering { get { return Description.ScanlineOrdering; } }
    }
}
