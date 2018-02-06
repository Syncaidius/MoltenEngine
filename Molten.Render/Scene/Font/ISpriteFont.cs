using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISpriteFont
    {
        /// <summary>Returns the size of a string of text, in pixels, if it was drawn with this font. 
        /// <remarks>Does not currently support line breaks.</remarks></summary>
        /// <param name="text">The string of text to measure.</param>
        /// <returns></returns>
        Vector2 MeasureString(string text);

        /// <summary>Returns the size of a string of text, in pixels, if it was drawn with this font. 
        /// <remarks>Does not currently support line breaks.</remarks></summary>
        /// <param name="text">The string of text to measure.</param>
        /// <param name="maxLength">The maximum number of characters to measure.</param>
        /// <returns></returns>
        Vector2 MeasureString(string text, int maxLength);

        /// <summary>Returns the size of a string of text, in pixels, if it was drawn with this font. 
        /// <remarks>Does not currently support line breaks.</remarks></summary>
        /// <param name="text">The string of text to measure.</param>
        /// <param name="startIndex">The character at which to start measuring within the string of text.</param>
        /// <param name="length">The number of characters to measure from the start index.</param>
        /// <returns></returns>
        Rectangle MeasureString(string text, int startIndex, int length);

        Rectangle GetCharRect(char c);

        /// <summary>A helper method which return the index of the nearest character within a string of text, if it was to be rendered with the current font.</summary>
        /// <param name="text">The text to test against.</param>
        /// <param name="localPosition">The location position, relative to the string at position 0,0.</param>
        /// <returns></returns>
        int NearestCharacter(string text, Vector2 localPosition);

        /// <summary>Gets the font size.</summary>
        int FontSize { get; }

        /// <summary>Gets the font name.</summary>
        string FontName { get; }

        /// <summary>Gets the underlying <see cref="ITexture2D"/> which holds all of the character sprites for the current <see cref="ISpriteFont"/>.</summary>
        ITexture2D UnderlyingTexture { get; }
    }
}
