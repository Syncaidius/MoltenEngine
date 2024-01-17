using Molten.Comparers;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Molten.Graphics;

public enum ShaderIOLayoutType
{
    Input = 0,

    Output = 1,
}

/// <summary>Represents an automatically generated shader input layout. 
/// Also generating useful metadata that can be used to validate vertex input at engine level.</summary>
public unsafe abstract class ShaderIOLayout : EngineObject, IEquatable<ShaderIOLayout>
{
    private class FieldElement
    {
        public FieldInfo Info;
        public IntPtr Offset;
    }

    public struct ElementMetadata
    {
        public string Name;

        public ShaderSVType SystemValueType;

        public uint SemanticIndex;

        public uint Register;

        public ShaderRegisterType ComponentType;
    }

    static IntPtrComparer _ptrComparer = new IntPtrComparer();

    /// <summary>
    /// Contains extra/helper information about elements
    /// </summary>
    public ElementMetadata[] Metadata { get; private set; }

    // Reference: http://takinginitiative.wordpress.com/2011/12/11/directx-1011-basic-shader-reflection-automatic-input-layout-creation/

    /// <summary>
    /// Creates a new, empty instance of <see cref="ShaderIOLayout"/>.
    /// </summary>
    protected ShaderIOLayout() { }

    /// <summary>
    /// Builds the current <see cref="ShaderIOLayout"/> as a vertex input layout, 
    /// using the provided <typeparamref name="T"/> to provide the vertex element information.
    /// </summary>
    /// <typeparam name="T">The type of vertex to use when building the input layout.</typeparam>
    internal void Build<T>()
        where T : struct, IVertexType
    {
        Type t = typeof(T);
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

        // Count out how many elements we have. WTF is this??? - Just use fieldElements.Count???
        int eCount = fieldElements.Count;

        Metadata = new ElementMetadata[eCount];
        InitializeAsVertexLayout(eCount);
        uint sizeOf = 0;

        // Now figure out what kind of InputElement each field represents.
        for (int i = 0; i < fieldElements.Count; i++)
        {
            FieldElement e = fieldElements[i];
            VertexElementAttribute att = e.Info.GetCustomAttribute<VertexElementAttribute>();
            if (att == null)
            {
                // TODO Attempt to figure out what the element is or throw an exception if we're unable to.
            }
            else
            {
                Metadata[i].Name = GetSemanticName(att);
                Metadata[i].SemanticIndex = att.SemanticIndex;
                BuildVertexElement(att, (uint)i, sizeOf);

                sizeOf += att.Type.ToGraphicsFormat().BytesPerPixel();
            }
        }
    }

    /// <summary>Builds the current <see cref="ShaderIOLayout"/> instance from shader reflection info.</summary>
    /// <param name="result">The <see cref="ShaderCodeResult"/> reflection object.</param>
    /// <param name="type">The type of IO structure to build from reflection.</param>
    internal void Build(ShaderCodeResult result, ShaderIOLayoutType type)
    {
        List<ShaderParameterInfo> parameters;

        switch (type)
        {
            case ShaderIOLayoutType.Input:
                parameters = result.Reflection.InputParameters;
                break;

            case ShaderIOLayoutType.Output:
                parameters = result.Reflection.OutputParameters;
                break;

            default:
                throw new Exception("The ShaderIOLayoutType provided is not supported. Must be Input or Output only.");
        }

        Metadata = new ElementMetadata[parameters.Count];

        for (int i = 0; i < parameters.Count; i++)
        {
            ShaderParameterInfo pInfo = parameters[i];

            Metadata[i] = new ElementMetadata()
            {
                Name = pInfo.SemanticName,
                SystemValueType = pInfo.SystemValueType,
                SemanticIndex = pInfo.SemanticIndex,
                Register = pInfo.Register,
                ComponentType = pInfo.ComponentType
            };
        }
    }

    private static string GetSemanticName(VertexElementAttribute att)
    {
        switch (att.Usage)
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

            case VertexElementUsage.Custom:
                return att.CustomSemantic;

            default:
                throw new NotSupportedException($"Unsupported vertex element usage: {att.Usage}");
        }
    }

    protected abstract void InitializeAsVertexLayout(int vertexElementCount);

    protected abstract void BuildVertexElement(VertexElementAttribute att, uint index, uint byteOffset);

    public bool IsCompatible(ShaderIOLayout other, int startIndex = 0)
    {
        int count = Math.Min(Metadata.Length - startIndex, other.Metadata.Length);
        for (int i = 0; i < count; i++)
        {
            int selfIndex = i + startIndex;
            if (other.Metadata[i].Name != Metadata[selfIndex].Name ||
                        other.Metadata[i].SemanticIndex != Metadata[selfIndex].SemanticIndex)
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(ShaderIOLayout other)
    {
        if(Metadata.Length != other.Metadata.Length)
            return false;

        for(int i = 0; i < Metadata.Length; i++)
        {
            if (Metadata[i].Name != other.Metadata[i].Name
                || Metadata[i].SemanticIndex != other.Metadata[i].SemanticIndex
                || Metadata[i].Register != other.Metadata[i].Register
                || Metadata[i].SystemValueType != other.Metadata[i].SystemValueType
                || Metadata[i].ComponentType != other.Metadata[i].ComponentType)
                return false;
        }

        return true;
    }
}
