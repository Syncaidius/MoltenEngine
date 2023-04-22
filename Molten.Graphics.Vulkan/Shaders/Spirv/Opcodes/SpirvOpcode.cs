using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics.Vulkan
{
    public unsafe abstract class SpirvOpcode
    {
        uint* _ptr;
        uint _worldCount;

        internal unsafe static T Get<T>(uint* ptr, uint wordCount)
            where T : SpirvOpcode, new()
        {
            T op = new T();
            op._ptr = ptr;
            op._worldCount = wordCount;
            op.Load();

            return op;
        }

        /// <summary>
        /// Gives the opcode a chance to load any additional data from the bytecode, such literal data.
        /// </summary>
        protected virtual unsafe void Load() { }

        public uint WordCount => _worldCount;

        public ref uint* Ptr => ref _ptr;

        public abstract uint ID { get; }
    }
}
