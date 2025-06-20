using System;
using System.Collections.Generic;
using System.Numerics;
using MonoGame.Framework.Graphics;

namespace MonoGame.Framework
{
    /// <summary>
    /// Sprite batch extensions for drawing primitive shapes
    /// </summary>
    public static class SpriteBatchShapeExtensions
    {
        private static Texture2D _whitePixelTexture;

        public static Texture2D GetWhitePixelTexture(GraphicsDevice graphicsDevice)
        {
            if (_whitePixelTexture == null)
            {
                _whitePixelTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Rgba32);
                _whitePixelTexture.SetData(stackalloc[] { Color.White });
            }
            return _whitePixelTexture;
        }

        /// <summary>
        /// Draws a closed polygon from an array of points
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="offset">Where to offset the points</param>
        /// <param name="points">The points to connect with lines</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the lines</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawPolygon(
           this SpriteBatch spriteBatch, Vector2 offset, IEnumerable<Vector2> points, Color color,
           float thickness = 1f, float layerDepth = 0, bool connect = true)
        {
            DrawPolygon(spriteBatch, offset, points.GetEnumerator(), color, thickness, layerDepth, connect);
        }

        /// <summary>
        /// Draws a closed polygon from an array of points
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="offset">Where to offset the points</param>
        /// <param name="points">The points to connect with lines</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the lines</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawPolygon<TEnumerator>(
            this SpriteBatch spriteBatch, Vector2 offset, TEnumerator points, Color color,
            float thickness = 1f, float layerDepth = 0, bool connect = true)
            where TEnumerator : IEnumerator<Vector2>
        {
            if (!points.MoveNext())
                return;
            Vector2 first = points.Current;

            if (!points.MoveNext())
                return;
            Vector2 last = first;

            var texture = GetWhitePixelTexture(spriteBatch.GraphicsDevice);
            do
            {
                var p1 = last + offset;
                var p2 = points.Current + offset;
                DrawPolygonEdge(spriteBatch, texture, p1, p2, color, thickness, layerDepth);
                last = points.Current;

            } while (points.MoveNext());

            if (connect)
            {
                var p1 = last + offset;
                var p2 = first + offset;
                DrawPolygonEdge(spriteBatch, texture, p1, p2, color, thickness, layerDepth);
            }
        }

        private static void DrawPolygonEdge(
            SpriteBatch spriteBatch, Texture2D texture, Vector2 point1, Vector2 point2, Color color,
            float thickness, float layerDepth)
        {
            var angle = MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            var length = Vector2.Distance(point1, point2);
            var scale = new Vector2(length, thickness);

            spriteBatch.Draw(texture, point1, null, color, angle, Vector2.Zero, scale, SpriteFlip.None, layerDepth);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void FillRectangle(
            this SpriteBatch spriteBatch, Vector2 location, SizeF size, Color color, float layerDepth = 0)
        {
            var tex = GetWhitePixelTexture(spriteBatch.GraphicsDevice);
            spriteBatch.Draw(tex, location, null, color, 0, Vector2.Zero, size, SpriteFlip.None, layerDepth);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="rectangle">The rectangle to draw</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void FillRectangle(
            this SpriteBatch spriteBatch, RectangleF rectangle, Color color, float layerDepth = 0)
        {
            FillRectangle(spriteBatch, rectangle.Position, rectangle.Size, color, layerDepth);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x">The X coord of the left side</param>
        /// <param name="y">The Y coord of the upper side</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void FillRectangle(
            this SpriteBatch spriteBatch, float x, float y, float width, float height, Color color, float layerDepth = 0)
        {
            FillRectangle(spriteBatch, new Vector2(x, y), new SizeF(width, height), color, layerDepth);
        }

        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="rectangle">The rectangle to draw</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="thickness">The thickness of the lines</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawRectangle(
            this SpriteBatch spriteBatch, RectangleF rectangle, Color color, float thickness = 1f, float layerDepth = 0)
        {
            var texture = GetWhitePixelTexture(spriteBatch.GraphicsDevice);
            var topLeft = new Vector2(rectangle.X, rectangle.Y);
            var topRight = new Vector2(rectangle.Right - thickness, rectangle.Y);
            var bottomLeft = new Vector2(rectangle.X, rectangle.Bottom - thickness);
            var horizontalScale = new Vector2(rectangle.Width, thickness);
            var verticalScale = new Vector2(thickness, rectangle.Height);

            spriteBatch.Draw(texture, topLeft, null, color, 0f, Vector2.Zero, horizontalScale, SpriteFlip.None, layerDepth);
            spriteBatch.Draw(texture, topLeft, null, color, 0f, Vector2.Zero, verticalScale, SpriteFlip.None, layerDepth);
            spriteBatch.Draw(texture, topRight, null, color, 0f, Vector2.Zero, verticalScale, SpriteFlip.None, layerDepth);
            spriteBatch.Draw(texture, bottomLeft, null, color, 0f, Vector2.Zero, horizontalScale, SpriteFlip.None, layerDepth);
        }

        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="color">The color to draw the rectangle in</param>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawRectangle(
            this SpriteBatch spriteBatch, Vector2 location, SizeF size, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            var rect = new RectangleF(location, size);
            DrawRectangle(spriteBatch, rect, color, thickness, layerDepth);
        }


        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        public static void DrawRectangle(
            this SpriteBatch spriteBatch, float x, float y, float width, float height, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            DrawRectangle(spriteBatch, new RectangleF(x, y, width, height), color, thickness, layerDepth);
        }

        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x1">The X coord of the first point</param>
        /// <param name="y1">The Y coord of the first point</param>
        /// <param name="x2">The X coord of the second point</param>
        /// <param name="y2">The Y coord of the second point</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawLine(
            this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness, layerDepth);
        }

        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="point">The starting point</param>
        /// <param name="length">The length of the line</param>
        /// <param name="angle">The angle of this line from the starting point</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawLine(
            this SpriteBatch spriteBatch, Vector2 point, float length, float angle, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            var tex = GetWhitePixelTexture(spriteBatch.GraphicsDevice);
            var origin = new Vector2(0f, 0.5f);
            var scale = new Vector2(length, thickness);

            spriteBatch.Draw(
                tex, point, null, color, angle, origin, scale, SpriteFlip.None, layerDepth);
        }

        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <param name="color">The color to use</param>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawLine(
            this SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            // calculate the distance between the two vectors
            float distance = Vector2.Distance(point1, point2);

            // calculate the angle between the two vectors
            float angle = MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            DrawLine(spriteBatch, point1, distance, angle, color, thickness, layerDepth);
        }

        /// <summary>
        /// Draws a point at the specified position. The center of the point will be at the position.
        /// </summary>
        public static void DrawPoint(
            this SpriteBatch spriteBatch, Vector2 position, Color color,
            float size = 1f, float layerDepth = 0)
        {
            var texture = GetWhitePixelTexture(spriteBatch.GraphicsDevice);
            var offset = new Vector2(0.5f - size * 0.5f);

            spriteBatch.Draw(
                texture, position + offset, null, color, 0f, Vector2.Zero, size, SpriteFlip.None, layerDepth);
        }

        /// <summary>
        /// Draws a point at the specified x, y position. The center of the point will be at the position.
        /// </summary>
        public static void DrawPoint(
            this SpriteBatch spriteBatch, float x, float y, Color color,
            float size = 1f, float layerDepth = 0)
        {
            DrawPoint(spriteBatch, new Vector2(x, y), color, size, layerDepth);
        }

        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="center">Center of the ellipse</param>
        /// <param name="radius">Radius of the ellipse</param>
        /// <param name="sides">The number of sides to generate.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="thickness">The thickness of the line around the ellipse.</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawEllipse(
            this SpriteBatch spriteBatch, Vector2 center, Vector2 radius, int sides, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            var points = new EllipseEnumerable(radius, sides);
            DrawPolygon(spriteBatch, center, points, color, thickness, layerDepth);
        }

        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="center">Center of the ellipse</param>
        /// <param name="radius">Radius of the ellipse</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="count">The amount of sides to draw</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="start">The starting rotation of the circle</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawSlicedEllipse(
            this SpriteBatch spriteBatch, Vector2 center, Vector2 radius, int sides, int count, Color color,
            float thickness = 1f, float start = -MathF.PI / 2, float layerDepth = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            var points = new EllipseEnumerable(radius, sides, count + 1, start);
            bool connect = count - 1 == sides;
            DrawPolygon(spriteBatch, center, points, color, thickness, layerDepth, connect);
        }

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="thickness">The thickness of the lines used</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawCircle(
            this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            DrawEllipse(spriteBatch, center, new Vector2(radius), sides, color, thickness, layerDepth);
        }

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="x">The center X of the circle</param>
        /// <param name="y">The center Y of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="thickness">The thickness of the line</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawCircle(
            this SpriteBatch spriteBatch, float x, float y, float radius, int sides, Color color,
            float thickness = 1f, float layerDepth = 0)
        {
            DrawCircle(spriteBatch, new Vector2(x, y), radius, sides, color, thickness, layerDepth);
        }

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="spriteBatch">The destination drawing surface</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="count">The amount of sides to draw</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="thickness">The thickness of the lines used</param>
        /// <param name="start">The starting rotation of the circle</param>
        /// <param name="layerDepth">The depth of the layer of this shape</param>
        public static void DrawSlicedCircle(
            this SpriteBatch spriteBatch, Vector2 center, float radius, int sides, int count, Color color,
            float thickness = 1f, float start = -MathF.PI / 2, float layerDepth = 0)
        {
            DrawSlicedEllipse(
                spriteBatch, center, new Vector2(radius), sides, count, color, start, thickness, layerDepth);
        }
    }
}