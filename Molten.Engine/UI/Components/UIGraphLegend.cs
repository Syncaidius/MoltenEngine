using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.IO;
using Molten.Graphics;
using Molten.Rendering;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIGraphLegend : UIComponent
    {
        UILineGraph _graph;
        Color _borderColor = new Color(20, 20, 20, 255);
        SpriteFont _font;
        int _fontSize;
        string _fontName;
        int _keySize = 20;
        int _spacing = 5;
        string _numberFormat = "N2";
        string _title;

        public UIGraphLegend(UISystem ui) : base(ui)
        {
            _fontName = _ui.DefaultFontName;
            _fontSize = _ui.DefaultFontSize;
            GetFont();
        }


        private void GetFont()
        {
            _font = SpriteFont.Create(_ui.Engine.GraphicsDevice, _fontName, _fontSize);
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            
        }

        protected override void OnRender(SpriteBatch sb, RenderProxy proxy)
        {
            IntVector2 textPos = new IntVector2(_clippingBounds.X + 5, _clippingBounds.Y);
            sb.DrawString(_font, _title, textPos, Color.White);


            if (_graph != null && _graph.Sets.Count > 0)
            {
                Rectangle borderRect = new Rectangle(textPos.X, textPos.Y + 20, _keySize, _keySize);
                Rectangle keyRect = borderRect;
                textPos = new IntVector2(borderRect.X + _keySize + _spacing, keyRect.Y);

                keyRect.Inflate(-2);

                _graph.Sets.ForInterlock(0, 1, (id, s) =>
                {
                    if (s.Points.Count == 0)
                        return false;

                    sb.Draw(borderRect, _borderColor);
                    sb.Draw(keyRect, s.Color);

                    int last = s.Points.Count - 1;
                    sb.DrawString(_font, s.Name + ": " + s.Points[last].ToString(_numberFormat), textPos, Color.White);

                    keyRect.Y += borderRect.Height + _spacing;
                    borderRect.Y += borderRect.Height + _spacing;
                    textPos.Y += borderRect.Height + _spacing;
                    return false;
                });
            }

            base.OnRender(sb, proxy);
        }

        /// <summary>Gets or sets the source graph of the legend.</summary>
        public UILineGraph SourceGraph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        /// <summary>Gets or sets the border color of legend keys.</summary>
        [DataMember]
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        /// <summary>Gets or sets the legend key size.</summary>
        [DataMember]
        public int KeySize
        {
            get { return _keySize; }
            set { _keySize = value; }
        }

        /// <summary>Gets or sets the spacing between legend entries and their labels.</summary>
        [DataMember]
        public int KeySpacing
        {
            get { return _spacing; }
            set { _spacing = value; }
        }

        /// <summary>Gets or sets the font by name, for the legend text.</summary>
        public string FontName
        {
            get { return _fontName; }
            set
            {
                _fontName = value;
                GetFont();
            }
        }

        /// <summary>Gets or sets the font size for the legend text.</summary>
        [DataMember]
        public int FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                GetFont();
            }
        }

        [DataMember]
        /// <summary>Gets or sets the number format for displaying the latest value of 
        /// each <see cref="UILineGraph.DataSet"/>. The default is N2.</summary>
        public string NumberFormat
        {
            get { return _numberFormat; }
            set { _numberFormat = value; }
        }

        [DataMember]
        /// <summary>Gets or sets the title of the legend.</summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }
    }
}
