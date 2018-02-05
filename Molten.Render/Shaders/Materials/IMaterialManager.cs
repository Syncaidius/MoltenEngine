using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IMaterialManager
    {
        /// <summary>Gets a material that was previously loaded via an asset manager.</summary>
        /// <param name="name">The name of the material. case-insensitive.</param>
        /// <returns></returns>
        IMaterial this[string name] { get; }
    }
}
