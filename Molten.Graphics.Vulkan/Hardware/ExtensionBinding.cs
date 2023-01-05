using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ExtensionBinding
    {
        internal Dictionary<string, VulkanExtension> Extensions = new Dictionary<string, VulkanExtension>();

        internal List<string> Layers = new List<string>();
    }
}
