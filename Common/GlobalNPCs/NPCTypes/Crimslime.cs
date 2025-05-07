using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaCells.Common.Utilities;

using static TerrariaCells.Common.Utilities.NPCHelpers;
using static TerrariaCells.Common.Utilities.NumberHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	public class Crimslime : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType is NPCID.LittleCrimslime or NPCID.Crimslime or NPCID.BigCrimslime;
		}

		const int Idle = 0; //Ooze left/right passively
		const int Lunge = 1; //Lunge horizontally left/right
		const int Jump = 2;

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest(false);
			if (!npc.HasValidTarget)
			{
				ResetAI(npc);
			}

			switch ((int)npc.ai[1])
			{
				case Idle:
					IdleAI(npc);
					break;
				case Lunge:
					LungeAI(npc);
					break;
				case Jump:
					JumpAI(npc);
					break;
			}
		}

		private static void ResetAI(NPC npc)
		{
			npc.ai[0] = 0;
			npc.ai[1] = Idle;
			npc.ai[2] = 0;
			npc.ai[3] = 0;
			CombatNPC.ToggleContactDamage(npc, false);
		}
		//HorizontalMovement(npc, npc.ai[0], npc.ai[2] * 6f, Start, End);
		private static void HorizontalMovement(NPC npc, float timer, float velocity, int start, int end)
		{
			int Length = end - start;
			if (start < npc.ai[0] && npc.ai[0] < end)
			{
				npc.velocity.X = TCellsUtils.LerpFloat(0, velocity, timer, Length, TCellsUtils.LerpEasing.Bell, start, true);
			}
			else
			{
				npc.velocity.X *= 0.9f;
			}
			Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

			if (MathF.Abs(npc.velocity.X) > 1f)
			{
				if (Main.rand.NextFloat() < 0.4f)
				{
					Dust d = Dust.NewDustDirect(npc.BottomLeft - new Vector2(0, 4), npc.width, 2, DustID.Crimslime);
					d.noGravity = true;
					d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					d.scale = Main.rand.NextFloat(1f, 1.67f);
					d.velocity = npc.velocity * 0.2f;
					d.alpha = 50;
				}
			}
		}

		private void IdleAI(NPC npc)
		{
			if (npc.TryGetTarget(out Entity target) && npc.TargetInAggroRange(ToTileDist(24)))
			{
				ResetAI(npc);
				if (Distance(target.position.Y, npc.position.Y) > ToTileDist(4) || target.velocity.Y < 0)
				{
					npc.ai[1] = Jump;
				}
				else
				{
					npc.ai[1] = Lunge;
				}
				return;
			}

			if (npc.ai[0] == 0)
			{
				npc.ai[2] = Main.rand.NextDirection();
				npc.netUpdate = true;
			}

			const int Start = 25;
			const int End = 85;
			HorizontalMovement(npc, npc.ai[0], 3f * npc.ai[2], Start, End);
			npc.velocity.Y += 0.2f;
			if (npc.ai[0] > (End + Start))
			{
				ResetAI(npc);
				return;
			}

			npc.ai[0]++;
		}

		private void LungeAI(NPC npc)
		{
			if (!npc.TryGetTarget(out Entity target))
			{
				ResetAI(npc);
				return;
			}
			if (npc.ai[0] == 0)
			{
				npc.ai[2] = DirectionFromTo(npc.Center.X, target.Center.X);
			}

			const int Start = 25;
			const int End = 85;
			HorizontalMovement(npc, npc.ai[0], 5f * npc.ai[2], Start, End);
			npc.velocity.Y += 0.2f;
			CombatNPC.ToggleContactDamage(npc, MathF.Abs(npc.velocity.X) > 3);

			if (npc.ai[0] > (End + Start) && MathF.Abs(npc.velocity.X) < 0.1f)
			{
				ResetAI(npc);
				return;
			}

			npc.ai[0]++;
		}

		private void JumpAI(NPC npc)
		{
			if (!npc.TryGetTarget(out Entity target))
			{
				ResetAI(npc);
				return;
			}

			CombatNPC.ToggleContactDamage(npc, npc.velocity.Y > 0);

			const int Start = 45;
			if ((int)npc.ai[0] == Start)
			{
				npc.velocity.Y = -7f;
			}
			npc.velocity.Y += 0.18f;
			if (npc.velocity.Y > 0)
				npc.velocity.Y *= 1.01f;

			if (npc.ai[0] > Start)
			{
				const float NudgeForce = 0.28f;
				if (Distance(npc.position.X, target.position.X) > ToTileDist(3))
				{
					npc.velocity.X = MathHelper.Clamp(npc.velocity.X + DirectionFromTo(npc.position.X, target.position.X) * NudgeForce, -2.4f, 2.4f);
				}

				//NPC not moving horizontally
				//Direction To Target is not the same as Direction Of Motion
				if (MathF.Abs(npc.velocity.X) < (NudgeForce * 0.9f) || (DirectionFromTo(npc.position.X, target.position.X) * npc.velocity.X) < 0)
				{
					npc.velocity.X *= 0.5f;
					npc.velocity.Y = MathF.Min(npc.velocity.Y + 0.07f, 8f);
				}

				if (npc.collideY && npc.oldVelocity.Y > 0)
				{
					npc.velocity.X = 0;
					ResetAI(npc);
					return;
				}
			}

			if (npc.collideY && npc.oldVelocity.Y > npc.velocity.Y)
			{
				int dustCount = Main.rand.Next(6, 10);
				for (int i = 0; i < dustCount; i++)
				{
					Dust d = Dust.NewDustDirect(npc.BottomLeft, npc.width, 2, DustID.Crimslime);
					d.noGravity = true;
					d.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					d.scale = Main.rand.NextFloat(1f, 1.5f);
					d.velocity = new Vector2(Main.rand.NextDirection() * 3f, -3f).RotatedByRandom(MathHelper.ToRadians(15));
					d.alpha = 50;
				}
			}

			npc.ai[0]++;
		}

		public override bool FindFrame(NPC npc, int frameHeight)
		{
			int limit = 0;
			if (MathF.Abs(npc.velocity.X) < 1)
			{
				npc.ai[3]++;
				limit = 75;
			}
			if (MathF.Abs(npc.velocity.Y) > 1)
			{
				npc.ai[3]++;
				limit = 40;
			}

			if (npc.ai[3] > limit && limit != 0)
			{
				int newFrameY = npc.frame.Y + frameHeight;
				if (newFrameY >= frameHeight * Main.npcFrameCount[npc.type])
				{
					newFrameY = 0;
				}
				npc.ai[3] = 0;
				npc.frame.Y = newFrameY;
			}
			return false;
		}

		public override bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor)
		{
			if (npc.ai[1] != Jump)
			{
				//Approximate acceleration, adjust horizontal squish based on that
				float[] accelX = new float[npc.oldPos.Length - 2];
				for (int i = 0; i < accelX.Length; i++)
				{
					accelX[i] = (npc.oldPos[i].X + npc.oldPos[i + 2].X) - (2 * npc.oldPos[i + 1].X);
				}
				float[] dAccelX = new float[accelX.Length - 1];
				for (int j = 0; j < dAccelX.Length; j++)
				{
					dAccelX[j] = accelX[j] - accelX[j + 1];
				}
				float trendX = dAccelX.Sum();
				int signedVel = MathF.Sign(npc.velocity.X);
				if (signedVel == 0)
					signedVel = MathF.Sign(npc.oldVelocity.X);
				float scaleX = 1 + MathHelper.Clamp(trendX * -2 * MathF.Sign(npc.velocity.X), -0.25f, 0.33f);
				float scaleY = 1;// + MathHelper.Clamp(MathF.Abs(npc.velocity.Y) * 0.2f, 0, 0.33f);

				Vector2 drawPos = npc.position - screenPos;
				//drawPos.X -= npc.width * (scaleX - 1);
				if (signedVel > 0)
					drawPos.X -= npc.width * (scaleX - 1);
				spritebatch.Draw(Terraria.GameContent.TextureAssets.Npc[npc.type].Value, drawPos, npc.frame, lightColor, npc.rotation, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
				return false;
			}
			if (npc.ai[1] == Jump)
			{
				float[] oldVelY = new float[npc.oldPos.Length - 1];
				for (int i = 0; i < oldVelY.Length; i++)
				{
					oldVelY[i] = npc.oldPos[i].Y - npc.oldPos[i + 1].Y;
				}
				float avgVelY = oldVelY.Sum() / (float)oldVelY.Length;
				if (npc.collideY)
					avgVelY *= 0.25f;
				float scaleY = 1 + MathHelper.Clamp(MathF.Abs(avgVelY) * 0.33f, 0, 0.33f);
				float scaleX = 1f;

				Vector2 drawPos = npc.position - screenPos;
				drawPos.Y -= npc.height * (scaleY - 1);
				spritebatch.Draw(Terraria.GameContent.TextureAssets.Npc[npc.type].Value, drawPos, npc.frame, lightColor, npc.rotation, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
				return false;
			}

			return base.PreDraw(npc, spritebatch, screenPos, lightColor);
		}
	}
}
