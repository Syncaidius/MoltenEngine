using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>
    /// Used to determine how a content asset behaves when reloaded after changes.
    /// </summary>
    public class ContentReloadAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="ContentReloadAttribute"/>.
        /// </summary>
        /// <param name="reinstantiate">If true, the content should be re-instantiated if reloaded.</param>
        public ContentReloadAttribute(bool reinstantiate)
        {
            Reinstantiate = reinstantiate;
        }

        /// <summary>
        /// Gets whether the content should be re-instantiated if it is reloaded.
        /// </summary>
        public bool Reinstantiate { get; }
    }
}
