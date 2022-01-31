using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class HlslCompilerContext
    {
        public class Message
        {
            public enum Kind
            {
                Message = 0,

                Error = 1,

                Warning = 2,

                Debug = 3,
            }

            public string Text;

            public Kind MessageType;
        }

        internal ShaderCompileResult Result = new ShaderCompileResult();

        /// <summary>
        /// HLSL shader objects stored by entry-point name
        /// </summary>
        internal Dictionary<string, HlslCompileResult> HlslShaders { get; } = new Dictionary<string, HlslCompileResult>();

        internal Dictionary<string, ShaderConstantBuffer> ConstantBuffers { get; } = new Dictionary<string, ShaderConstantBuffer>();

        internal IReadOnlyList<Message> Messages { get; }

        internal HlslCompiler Compiler { get; }

        internal HlslSource Source { get; set; }

        internal bool HasErrors { get; private set; }

        internal DxcArgumentBuilder Args { get; }

        List<Message> _messages;

        internal HlslCompilerContext(HlslCompiler compiler)
        {
            Compiler = compiler;
            Args = new DxcArgumentBuilder(this);
            _messages = new List<Message>();
            Messages = _messages.AsReadOnly();
        }

        internal void AddMessage(string text, Message.Kind type = Message.Kind.Message)
        {
            _messages.Add(new Message()
            {
                Text = $"[{type}] {text}",
                MessageType = type,
            });

            if (type == Message.Kind.Error)
                HasErrors = true;
        }

        internal void AddError(string text)
        {
            AddMessage(text, Message.Kind.Error);
        }

        internal void AddDebug(string text)
        {
            AddMessage(text, Message.Kind.Debug);
        }

        internal void AddWarning(string text)
        {
            AddMessage(text, Message.Kind.Warning);
        }
    }
}
