using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteBatchException : Exception
    {
        public SpriteBatchException(SpriteBatch sb, string message) : base(message)
        {
            SpriteBatch = sb;
        } 

        public SpriteBatch SpriteBatch { get; private set; }
    }
}
