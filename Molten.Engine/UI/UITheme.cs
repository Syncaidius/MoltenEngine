using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UITheme
    {
        [DataMember]
        public Color TextColor { get; set; } = Color.White;

        [DataMember]
        public Color BackgroundColor { get; set; } = new Color(40, 40, 150, 200);

        [DataMember]
        public Color BorderColor { get; set; } = new Color(80, 80, 190);

        [DataMember]
        public float BorderThickness { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the default font path, or name of a system font.
        /// </summary>
        [DataMember]
        public string FontName { get; set; } = "Arial";

        /// <summary>
        /// Gets or sets the default font size.
        /// </summary>
        [DataMember]
        public int FontSize { get; set; } = 16;

        /// <summary>
        /// Gets or sets the default size (in spaces) of the a tab character.
        /// </summary>
        [DataMember]
        public int TabSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets the default font texture sheet size.
        /// </summary>
        [DataMember]
        public int FontTextureSize { get; set; } = 512;

        /// <summary>
        /// Gets or sets the default number of points per curve, when rendering font characters.
        /// </summary>
        [DataMember]
        public int FontPointsPerCurve { get; set; } = 12;

        /// <summary>
        /// Gets or sets the default font character padding.
        /// </summary>
        [DataMember]
        public int CharacterPadding { get; set; } = 2;

        public void RequestFont(Engine engine, ContentRequestHandler loadCallback)
        {
            ContentRequest cr = engine.Content.BeginRequest("");
            cr.Load<TextFont>(FontName);
            cr.OnCompleted += loadCallback;
            cr.Commit();
        }
    }
}
