using Molten.Collections;
using Molten.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>A helper class that safely wraps InputLayout.</summary>
    internal unsafe class VertexInputLayout : EngineObject
    {
        internal ID3D11InputLayout* Native;
        bool _valid = true;
        bool _isInstanced = false;
        ulong[] _hashKeys;

        internal VertexInputLayout(DeviceDX11 device, 
            PipelineBindSlot<BufferSegment, DeviceDX11, PipeDX11>[] slots, 
            byte[] vertexBytecode,
            ShaderIOStructure io)
        {
            uint maxSlots = device.Features.MaxVertexBufferSlots;
            _hashKeys = new ulong[maxSlots];
            List<InputElementDesc> elements = new List<InputElementDesc>();
            VertexFormat format = null;

            for (int i = 0; i < maxSlots; i++)
            {
                if (slots[i].BoundObject == null)
                    continue;

                format = slots[i].BoundObject.VertexFormat;

                /* Check if the current vertex segment's format matches 
                   the part of the shader's input structure it's meant to represent. */
                int startID = elements.Count;
                bool inputMatch = io.IsCompatible(format, (int)startID);
                if (inputMatch == false)
                {
                    _valid = false;
                    break;
                }

                // Collate vertex format elements into layout and set the correct input slot for each element.
                elements.AddRange(format.Elements);
                for (int eID = startID; eID < elements.Count; eID++)
                {
                    InputElementDesc e = elements[eID];
                    e.InputSlot = i; // Vertex buffer input slot.
                    elements[eID] = e;

                    _isInstanced = _isInstanced || elements[eID].InputSlotClass == InputClassification.InputPerInstanceData;
                }

                _hashKeys[i] = (ulong)format.HashKey << 32 | (uint)io.HashKey;
            }

            // Check if there are actually any elements. If not, use the default placeholder vertex type.
            if (elements.Count == 0)
            {
                VertexFormat nullFormat = device.VertexBuilder.GetFormat<VertexWithID>();
                elements.Add(nullFormat.Elements[0]);
            }

            InputElementDesc[] finalElements = elements.ToArray();

            // Attempt creation of input layout.
            if (_valid)
            {
                device.Native->CreateInputLayout(finalElements, 
                    finalElements.Length, 
                    vertexBytecode, 
                    vertexBytecode.Length, 
                    ref Native);
            }
            else
            {
                device.Log.WriteWarning($"Vertex formats do not match the input layout of shader:");
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].BoundObject == null)
                        continue;

                    format = slots[i].BoundObject.VertexFormat;


                    device.Log.WriteWarning("Format - Buffer slot "+ i + ": ");
                    for (int f = 0; f < format.Elements.Length; f++)
                    {
                        device.Log.WriteWarning("\t[" + f + "] " + format.Elements[f].SemanticName +
                            " -- index: " + format.Elements[f].SemanticIndex);
                    }
                }

                device.Log.WriteWarning("Shader Input Structure: ");
                for (int i = 0; i < finalElements.Length; i++)
                {
                    device.Log.WriteWarning("\t[" + i + "] " + finalElements[i].SemanticName +
                        " -- index: " + finalElements[i].SemanticIndex);
                }
            }
        }

        public bool IsMatch(Logger log, PipelineBindSlot<BufferSegment, DeviceDX11, PipeDX11>[] slots, ShaderIOStructure io, int lastSlot)
        {
            bool isMatch = true;

            for (int i = 0; i < lastSlot; i++)
            {
                // If resource is null, check if is actually meant to be.
                if (slots[i].BoundObject == null)
                {
                    if (_hashKeys[i] == 0)
                    {
                        continue;
                    }
                    else
                    {
                        isMatch = false;
                        break;
                    }
                }

                // Prevent vertex segments with no format from crashing the application.
                if (slots[i].BoundObject.VertexFormat == null)
                {
                    log.WriteWarning($"Missing format for vertex segment in slot {i}. Skipping validation. This may cause a false input layout match.");
                    continue;
                }

                // if composite hash-key does not match the one held by the input layout, flag match as false and abort.
                ulong comKey = (ulong)slots[i].BoundObject.VertexFormat.HashKey << 32 | (uint)io.HashKey;
                if (comKey != _hashKeys[i])
                {
                    isMatch = false;
                    break;
                }
            }

            return isMatch;
        }

        protected override void OnDispose()
        {
            Native->Release();
            Native = null;
        }

        /// <summary>Gets whether or not the input layout is valid.</summary>
        internal bool IsValid
        {
            get { return _valid; }
        }

        /// <summary>Gets whether or not the vertex input layout is designed for use with instanced draw calls.</summary>
        public bool IsInstanced
        {
            get { return _isInstanced; }
        }
    }
}
