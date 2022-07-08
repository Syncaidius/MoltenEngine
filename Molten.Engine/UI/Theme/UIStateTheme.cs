using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIStateTheme
    {
        [DataMember]
        public Color TextColor { get; set; } = Color.White;

        [DataMember]
        public Color BackgroundColor { get; set; } = new Color(0, 109, 155, 200);

        [DataMember]
        public Color BorderColor { get; set; } = new Color(52, 189, 235, 255);


        [DataMember]
        public float BorderThickness { get; set; } = 2f;

        [DataMember]
        public CornerInfo CornerRadius { get; set; } = new CornerInfo(8f);

        /// <summary>
        /// Gets or sets the default size (in spaces) of the a tab character.
        /// </summary>
        [DataMember]
        public int TabSize { get; set; } = 3;


        [DataMember]
        public UIVerticalAlignment VerticalAlign { get; set; }

        [DataMember]
        public UIHorizonalAlignment HorizontalAlign { get; set; }

        /// <summary>
        /// Sets the current <see cref="UIStateTheme"/> by copying all of the values from another <see cref="UIStateTheme"/>.
        /// </summary>
        /// <param name="source">The source <see cref="UIStateTheme"/> from which to copy values.</param>
        public void Set(UIStateTheme source)
        {
            TextColor = source.TextColor;
            BackgroundColor = source.BackgroundColor;
            BorderColor = source.BorderColor;
            BorderThickness = source.BorderThickness;
            CornerRadius = source.CornerRadius;
            TabSize = source.TabSize;
            VerticalAlign = source.VerticalAlign;
            HorizontalAlign = source.HorizontalAlign;
        }
    }
}
