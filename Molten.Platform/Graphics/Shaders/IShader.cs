using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IShader
    {
        /// <summary>Gets the description of the material.</summary>
        string Description { get; }

        /// <summary>Gets the author of the material.</summary>
        string Author { get; }

        string Filename { get; }

        Dictionary<string, string> Metadata { get; }

        /// <summary>Gets or sets a material value.</summary>
        /// <param name="key">The value key</param>
        /// <returns></returns>
        IShaderValue this[string key] { get; set; }
    }
}
