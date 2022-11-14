using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// A base class for implementing text-based UI elements.
    /// </summary>
    public abstract class UITextElement : UIElement
    {
        string _fontName;
        UITextParser _parser;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);
            DefaultFontName = settings.DefaultFontName;
            _parser = settings.DefaultTextParser ?? new UIDefaultTextParser();
        }

        public abstract UITextLine NewLine();

        public abstract void AppendLine(UITextLine line);

        public abstract void AppendSegment(UITextSegment segment);

        public abstract void InsertLine(UITextLine line, int lineNumber);

        public abstract void InsertLine(UITextLine line, UITextLine insertAfter);

        /// <summary>
        /// Gets or sets the name of the default font for the current <see cref="UITextBox"/>. This will attempt to load/retrieve and populate <see cref="Font"/>.
        /// </summary>
        [UIThemeMember]
        public string DefaultFontName
        {
            get => _fontName;
            set
            {
                value = (value ?? string.Empty).ToLower();
                if (_fontName != value)
                {
                    _fontName = value;
                    if (!string.IsNullOrWhiteSpace(_fontName))
                    {
                        Engine.Content.LoadFont(_fontName, (font, isReload) =>
                        {
                            DefaultFont = font;
                            DefaultLineHeight = (int)Math.Ceiling(DefaultFont.MeasureString(" ").Y);
                        },
                        new SpriteFontParameters()
                        {
                            FontSize = 16,
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Gets the default <see cref="SpriteFont"/> for the current <see cref="UITextElement"/>. This is controlled by setting <see cref="DefaultFontName"/>
        /// </summary>
        public SpriteFont DefaultFont { get; private set; }

        /// <summary>
        /// Gets the default line height of the current <see cref="UITextElement"/>. 
        /// <para>This is based off the <see cref="DefaultFont"/>, which is controlled by setting <see cref="DefaultFontName"/>.</para>
        /// </summary>
        public int DefaultLineHeight { get; private set; }


        /// <summary>
        /// Gets or sets whether the current <see cref="UITextBox"/> is a multi-line textbox. If false, any line breaks will be substituted with spaces.
        /// </summary>
        public abstract bool IsMultiLine { get; }

        /// <summary>
        /// Gets or sets the maximum number of characters that can be entered into the current <see cref="UITextElement"/>.
        /// </summary>
        public ulong MaxCharacters { get; set; } = 0;
    }
}
