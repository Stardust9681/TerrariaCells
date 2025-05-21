using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace TerrariaCells.Common.Utilities
{
	public static class Drawing
	{
		//Vanilla DrawLine didn't play well with being BEEEG
		public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color sColor, Color eColor, int lineWidth, int divisions = 25)
		{
			float len = (end - start).Length();
			if (float.IsNaN(len) || len == 0)
				return;
			float segmentDist = len / divisions;
			Vector2 direction = (end - start) / len;
			float rotation = direction.ToRotation();
			Vector2 drawPos = start - Main.screenPosition;
			drawPos += new Vector2(direction.Y, -direction.X) * lineWidth * 0.5f;
			Rectangle drawRect = new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)segmentDist, lineWidth);
			for (float i = 0; i < divisions; i++)
			{
				float lerpVal = i / divisions;
				Color drawCol = Color.Lerp(sColor, eColor, lerpVal);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, drawRect, null, drawCol, rotation, Vector2.Zero, SpriteEffects.None, 0f);
				drawRect.X += (int)(direction.X * segmentDist);
				drawRect.Y += (int)(direction.Y * segmentDist);
			}
		}

        static readonly Vector2 normaliseVector = new(1, 0.001f);

        // Yes, start and end are both inclusive, and will be fixed automatically if they're the wrong way round
        public static void highlightTileRegion(
            SpriteBatch spriteBatch,
            Point start,
            Point end,
            Color colour
        ) {
            var r = new Rectangle(
                (int) MathF.Min(start.X, end.X),
                (int) MathF.Min(start.Y, end.Y),
                (int) MathF.Abs(start.X - end.X) + 1,
                (int) MathF.Abs(start.Y - end.Y) + 1
            );
            var pos = r.TopLeft() * 16 - Main.screenPosition;
            var size = r.Size() * 16;
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos,
                null,
                colour * 0.6f,
                0,
                Vector2.Zero,
                size * normaliseVector,
                SpriteEffects.None,
                0
            );
            // Draw borders
            var vScale = new Vector2(2, size.Y) * normaliseVector;
            var hScale = new Vector2(size.X, 2) * normaliseVector;
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos - Vector2.UnitX * 2,
                null,
                colour,
                0,
                Vector2.Zero,
                vScale,
                SpriteEffects.None,
                0
            );
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos - Vector2.UnitY * 2,
                null,
                colour,
                0,
                Vector2.Zero,
                hScale,
                SpriteEffects.None,
                0
            );
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos + Vector2.UnitX * size.X,
                null,
                colour,
                0,
                Vector2.Zero,
                vScale,
                SpriteEffects.None,
                0
            );
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos + Vector2.UnitY * size.Y,
                null,
                colour,
                0,
                Vector2.Zero,
                hScale,
                SpriteEffects.None,
                0
            );
        }

        public static void highlightTileRegion(
            SpriteBatch spriteBatch,
            Point start,
            Color colour,
            int width = 0,
            int height = 0
        ) {
            highlightTileRegion(
                spriteBatch,
                start,
                start + new Point(width, height),
                colour
            );
        }
    }
}
