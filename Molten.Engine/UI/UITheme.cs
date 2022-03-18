using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
    }
}
