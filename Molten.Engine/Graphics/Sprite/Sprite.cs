namespace Molten.Graphics
{
    public class Sprite
    {
        public SpriteData Data = new SpriteData();

        public Vector2F Position { get; set; }

        public float Rotation { get; set; }

        public Vector2F Scale { get; set; } = new Vector2F(1);

        public Vector2F Origin { get; set; }

        public IMaterial Material { get; set; }
    }
}
