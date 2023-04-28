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
        static Dictionary<SpirvOpCode, SpirvInstructionDef> _defs;
        List<SpirvInstruction> _instructions;
        SpirvStream _stream;

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
            _instructions = new List<SpirvInstruction>();
            _stream = new SpirvStream(byteCode, numBytes);

            // Next op is the version number.
            SpirvVersion version = (SpirvVersion)_stream.ReadWord();

            // Next op is the generator number.
            uint generator = _stream.ReadWord();

            // Next op is the bound number.
            uint bound = _stream.ReadWord();

            // Next op is the schema number.
            uint schema = _stream.ReadWord();

            ReadInstructions(log);
        }

        private void ReadInstructions(Logger log)
        {
            uint instID = 0;
            while (!_stream.IsEndOfStream)
            {
                SpirvInstruction inst = _stream.ReadInstruction();
                _instructions.Add(inst);

                if (_defs.TryGetValue(inst.OpCode, out SpirvInstructionDef def))
                {
                    foreach (string wordDesc in def.Words.Keys)
                    {
                        string wordTypeName = def.Words[wordDesc];
                        Type t = GetWordType(log, wordTypeName);

                        if (t != null)
                        {
                            SpirvWord word = Activator.CreateInstance(t) as SpirvWord;
                            word.Name = wordDesc;

                            if(word is SpirvResultID resultID)
                                inst.Result = resultID;
                            else
                                inst.Words.Add(word);

                            word.Read(inst);
                        }
                        else
                        {
                            log.Warning($"Unknown word type: {wordTypeName}");
                        }
                    }

                    string opResult = inst.Result != null ? $"{inst.Result} = " : "";
                    if (inst.Words.Count > 0)
                    {
                        string operands = string.Join(", ", inst.Words.Select(x => x.ToString()));
                        log.WriteLine($"Instruction {instID}: {opResult}{inst.OpCode} -- {operands}");
                    }
                    else
                    {
                        log.WriteLine($"Instruction {instID}: {opResult}{inst.OpCode}");
                    }
                }
                else
                {
                    log.Warning($"Instruction {instID}: Unknown opcode ({inst.OpCode}).");
                }

                instID++;
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
                if(t == null)
                    t = Type.GetType($"System.{typeName}`{genericTypes.Count}");

                if (t != null)
                    t = t.MakeGenericType(genericTypes.ToArray());                    
            }
            else
            {
                t = Type.GetType($"Molten.Graphics.Vulkan.{typeName}");
                if(t == null)
                    t = Type.GetType($"System.{typeName}");
            }

            return t;
        }




    }
}
