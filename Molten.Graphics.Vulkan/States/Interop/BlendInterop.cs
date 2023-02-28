using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
