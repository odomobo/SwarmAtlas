using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame;

namespace SwarmAtlas.Gui
{
    public class FilledRect : IMyDrawable
    {
        public Vector2 Location { get; set; } = Vector2.Zero;
        public Vector2 Size { get; set; } = Vector2.One;
        public Color Color { get; set; } = Color.Magenta;

        public FilledRect(Vector2 location, Vector2 size, Color color)
        {
            Location = location;
            Size = size;
            Color = color;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, float scale, int windowHeight)
        {
            var lowerLeft = new Vector2(Location.X, Location.Y + Size.Y);
            spriteBatch.FillRectangle(Scene.TranslateVector2(lowerLeft, offset, scale, windowHeight), Size * scale, Color);
        }
    }
}
