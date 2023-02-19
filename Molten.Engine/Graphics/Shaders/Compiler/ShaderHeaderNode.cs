using System.Xml;

namespace Molten.Graphics
{
    public class ShaderHeaderNode
    {
        internal ShaderHeaderNode(XmlNode original)
        {
            OriginalNode = original;
            Name = original.Name;
        }

        public override string ToString()
        {
            return $"Node: {OriginalNode.Name} -- Name: {Name}";
        }

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
        public int SlotID { get; set; }

        /// <summary>
        /// Gets the type of value held in <see cref="Value"/>.
        /// </summary>
        public ShaderHeaderValueType ValueType { get; set; }

        public StateConditions Conditions { get; set; }

        /// <summary>
        /// Gets a list of child values that were bound to the header node
        /// </summary>
        public List<(string key, string value)> ChildValues { get; } = new List<(string key, string value)>();

        /// <summary>
        /// Gets a list of child nodes bound to the current <see cref="ShaderHeaderNode"/>. These were nodes which had their own child values/nodes.
        /// </summary>
        public List<ShaderHeaderNode> ChildNodes { get; } = new List<ShaderHeaderNode>();

        public XmlNode OriginalNode { get; }

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
