using Molten.Comparers;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Molten.Graphics
{
    public sealed class VertexFormat : EngineObject
    {
        private class FieldElement
        {
            public FieldInfo Info;
            public IntPtr Offset;
        }

        static IntPtrComparer _ptrComparer = new IntPtrComparer();

        internal InputElementData Data { get; } 

        private unsafe VertexFormat(uint elementCount)
        {
            Data = new InputElementData(elementCount);
        }

        protected unsafe override void OnDispose()
        {
            Data.Dispose();
        }

        /// <summary>Gets the total size of the Vertex Format, in bytes.</summary>
        public uint SizeOf { get; private set; }

        internal static VertexFormat FromType<T>() where T : IVertexType
        {
            return FromType(typeof(T));
        }

        internal unsafe static VertexFormat FromType(Type t)
        {
            List<FieldElement> fieldElements = new List<FieldElement>();

            // Retrieve the offset of each field member in the struct, which we can then use for sorting them.
            FieldInfo[] fields = t.GetFields();
            foreach (FieldInfo info in fields)
            {
                FieldElement el = new FieldElement()
                {
                    Info = info,
                    Offset = Marshal.OffsetOf(t, info.Name),
                };
                fieldElements.Add(el);
            }

            fieldElements = fieldElements.OrderBy(e => e.Offset, _ptrComparer).ToList();

            uint eCount = 0;
            for (int ec = 0; ec < fieldElements.Count; ec++)
                eCount += fieldElements[ec] != null ? 1U : 0;

            VertexFormat vf = new VertexFormat(eCount);
            eCount = 0;

            // Now figure out what kind of InputElement each field represents.
            foreach (FieldElement e in fieldElements)
            {
                VertexElementAttribute att = e.Info.GetCustomAttribute<VertexElementAttribute>();
                if (att == null)
                {
                    // TODO Attempt to figure out what the element is or throw an exception if we're unable to.
                }
                else
                {
                    vf.Data.Metadata[eCount].Name = GetSemanticName(att.Usage);
                    vf.Data.Elements[eCount] = new InputElementDesc()
                    {
                        SemanticName = (byte*)SilkMarshal.StringToPtr(vf.Data.Metadata[eCount].Name),
                        SemanticIndex = att.SemanticIndex,
                        AlignedByteOffset = vf.SizeOf,
                        InputSlotClass = att.Classification.ToApi(),
                    };

                    vf.SizeOf += CalculateElement(att.Type, ref vf.Data.Elements[eCount]);
                }

                eCount++;
            }

            return vf;
        }

        private static string GetSemanticName(VertexElementUsage usage)
        {
            switch (usage)
            {
                case VertexElementUsage.PixelPosition:
                    return "SV_POSITION";

                case VertexElementUsage.Position:
                    return "POSITION";

                case VertexElementUsage.Color:
                    return "COLOR";

                case VertexElementUsage.Normal:
                    return "NORMAL";

                case VertexElementUsage.TextureCoordinate:
                    return "TEXCOORD";

                case VertexElementUsage.BlendIndices:
                    return "BLENDINDICES";

                case VertexElementUsage.BlendWeight:
                    return "BLENDWEIGHT";

                case VertexElementUsage.Binormal:
                    return "BINORMAL";

                case VertexElementUsage.Tangent:
                    return "TANGENT";

                case VertexElementUsage.PointSize:
                    return "PSIZE";

                case VertexElementUsage.VertexID:
                    return "SV_VERTEXID";

                case VertexElementUsage.InstanceID:
                    return "SV_INSTANCEID";

                default:
                    throw new NotSupportedException($"Unsupported vertex element usage: {usage}");
            }
        }

        /// <summary>Calculates the format and size of an element based on the specified <see cref="VertexElementType"/>.</summary>
        /// <param name="type">The type of the element. This is passed in because <see cref="InputElement"/> cannot store the type information for us.</param>
        /// <param name="element">The element to calculate the format and size of.</param>
        /// <returns>The expected size of the element.</returns>
        private static uint CalculateElement(VertexElementType type, ref InputElementDesc element)
        {
            switch (type)
            {
                case VertexElementType.Float:
                    element.Format = Format.FormatR32Float;
                    return 4;

                case VertexElementType.Vector2:
                    element.Format = Format.FormatR32G32Float;
                    return 8;

                case VertexElementType.Vector3:
                    element.Format = Format.FormatR32G32B32Float;
                    return 12;

                case VertexElementType.Vector4:
                    element.Format = Format.FormatR32G32B32A32Float;
                    return 16;

                case VertexElementType.Color:
                    element.Format = Format.FormatR8G8B8A8Unorm;
                    return 4;

                case VertexElementType.Byte:
                    element.Format = Format.FormatR8Uint;
                    return 1;

                case VertexElementType.Byte4:
                    element.Format = Format.FormatR8G8B8A8Uint;
                    return 4;

                case VertexElementType.Short:
                    element.Format = Format.FormatR16Sint;
                    return 2;

                case VertexElementType.Short2:
                    element.Format = Format.FormatR16G16Sint;
                    return 4;

                case VertexElementType.Short4:
                    element.Format = Format.FormatR16G16B16A16Sint;
                    return 8;

                case VertexElementType.NormalizedShort2:
                    element.Format = Format.FormatR16G16SNorm;
                    return 4;

                case VertexElementType.NormalizedShort4:
                    element.Format = Format.FormatR16G16B16A16SNorm;
                    return 8;

                case VertexElementType.Half:
                    element.Format = Format.FormatR16Float;
                    return 2;

                case VertexElementType.Half2:
                    element.Format = Format.FormatR16G16Float;
                    return 4;

                case VertexElementType.Half4:
                    element.Format = Format.FormatR16G16B16A16Float;
                    return 8;

                case VertexElementType.Int:
                    element.Format = Format.FormatR32Sint;
                    return 4;

                case VertexElementType.Int2:
                    element.Format = Format.FormatR32G32Sint;
                    return 8;

                case VertexElementType.Int3:
                    element.Format = Format.FormatR32G32B32Sint;
                    return 12;

                case VertexElementType.Int4:
                    element.Format = Format.FormatR32G32B32A32Sint;
                    return 16;

                case VertexElementType.UInt:
                    element.Format = Format.FormatR32Uint;
                    return 4;

                case VertexElementType.UInt2:
                    element.Format = Format.FormatR32G32Uint;
                    return 8;

                case VertexElementType.UInt3:
                    element.Format = Format.FormatR32G32B32Uint;
                    return 12;

                case VertexElementType.UInt4:
                    element.Format = Format.FormatR32G32B32A32Uint;
                    return 16;

                default:
                    throw new NotSupportedException("Unknown vertex element format!");
            }
        }
    }
}
