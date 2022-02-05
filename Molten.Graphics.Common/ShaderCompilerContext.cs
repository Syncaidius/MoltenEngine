using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class ShaderCompilerContext
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

        public IReadOnlyList<Message> Messages { get; }

        internal bool HasErrors { get; private set; }

        List<Message> _messages;

        public ShaderCompilerContext()
        {
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
