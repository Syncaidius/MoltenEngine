using SharpDX.Direct3D11;
using Molten.Comparers;
using Molten.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A helper class for building <see cref="VertexFormat"/> objects.</summary>
    internal class VertexFormatBuilder
    {
        class FieldElement
        {
            public FieldInfo Info;
            public IntPtr Offset;
        }

        Dictionary<Type, VertexFormat> _cache = new Dictionary<Type, VertexFormat>();
        IntPtrComparer _ptrComparer = new IntPtrComparer();

        internal VertexFormat GetFormat<T>() where T: struct, IVertexType
        {
            Type t = typeof(T);
            return GetFormat(t);
        }

        internal VertexFormat GetFormat(Type t)
        {
            Debug.Assert(typeof(IVertexType).IsAssignableFrom(t), "The specified vertex type must implement IVertexType.");

            VertexFormat format = null;
            if (!_cache.TryGetValue(t, out format))
            {
                format = BuildFormat(t);
                _cache.Add(t, format);
            }

            return format;
        }

        private VertexFormat BuildFormat(Type t)
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
            List<InputElement> elements = new List<InputElement>();

            // Now figure out what kind of InputElement each field represents.
            int elementAlignedOffset = 0;
            string signature = "";
            foreach(FieldElement e in fieldElements)
            {
                VertexElementAttribute att = e.Info.GetCustomAttribute<VertexElementAttribute>();
                if(att == null)
                {
                    // TODO Attempt to figure out what the element is or throw an exception if we're unable to.
                }
                else
                {
                    InputElement el = new InputElement();
                    el.SemanticName = GetSemanticName(att.Usage);
                    el.SemanticIndex = att.SemanticIndex;
                    el.AlignedByteOffset = elementAlignedOffset;
                    el.Classification = att.Classification.ToApi();
                    elementAlignedOffset += CalculateElement(att.Type, ref el);
                    signature += $"{elementAlignedOffset}{el.Format}{el.SemanticIndex}{att.Usage}{el.Classification}";
                    elements.Add(el);
                }
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(signature);
            int hash = HashHelper.ComputeFNV(bytes);
            return new VertexFormat(elements.ToArray(), elementAlignedOffset, hash);
        }

        private string GetSemanticName(VertexElementUsage usage)
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
        private int CalculateElement(VertexElementType type, ref InputElement element)
        {
            switch (type)
            {
                case VertexElementType.Float:
                    element.Format =  SharpDX.DXGI.Format.R32_Float;
                    return  4;

                case VertexElementType.Vector2:
                    element.Format =  SharpDX.DXGI.Format.R32G32_Float;
                    return  8;

                case VertexElementType.Vector3:
                    element.Format =  SharpDX.DXGI.Format.R32G32B32_Float;
                    return  12;

                case VertexElementType.Vector4:
                    element.Format =  SharpDX.DXGI.Format.R32G32B32A32_Float;
                    return  16;

                case VertexElementType.Color:
                    element.Format =  SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                    return  4;

                case VertexElementType.Byte:
                    element.Format =  SharpDX.DXGI.Format.R8_UInt;
                    return  1;

                case VertexElementType.Byte4:
                    element.Format =  SharpDX.DXGI.Format.R8G8B8A8_UInt;
                    return  4;

                case VertexElementType.Short:
                    element.Format =  SharpDX.DXGI.Format.R16_SInt;
                    return  2;

                case VertexElementType.Short2:
                    element.Format =  SharpDX.DXGI.Format.R16G16_SInt;
                    return  4;

                case VertexElementType.Short4:
                    element.Format =  SharpDX.DXGI.Format.R16G16B16A16_SInt;
                    return  8;

                case VertexElementType.NormalizedShort2:
                    element.Format =  SharpDX.DXGI.Format.R16G16_SNorm;
                    return  4;

                case VertexElementType.NormalizedShort4:
                    element.Format =  SharpDX.DXGI.Format.R16G16B16A16_SNorm;
                    return  8;

                case VertexElementType.Half:
                    element.Format =  SharpDX.DXGI.Format.R16_Float;
                    return  2;

                case VertexElementType.Half2:
                    element.Format =  SharpDX.DXGI.Format.R16G16_Float;
                    return  4;

                case VertexElementType.Half4:
                    element.Format =  SharpDX.DXGI.Format.R16G16B16A16_Float;
                    return  8;

                case VertexElementType.Int:
                    element.Format =  SharpDX.DXGI.Format.R32_SInt;
                    return  4;

                case VertexElementType.Int2:
                    element.Format =  SharpDX.DXGI.Format.R32G32_SInt;
                    return  8;

                case VertexElementType.Int3:
                    element.Format =  SharpDX.DXGI.Format.R32G32B32_SInt;
                    return  12;

                case VertexElementType.Int4:
                    element.Format =  SharpDX.DXGI.Format.R32G32B32A32_SInt;
                    return  16;

                case VertexElementType.UInt:
                    element.Format =  SharpDX.DXGI.Format.R32_UInt;
                    return  4;

                case VertexElementType.UInt2:
                    element.Format =  SharpDX.DXGI.Format.R32G32_UInt;
                    return  8;

                case VertexElementType.UInt3:
                    element.Format =  SharpDX.DXGI.Format.R32G32B32_UInt;
                    return  12;

                case VertexElementType.UInt4:
                    element.Format =  SharpDX.DXGI.Format.R32G32B32A32_UInt;
                    return  16;

                default:
                    throw new NotSupportedException("Unknown vertex element format!");
            }
        }
    }
}
