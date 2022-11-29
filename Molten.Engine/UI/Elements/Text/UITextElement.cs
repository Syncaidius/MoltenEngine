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

            Caret = new UITextCaret(this);
            DefaultFontName = settings.DefaultFontName;
            _parser = settings.DefaultTextParser ?? new UIDefaultTextParser();
        }

        /// <summary>
        /// Inserts a blank new <see cref="UITextLine"/>.
        /// </summary>
        /// <returns></returns>
        public abstract UITextLine NewLine();

        /// <summary>
        /// Inserts the given <see cref="UITextLine"/> at the end of the current <see cref="UITextElement"/>'s text.
        /// </summary>
        /// <param name="line">The line to append to the end.</param>
        public abstract void AppendLine(UITextLine line);

        /// <summary>
        /// Inserts the given <see cref="UITextSegment"/> to the end of the last <see cref="UITextLine"/>, in the current <see cref="UITextElement"/>.
        /// </summary>
        /// <param name="segment">The segment to append to the end.</param>
        public abstract void AppendSegment(UITextSegment segment);

        /// <summary>
        /// Inserts a <see cref="UITextLine"/> after the specified one.
        /// </summary>
        /// <param name="line">The line to be inserted.</param>
        /// <param name="insertAfter">The line to insert <paramref name="line"/> after.</param>
        public abstract void InsertLine(UITextLine line, UITextLine insertAfter);

        /// <summary>
        /// Clear all text from the current <see cref="UITextElement"/>.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Retrieves the full text string of the current <see cref="UITextElement"/>.
        /// </summary>
        /// <returns></returns>
        public abstract string GetText();

        /// <summary>
        /// Forces the current <see cref="UITextElement"/> to recalculate any peripherial values, such as scrollbars or effects.
        /// </summary>
        public abstract void Recalculate();

        /// <summary>
        /// Sets the text of the current <see cref="UITextElement"/>. The string will be parsed by the <see cref="UITextParser"/> at <see cref="Parser"/>.
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            Clear();

            if (MaxLength > 0 && text.Length > MaxLength)
                text = text.Substring(0, MaxLength);

            _parser.ParseText(this, text);
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);
            Caret.Update(time);
        }

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
        public int MaxLength { get; set; } = 0;

        /// <summary>
        /// Gets the <see cref="UITextCaret"/> bound to the current <see cref="UITextElement"/>.
        /// </summary>
        public UITextCaret Caret { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="UITextParser"/> of the current <see cref="UITextElement"/>.
        /// </summary>
        public UITextParser Parser
        {
            get => _parser;
            set
            {
                value = value ?? Engine.Settings.UI.DefaultTextParser;
                if(_parser != value)
                {
                    _parser = value;
                    Clear();
                    string text = GetText();
                    _parser.ParseText(this, text);
                }
            }
        }
    }
}
