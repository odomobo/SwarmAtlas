using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmAtlas.Gui
{
    public interface IMyDrawable
    {
        /// <summary>
        /// Transform any particular point with the formula: t(p) = (p-offset)*scale
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        void Draw(SpriteBatch spriteBatch, Vector2 offset, float scale, int windowHeight);
    }
    public class Scene
    {
        // TODO: put this stuff somewhere else
        public static Vector2 TranslateVector2(Vector2 point, Vector2 offset, float scale, int windowHeight)
        {
            point = new Vector2(point.X, point.Y);
            var transformedPoint = (point - offset) * scale;
            return new Vector2(transformedPoint.X, windowHeight - transformedPoint.Y);
        }

        public static Vector2 ScreenPointToWorldVector2(Point screenPoint, Vector2 offset, float scale, int windowHeight)
        {
            screenPoint = new Point(screenPoint.X, windowHeight - screenPoint.Y);
            var screenspaceVec2 = screenPoint.ToVector2();
            var scaledUnoffsetWorldspaceVec2 = screenspaceVec2 / scale;
            var worldspaceVec2 = scaledUnoffsetWorldspaceVec2 + offset;
            return new Vector2(worldspaceVec2.X, worldspaceVec2.Y);
        }

        // TODO: do we need this????
        //public static Point TranslatePoint(Point point, Vector2 offset, float scale)
        //{
        //    return TranslateVector2(point.ToVector2(), offset, scale).ToPoint();
        //}
        //
        //public static Rectangle TranslateRectangle(Rectangle rect, Vector2 offset, float scale)
        //{
        //    return new Rectangle(TranslatePoint(rect.Location, offset, scale), rect.Size * scale);
        //}

        public Vector2 MapExtents = new Vector2(200, 180);

        public List<IMyDrawable> MapObjects { get; set; } = new List<IMyDrawable>();
        // TODO: other layers
    }
}
