using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class ShaderCompilerContext
    {
        public IReadOnlyList<ShaderCompilerMessage> Messages { get; }

        public bool HasErrors { get; private set; }

        public ShaderSource Source { get; set; }

        List<ShaderCompilerMessage> _messages;

        public ShaderCompilerContext()
        {
            _messages = new List<ShaderCompilerMessage>();
            Messages = _messages.AsReadOnly();
        }

        public void AddMessage(string text, ShaderCompilerMessage.Kind type = ShaderCompilerMessage.Kind.Message)
        {
            _messages.Add(new ShaderCompilerMessage()
            {
                Text = $"[{type}] {text}",
                MessageType = type,
            });

            if (type == ShaderCompilerMessage.Kind.Error)
                HasErrors = true;
        }

        public void AddError(string text)
        {
            AddMessage(text, ShaderCompilerMessage.Kind.Error);
        }

        public void AddDebug(string text)
        {
            AddMessage(text, ShaderCompilerMessage.Kind.Debug);
        }

        public void AddWarning(string text)
        {
            AddMessage(text, ShaderCompilerMessage.Kind.Warning);
        }

        public abstract void ParseFlags(ShaderCompileFlags flags);
    }
}