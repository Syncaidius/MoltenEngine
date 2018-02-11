using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteFont
    {
        ITexture2D _sheet;

        public SpriteFont(IRenderer renderer, string fontName, int size, FontSource source = FontSource.Path)
        {
            switch (source)
            {
                case FontSource.Path:

                    break;

                case FontSource.System:
                    throw new NotImplementedException();
                    break;
            }
        }

        /// <summary>Gets the underlying <see cref="ITexture2D"/> which contains the font sheet.</summary>
        public ITexture2D SheetTexture => _sheet;
    }
}
