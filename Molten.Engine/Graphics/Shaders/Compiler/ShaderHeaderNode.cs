using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderHeaderNode
    {
        /// <summary>
        /// The original name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value that was held in the node.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The index or slot that the node refers to for a shader element.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets the type of value held in <see cref="Value"/>.
        /// </summary>
        public ShaderHeaderValueType ValueType { get; set; }

        /// <summary>
        /// Gets a list of child values that were bound to the header node
        /// </summary>
        public List<(string key, string value)> ChildValues { get; } = new List<(string key, string value)>();
    }

    public enum ShaderHeaderValueType
    {
        /// <summary>
        /// No value was provided.
        /// </summary>
        None = 0,

        /// <summary>
        /// A value was provided.
        /// </summary>
        Value = 1,

        /// <summary>
        /// The value is the name of a preset
        /// </summary>
        Preset = 1
    }
}
