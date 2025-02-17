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
	}
}
