using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct NodeParseResult
    {
        public string Message;

        public NodeParseResultType Type;

        public NodeParseResult(NodeParseResultType type)
        {
            Message = null;
            Type = type;
        }

        public NodeParseResult(NodeParseResultType type, string message)
        {
            Message = message;
            Type = type;
        }
    }

    public enum NodeParseResultType
    {
        Ignored = 0,

        Error = 1,

        Success = 2,
    }
}
