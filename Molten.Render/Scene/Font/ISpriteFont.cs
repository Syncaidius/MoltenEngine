using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISpriteFont
    {
        Rectangle GetCharRect(char c);

        int FontSize { get; }

        string FontName { get; }

        ITexture2D UnderlyingTexture { get; }
    }
}
