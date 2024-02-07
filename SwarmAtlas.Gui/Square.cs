using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Gui
{
    public class Square : IMyDrawable
    {
        public Vector2 Center { get; set; } = Vector2.Zero;
        public float Size { get; set; } = 1;
        public Color Color { get; set; } = Color.Magenta;
        public float Thickness { get; set; } = 0.1f;
        public float MinThickness { get; set; } = 1;
        public bool Centered { get; set; } = false;

        public Square(Vector2 center, float size, Color color, float thickness = 0.1f, float minThickness = 1, bool centered = false)
        {
            Center = center;
            Size = size;
            Color = color;
            Thickness = thickness;
            Centered = centered;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset, float scale, int windowHeight)
        {
            Vector2 position;
            if (Centered)
            {
                position = Center - (Vector2.One * (Size / 2));
            }
            else
            {
                position = new Vector2(Center.X, Center.Y + Size);
            }
            spriteBatch.DrawRectangle(Scene.TranslateVector2(position, offset, scale, windowHeight), Vector2.One * Size * scale, Color, Math.Max(Thickness*scale, MinThickness));
        }
    }
}
