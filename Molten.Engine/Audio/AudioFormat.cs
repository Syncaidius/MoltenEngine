using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    public enum AudioFormat
    {
        //
        // Summary:
        //     1 Channel, 8 bits per sample.
        Mono8 = 4352,
        //
        // Summary:
        //     1 Channel, 16 bits per sample.
        Mono16,
        //
        // Summary:
        //     2 Channels, 8 bits per sample each.
        Stereo8,
        //
        // Summary:
        //     2 Channels, 16 bits per sample each.
        Stereo16
    }
}
