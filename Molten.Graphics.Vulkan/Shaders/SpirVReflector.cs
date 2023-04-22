using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    /// <summary>
    /// Generates a <see cref="ShaderReflection"/> object from a compiled SPIR-V shader, by parsing its bytecode.
    /// </summary>
    /// <remarks>For SPIR-V specificational information see the following:
    /// <para>Main specification: https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#_magic_number</para>
    /// <para>Physical/Data layout: https://registry.khronos.org/SPIR-V/specs/unified1/SPIRV.html#PhysicalLayout</para>
    /// </remarks>
    internal unsafe class SpirvReflector
    {
        const uint MAGIC_NUMBER = 0x07230203;

        uint* _instructions;
        ulong _numInstructions;
        ulong _pos;

        internal SpirvReflector(void* byteCode, nuint numBytes)
        {
            if (numBytes % 4 != 0)
                throw new ArgumentException("Bytecode size must be a multiple of 4.", nameof(numBytes));

            _instructions = (uint*)byteCode;
            _numInstructions = numBytes / 4U;

            // First op is always the magic number.
            if (ReadWord() != MAGIC_NUMBER)
                throw new ArgumentException("Invalid SPIR-V bytecode.", nameof(byteCode));

            // Next op is the version number.
            SpirvVersion version = (SpirvVersion)ReadWord();

            // Next op is the generator number.
            uint generator = ReadWord();

            // Next op is the bound number.
            uint bound = ReadWord();

            // Next op is the schema number.
            uint schema = ReadWord();

            ulong test = ReadInstruction<ulong>();
        }

        private uint ReadWord()
        {
            return _instructions[_pos++];
        }

        private T ReadInstruction<T>()
            where T : unmanaged
        {
            uint head = _instructions[_pos++];
            ulong opDataPos = _pos;

            uint wordCount = head >> 16;
            uint opCode = head & 0xFFFF;

            T op = *(T*)(_instructions + opDataPos);
            _pos += wordCount - 1;
            return op;
        }

        public ulong Position => _pos;

        public ulong NumInstructions => _numInstructions;
    }
}
