using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.GLFW;

namespace Molten.Graphics
{
    internal unsafe class DisplayModeVK : IDisplayMode
    {
        VideoMode* _mode;

        internal DisplayModeVK(VideoMode* mode)
        {
            _mode = mode;

            if (_mode->RedBits > 0 && _mode->GreenBits > 0 && _mode->BlueBits > 0)
            {
                string modeName = $"R{_mode->RedBits}G{_mode->GreenBits}B{_mode->BlueBits}A8_SNorm";

                if (Enum.TryParse(modeName, out GraphicsFormat format))
                    Format = format;
                else
                    Format = GraphicsFormat.R8G8B8A8_SNorm;
            }
        }

        public uint Width => (uint)_mode->Width;

        public uint Height => (uint)_mode->Height;

        public uint RefreshRate => (uint)_mode->RefreshRate;

        public DisplayScalingMode Scaling => DisplayScalingMode.Centered | DisplayScalingMode.Stretched;

        public bool StereoPresent => false;

        public GraphicsFormat Format { get; }
    }
}
