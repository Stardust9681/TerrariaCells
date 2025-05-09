using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
	public partial class Casters : GlobalNPC
	{
		public bool DesertSpiritDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Asset<Texture2D> t = TextureAssets.Npc[npc.type];
			for (int i = 0; i < npc.oldPos.Length; i++)
			{
				spriteBatch.Draw(t.Value, npc.oldPos[i] - screenPos + new Vector2(npc.width, npc.height) / 2, new Rectangle(0, npc.frame.Y, npc.frame.Width, npc.frame.Height), drawColor * 0.5f * (1 - (float)i / npc.oldPos.Length), npc.rotation, new Vector2(npc.frame.Width, npc.frame.Height) / 2, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			}
			spriteBatch.Draw(t.Value, npc.position - screenPos + new Vector2(npc.width, npc.height) / 2, new Rectangle(0, CustomFrameY, npc.frame.Width, npc.frame.Height), drawColor * 0.9f, npc.rotation, new Vector2(npc.frame.Width, npc.frame.Height) / 2, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
			return false;
		}
		public void DesertSpiritFrame(NPC npc)
		{
			int min = npc.ai[3] > 60 && npc.ai[3] < 220 ? 8 : 0;
			CustomFrameCounter++;
			if (CustomFrameCounter >= 5)
			{
				CustomFrameCounter = 0;
				CustomFrameY += 64;
				if (CustomFrameY >= 64 * (min + 8))
				{

					CustomFrameY = min * 64;
				}
			}
		}
		public bool DesertSpiritAI(NPC npc, Player? target)
		{
			const int TimeRotating = 360;

			bool validTarget;
			if (target != null)
				validTarget = npc.TargetInAggroRange(target, 448, false);
			else
				validTarget = npc.TargetInAggroRange(448, false);

			if (npc.ai[1] == 0)
			{
				npc.ai[1] = Pack(npc.Center.ToTileCoordinates16());
				//npc.ai[1] = npc.Center.X;
				//npc.ai[2] = npc.Center.Y;
			}
			if (npc.HasValidTarget)
			{
				npc.direction = npc.Center.X > target.Center.X ? -1 : 1;
				npc.spriteDirection = npc.direction;
			}

			// ===== Removed Teleport Code =====
			// Sorbet's discretion, says it doesn't fit the level
			// Read here: https://discord.com/channels/1260223010728706169/1260224973549736006/1326664401742467074
			/*
			if (npc.ai[0] > 300 && npc.ai[3] <= 0)
			{
				Vector2 tpos = npc.Center;
				if (npc.HasValidTarget)
				{
					tpos = target.Center;
				}

				for (int i = 0; i < 30; i++)
				{
					Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame);
				}

				Teleport(npc, tpos, 200);
				SoundEngine.PlaySound(SoundID.Item8, npc.Center);
				for (int i = 0; i < 30; i++)
				{
					Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame);
				}
				Vector2 rotpos = npc.Center - new Vector2(0, 15);
				npc.ai[0] = 0;
				npc.ai[1] = rotpos.X;
				npc.ai[2] = rotpos.Y;
				npc.ai[3] = TimeRotating;
			}
			*/

			(ushort x, ushort y) = Unpack(npc.ai[1]);
			Vector2 worldPos = new Terraria.DataStructures.Point16(x, y).ToWorldCoordinates();
			worldPos.Y -= npc.height;
			for (int i = 0; i < 2 * npc.height; i += 16)
			{
				Tile tile = Framing.GetTileSafely(x+(i/16), y);
				if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType])
				{
					worldPos.Y += i;
					break;
				}
			}
			npc.Center = worldPos + new Vector2(MathF.Sin(MathHelper.ToRadians(npc.ai[3] * 2)) * 24, MathF.Sin(MathHelper.ToRadians(npc.ai[3] * 5)) * 8);

			if (validTarget)
			{
				if (npc.ai[3] < 200 && (int)npc.ai[3] % 10 == 0 && npc.HasValidTarget && npc.ai[3] > 100)
				{
					Projectile fire = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), target.Center + (Vector2.UnitX * Main.rand.Next(10, 25)).RotatedByRandom(MathHelper.TwoPi), Vector2.Zero, ProjectileID.DesertDjinnCurse, 0, 1, -1, -1, 0, npc.whoAmI);
					fire.timeLeft -= 50;
					fire.localAI[0] = 0.05f;
					fire.velocity = Vector2.Zero;
				}
			}

			npc.ai[3]--;
			if (npc.ai[3] < 1) npc.ai[3] = TimeRotating;

			return false;
		}
	}
}
