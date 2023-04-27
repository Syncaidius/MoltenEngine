using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Molten.IO;
using Newtonsoft.Json;

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

        static Dictionary<SpirvOpCode, SpirvInstructionDef> _defs;

        uint* _ptrStart;
        uint* _ptrEnd;
        uint* _ptr;
        ulong _numWords;
        List<SpirvInstruction> _instructions;

        static SpirvReflector()
        {
            _defs = new Dictionary<SpirvOpCode, SpirvInstructionDef>();
            Stream stream = EmbeddedResource.TryGetStream("Molten.Graphics.Vulkan.Shaders.Spirv.Spirv_opcodes.json", typeof(SpirvInstructionDef).Assembly);
            if (stream != null)
            {
                string json = null;
                using (StreamReader reader = new StreamReader(stream))
                    json = reader.ReadToEnd();

                stream.Dispose();

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        Dictionary<string, SpirvInstructionDef> instructionDefs = JsonConvert.DeserializeObject<Dictionary<string, SpirvInstructionDef>>(json);
                        foreach (SpirvInstructionDef def in instructionDefs.Values)
                            _defs.Add((SpirvOpCode)def.ID, def);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to parse SPIR-V opcode definitions: {ex.Message}");
                    }
                }
            }
            else
            {
                Debug.WriteLine($"SPIR-V opcode definition file not found.");
            }
        }

        internal SpirvReflector(void* byteCode, nuint numBytes, Logger log)
        {
            if (numBytes % 4 != 0)
                throw new ArgumentException("Bytecode size must be a multiple of 4.", nameof(numBytes));

            _ptrEnd = (uint*)((byte*)byteCode + numBytes);
            _ptrStart = (uint*)byteCode;
            _ptr = _ptrStart;
            _instructions = new List<SpirvInstruction>();
            _numWords = numBytes / 4U;

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

            ReadInstructions(log);
        }

        private void ReadInstructions(Logger log)
        {
            uint instID = 0;
            while (_ptr < _ptrEnd)
            {
                SpirvInstruction inst = new SpirvInstruction(_ptr);
                _instructions.Add(inst);

                if (_defs.TryGetValue(inst.OpCode, out SpirvInstructionDef def))
                {
                    uint* ptrInst = _ptr;
                    uint remainingWords = inst.WordCount;
                    uint* ptrEnd = ptrInst + inst.WordCount;
                    ptrInst++;
                    remainingWords--;

                    foreach (string wordDesc in def.Words.Keys)
                    {
                        string wordTypeName = def.Words[wordDesc];
                        uint readCount = 1;

                        Type t = GetWordType(log, wordTypeName);
                        if (t != null)
                        {
                            SpirvWord word = Activator.CreateInstance(t) as SpirvWord;
                            word.Name = wordDesc;

                            if(word is SpirvResultID resultID)
                                inst.Result = resultID;
                            else
                                inst.Words.Add(word);

                            readCount = word.Read(ptrInst, remainingWords);
                        }
                        else
                        {
                            log.Warning($"Unknown word type: {wordTypeName}");
                        }

                        ptrInst += readCount;
                        remainingWords -= readCount;
                    }

                    string operands = string.Join(", ", inst.Words.Select(x => x.ToString()));
                    string opResult = inst.Result != null ? $"{inst.Result} = " : ""; 
                    log.WriteLine($"Instruction {instID++}: {opResult}{inst.OpCode} -- {operands}");
                }
                else
                {
                    log.Warning($"Instruction {instID}: Unknown opcode ({inst.OpCode}).");
                }

                _ptr += inst.WordCount;
            }
        }

        private Type GetWordType(Logger log, string typeName)
        {
            int genericIndex = typeName.IndexOf('{');
            List<Type> genericTypes = new List<Type>();
            Type t = null;

            if (genericIndex > -1)
            {
                string generics = typeName.Substring(genericIndex + 1, typeName.Length - genericIndex - 2);
                typeName = typeName.Substring(0, genericIndex);
                string[] gParams = generics.Split(',');

                foreach(string gp in gParams)
                {
                    string genericName = gp.Trim();
                    Type gType = GetWordType(log, genericName);
                    if (gType == null)
                    {
                        log.Warning($"Unknown generic type '{genericName}' for type '{typeName}'");
                        return null;
                    }

                    genericTypes.Add(gType);
                }

                t = Type.GetType($"Molten.Graphics.Vulkan.{typeName}`{genericTypes.Count}");
                if (t != null)
                    t = t.MakeGenericType(genericTypes.ToArray());
            }
            else
            {
                t = Type.GetType($"Molten.Graphics.Vulkan.{typeName}");
            }

            return t;
        }

        private uint ReadWord()
        {
            uint val = *_ptr;
            _ptr++;
            return val;
        }

        public ulong WordCount => _numWords;
    }
}
