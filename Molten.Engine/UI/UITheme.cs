using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UITheme
    {
        public class ColorSet
        {
            [DataMember]
            public Color Text { get; set; } = Color.White;

            [DataMember]
            public Color Background { get; set; } = new Color(40, 40, 150, 200);

            [DataMember]
            public Color Border { get; set; } = new Color(80, 80, 190);
        }

        /// <summary>
        /// Gets or sets the default colors used when representing a a normal, unmodified state of functionality
        /// </summary>
        [DataMember]
        public ColorSet DefaultColors { get; set; } = new ColorSet();

        /// <summary>
        /// Gets or sets the colors used when represending a hover action. e.g. mouse or touch-hold gesture.
        /// </summary>
        [DataMember]
        public ColorSet HoverColors { get; set; } = new ColorSet();

        /// <summary>
        /// Gets or sets the colors used when representing a click, press or touch interaction.
        /// </summary>
        [DataMember]
        public ColorSet PressColors { get; set; } = new ColorSet();

        /// <summary>
        /// Gets or sets the colors used by elements to represent a disabled state of functionality.
        /// </summary>
        [DataMember]
        public ColorSet DisabledColors { get; set; } = new ColorSet();

        /// <summary>
        /// Gets or sets the colors used when representing active or selected functionality.
        /// </summary>
        [DataMember]
        public ColorSet ActiveColors { get; set; } = new ColorSet();


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
