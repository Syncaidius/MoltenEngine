using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace Molten.Audio.OpenAL
{
    internal static class InteropExtensions
    {
        public static BufferFormat ToApi(this AudioFormat format)
        {
            return (BufferFormat)format;
        }

        public static AudioFormat FromApi(this BufferFormat format)
        {
            return (AudioFormat)format;
        }
    }
}
