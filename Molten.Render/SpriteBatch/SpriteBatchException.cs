using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteBatcherException : Exception
    {
        public SpriteBatcherException(SpriteBatcher sb, string message) : base(message)
        {
            Batcher = sb;
        } 

        public SpriteBatcher Batcher { get; private set; }
    }
}
