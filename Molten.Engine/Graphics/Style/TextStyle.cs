using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextStyle
    {
        public static readonly TextStyle Default = new TextStyle(Color.White);

        public Color FillColor;

        public Color OutlineColor;

        public Color DropShadowColor;

        public float OutlineSize;

        public float DropShadowSize;

        public Vector2F DropShadowDirection;

        public TextStyle()
        {
            FillColor = Color.White;
            OutlineColor = Color.White;
            DropShadowColor = Color.White;
            OutlineSize = 0f;
            DropShadowSize = 0;
            DropShadowDirection = Vector2F.One;
        }

        public TextStyle(Color fillColor)
        {
            FillColor = fillColor;
            OutlineColor = Color.White;
            DropShadowColor = Color.White;
            OutlineSize = 0f;
            DropShadowSize = 0;
            DropShadowDirection = Vector2F.One;
        }

        public TextStyle(Color fillColor, Color outlineColor, float outlineSize)
        {
            FillColor = fillColor;
            OutlineColor = outlineColor;
            DropShadowColor = Color.White;
            OutlineSize = outlineSize;
            DropShadowSize = 0;
            DropShadowDirection = Vector2F.One;
        }

        public TextStyle(Color fillColor, Color outlineColor, float outlineSize, Color dropShadowColor, float dropShadowSize, Vector2F dropShadowDir)
        {
            FillColor = fillColor;
            OutlineColor = outlineColor;
            DropShadowColor = dropShadowColor;
            OutlineSize = outlineSize;
            DropShadowSize = dropShadowSize;
            DropShadowDirection = dropShadowDir.GetNormalized();
        }

        public static implicit operator TextStyle(Color color)
        {
            return new TextStyle()
            {
                FillColor = color,
                OutlineColor = Color.White,
                DropShadowColor = Color.White,
                OutlineSize = 0f,
                DropShadowSize = 0,
                DropShadowDirection = Vector2F.One,
            };
        }

        public static implicit operator Color(TextStyle style)
        {
            return style.FillColor;
        }
    }
}
