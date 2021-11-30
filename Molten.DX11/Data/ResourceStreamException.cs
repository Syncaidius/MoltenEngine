using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ResourceStreamException : Exception
    {
        internal ResourceStreamException(Map mapType, string message) : base($"{message} - Mode: {mapType}")
        {
            MapType = mapType;
        }

        /// <summary>
        /// Gets the <see cref="Map"/> type value that the <see cref="ResourceStream"/> was set to when the exception occurred.
        /// </summary>
        public Map MapType { get; }
    }
}
