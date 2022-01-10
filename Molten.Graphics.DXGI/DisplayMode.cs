using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Dxgi
{
    public class DisplayMode
    {
        public ModeDesc1 Description;

        public DisplayMode(ref ModeDesc1 desc)
        {
            Description = desc;
        }

        public uint Width
        {
            get => Description.Width;
            set => Description.Width = value;
        }

        public uint Height
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

        public ModeScaling Scaling { get { return Description.Scaling; } }

        public ModeScanlineOrder ScanLineOrdering { get { return Description.ScanlineOrdering; } }
    }
}
