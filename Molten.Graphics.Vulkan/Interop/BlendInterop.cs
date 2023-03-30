using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal static class BlendInterop
    {
        public static LogicOp ToApi(this LogicOperation mode)
        {
            switch (mode)
            {
                default:
                case LogicOperation.Set:
                    return LogicOp.Set;

                case LogicOperation.Nor:
                    return LogicOp.Nor;

                case LogicOperation.Or:
                    return LogicOp.Or;

                case LogicOperation.Nand:
                    return LogicOp.Nand;

                case LogicOperation.Xor:
                    return LogicOp.Xor;

                case LogicOperation.Clear:
                    return LogicOp.Clear;

                case LogicOperation.And:
                    return LogicOp.And;

                case LogicOperation.AndReverse:
                    return LogicOp.AndReverse;

                case LogicOperation.Copy:
                    return LogicOp.Copy;

                case LogicOperation.AndInverted:
                    return LogicOp.AndInverted;

                case LogicOperation.Noop:
                    return LogicOp.NoOp;

                case LogicOperation.Equivalent:
                    return LogicOp.Equivalent;

                case LogicOperation.Invert:
                    return LogicOp.Invert;

                case LogicOperation.OrReverse:
                    return LogicOp.OrReverse;

                case LogicOperation.OrInverted:
                    return LogicOp.OrInverted;

                case LogicOperation.CopyInverted:
                    return LogicOp.CopyInverted;
            }
        }

        public static BlendOp ToApi(this BlendOperation op)
        {
            switch (op)
            {
                case BlendOperation.Min: return BlendOp.Min;
                case BlendOperation.Max: return BlendOp.Max;
                case BlendOperation.Add: return BlendOp.Add;
                case BlendOperation.Subtract: return BlendOp.Subtract;
                case BlendOperation.RevSubtract: return BlendOp.ReverseSubtract;

                default:
                case BlendOperation.Invalid:
                    throw new NotSupportedException($"Unsupported blend operation value: {op}");
            }
        }

        public static BlendFactor ToApi(this BlendType type)
        {
            switch (type)
            {
                case BlendType.Zero: return BlendFactor.Zero;
                case BlendType.One: return BlendFactor.One;
                case BlendType.SrcColor: return BlendFactor.SrcColor;
                case BlendType.InvSrcColor: return BlendFactor.OneMinusSrcColor;
                case BlendType.SrcAlpha: return BlendFactor.SrcAlpha;
                case BlendType.InvSrcAlpha: return BlendFactor.OneMinusSrcAlpha;
                case BlendType.DestAlpha: return BlendFactor.DstAlpha;
                case BlendType.InvDestAlpha: return BlendFactor.OneMinusDstAlpha;
                case BlendType.DestColor: return BlendFactor.DstColor;
                case BlendType.InvDestColor: return BlendFactor.OneMinusDstColor;
                case BlendType.SrcAlphaSat: return BlendFactor.SrcAlphaSaturate;
                case BlendType.BlendFactor: return BlendFactor.ConstantColor;
                case BlendType.InvBlendFactor: return BlendFactor.OneMinusConstantColor;
                case BlendType.Src1Color: return BlendFactor.Src1Color;
                case BlendType.InvSrc1Color: return BlendFactor.OneMinusSrc1Color;
                case BlendType.Src1Alpha: return BlendFactor.Src1Alpha;
                case BlendType.InvSrc1Alpha: return BlendFactor.OneMinusSrc1Alpha;
                case BlendType.BlendFactorAlpha: return BlendFactor.ConstantAlpha;
                case BlendType.InvBlendFactorAlpha: return BlendFactor.OneMinusConstantAlpha;

                default:
                case BlendType.Invalid:
                    throw new NotSupportedException($"Unsupported blend type: {type}");
            }
        }

        public static ColorComponentFlags ToApi(this ColorWriteFlags flags)
        {
            // Vulkan color component flags share the same values as DX11/Molten.
            return (ColorComponentFlags) flags;
        }
    }
}
