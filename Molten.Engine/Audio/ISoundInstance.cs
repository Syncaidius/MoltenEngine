using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Audio
{
    /// <summary>
     /// Represents an implementation of an audio instance.
     /// </summary>
    public interface ISoundInstance : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="ISoundSource"/> that owns the current instance.
        /// </summary>
        ISoundSource Source { get; }
    }
}
