using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static TerrariaCells.Common.Utilities.NPCHelpers;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	public class RockGolem : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType == NPCID.RockGolem;
		}

		//Can have up to 6 floating rocks summoned at a time (as a ranged resource)
		//Can turn into standard boulder (regains all rocks)
			//Explodes out of boulder upon hitting a wall (delay)
		//Can expend rocks when player is distant
			//Throw 1 rock with 1 hand (1 Rock)
			//Throw 2 rocks with 2 hands (2 Rock)
			//Roll boulder with 2 hands (3 Rock)

		const int Idle = 0;
		const int Roll = 1;
		const int Delay = 2;
		const int ThrowHands = 3;

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest(false);
			if (!npc.HasValidTarget)
			{
				IdleAI(npc);
				return;
			}

			switch ((int)npc.ai[1])
			{
				case Idle:
					IdleAI(npc);
					break;
				case Roll:
					RollAI(npc);
					break;
				case Delay:
					DelayAI(npc);
					break;
				case ThrowHands:
					ThrowAI(npc);
					break;
			}
		}

		private void IdleAI(NPC npc)
		{
			if (!npc.TryGetTarget(out Entity target))
			{
				npc.ai[0] = 0;
				npc.ai[1] = Idle;
				npc.ai[2] = 0;
				npc.ai[3] = 0;
				return;
			}

			if (!npc.dontTakeDamage)
			{
				npc.dontTakeDamage = true;
				npc.netUpdate = true;
			}

			npc.rotation = 0;
			npc.velocity.X *= 0.99f;
			npc.velocity.Y += 0.14f;

			if (npc.TargetInAggroRange(30 * 16, allowDamageTrigger: false))
			{
				npc.ai[1] = Roll;
				npc.ai[0] = 0;
				npc.ai[3] = MathF.Sign(target.position.X - npc.position.X);
				return;
			}
		}

		private void RollAI(NPC npc)
		{
			if (!npc.TryGetTarget(out Entity target))
			{
				npc.ai[0] = 0;
				npc.ai[1] = Idle;
				npc.ai[2] = 0;
				npc.ai[3] = 0;
				npc.height = 74;
				npc.position.Y -= 42;
				npc.netUpdate = true;
				return;
			}

			if (npc.ai[0] == 0)
			{
				npc.ai[2] = 5;
				npc.height = 32;
				npc.position.Y += 42;
				npc.netUpdate = true;
			}
			npc.ai[0]++;

			npc.velocity.Y += 0.14f;
			if (npc.dontTakeDamage)
			{
				CombatNPC.ToggleContactDamage(npc, true);

				Vector2 vel1 = npc.oldVelocity;
				Collision.StepUp(ref npc.position, ref vel1, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				npc.velocity = vel1;

				if (npc.collideX || npc.ai[0] > 45)
				{
					if (MathF.Abs(npc.velocity.X) < 0.3f)
					{
						npc.ai[1] = Delay;
						npc.ai[0] = 0;
						npc.velocity.Y -= 6f;
						npc.height = 74;
						npc.position.Y -= 42;
						npc.netUpdate = true;
						return;
					}
				}

				float xDist = npc.position.X - target.position.X;
				xDist *= npc.ai[3];
				if (xDist < 14 * 16)
					npc.velocity.X = MathHelper.Lerp(npc.velocity.X, 6f * npc.ai[3], 0.01f);
				else
					npc.velocity.X *= 0.9f;

				npc.rotation += MathHelper.ToRadians(npc.velocity.X) * 4f;
			}
			else if (npc.ai[0] > 15)
			{
				npc.velocity.X *= 0.99f;
				npc.dontTakeDamage = true;
				npc.netUpdate = true;
				return;
			}
		}

		private void DelayAI(NPC npc)
		{
			if (!npc.TryGetTarget(out Entity target))
			{
				npc.ai[0] = Idle;
				npc.ai[1] = 0;
				npc.ai[2] = 0;
				npc.ai[3] = 0;
				return;
			}

			npc.rotation = 0;
			npc.velocity.X = 0;
			npc.velocity.Y += 0.14f;
			npc.ai[0]++;

			if (npc.dontTakeDamage)
			{
				npc.dontTakeDamage = false;
				CombatNPC.ToggleContactDamage(npc, false);
			}

			int direction = MathF.Sign(target.position.X - npc.position.X);
			npc.spriteDirection = npc.direction = direction;
			float xOffset = -direction * 20 * 16;
			int dirToOffset = MathF.Sign(target.position.X + xOffset - npc.position.X);
			npc.velocity.X = MathHelper.Lerp(npc.velocity.X, dirToOffset * 5f, 0.1f);
			if (npc.ai[0] > 40)
				npc.velocity.X *= 0.6f;

			if (npc.ai[0] > 1)
			{
				Vector2 vel1 = npc.oldVelocity;
				Collision.StepUp(ref npc.position, ref vel1, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				npc.velocity = vel1;
			}

			if (npc.ai[0] > 60)
			{
				npc.ai[1] = ThrowHands;
				npc.ai[0] = 0;
				return;
			}
		}

		private void ThrowAI(NPC npc)
		{
			if (!npc.TryGetTarget(out Entity target))
			{
				npc.ai[1] = Idle;
				return;
			}

			npc.velocity.X *= 0.9f;
			npc.velocity.Y += 0.014f;

			if (npc.ai[0] == 0)
			{
				if (npc.ai[2] > 2 && MathF.Abs(npc.Bottom.Y - target.Bottom.Y) < 2 * 16)
					//npc.ai[3] = Main.rand.Next(2) + 2; //Pick from 2 and 3
					npc.ai[3] = 3;
				else if (npc.ai[2] > 1)
					npc.ai[3] = Main.rand.Next(2) + 1; //Pick from 1 and 2
				else if (npc.ai[2] > 0)
					npc.ai[3] = 1; //Pick from 1
				else
				{
					npc.ai[0] = 0;
					npc.ai[1] = Roll;
					npc.ai[2] = 0;
					npc.ai[3] = MathF.Sign(target.position.X - npc.position.X);
					return;
				}
			}
			npc.ai[0]++;

			const float RockSpeed = 10f;
			const float RockSpread_Deg = 10f;
			switch ((int)npc.ai[3])
			{
				//Throw 1 rock
				case 1:
					//3 frame wind-up (15-17)
					//3 frame follow-through (18-20)
					if (npc.ai[0] == 24)
					{
						Vector2 vel = npc.DirectionTo(target.Center - new Vector2(0, MathF.Abs(npc.position.X - target.position.X) * 0.1f)).RotatedByRandom(MathHelper.ToRadians(RockSpread_Deg)) * RockSpeed;
						Projectile proj = Projectile.NewProjectileDirect(
							npc.GetSource_FromAI(),
							npc.Center + vel,
							vel,
							ProjectileID.RockGolemRock,
							Common.Utilities.TCellsUtils.ScaledHostileDamage(45),
							1f,
							Main.myPlayer
						);
					}
					//12 ticks delay (0)
					if(npc.ai[0] > 36)
					{
						npc.ai[2]--;
						npc.ai[0] = 0;
						return;
					}
					break;
				//Throw 2 Rocks
				case 2:
					//3 frame wind-up (10-12)
					//2 frame follow-through (13-14)
					if (npc.ai[0] == 40 && npc.ai[2] > 1)
					{
						npc.ai[2] -= 2;
						Vector2 vel = npc.DirectionTo(target.Center - new Vector2(0, MathF.Abs(npc.position.X - target.position.X) * 0.1f)) * RockSpeed;
						for (int i = 0; i < 2; i++)
						{
							Projectile proj = Projectile.NewProjectileDirect(
								npc.GetSource_FromAI(),
								npc.Center + vel,
								vel.RotatedByRandom(MathHelper.ToRadians(3 * RockSpread_Deg)),
								ProjectileID.RockGolemRock,
								Common.Utilities.TCellsUtils.ScaledHostileDamage(30),
								1f,
								Main.myPlayer
							);
						}
					}
					//20 ticks delay (0)
					if (npc.ai[0] > 60)
					{
						npc.ai[0] = 0;
						return;
					}
					break;
				//Roll Boulder (3 Rocks)
				case 3:
					//3 frame summon (9-11)
					if (npc.ai[0] == 8 && npc.ai[2] > 2)
					{
						Vector2 vel = new Vector2(MathF.Sign(target.position.X - npc.position.X) * 4, -3f);
						Projectile proj = Projectile.NewProjectileDirect(
							npc.GetSource_FromAI(),
							npc.Center + vel,
							vel,
							ProjectileID.Boulder,
							Common.Utilities.TCellsUtils.ScaledHostileDamage(60),
							1f,
							Main.myPlayer
						);
						proj.friendly = false;
						proj.netUpdate = true;
						npc.ai[2] -= 3;
					}

					//3 frame wind-down (11-9)
					if (npc.ai[0] > 60)
					{
						npc.ai[0] = 0;
						return;
					}
					break;
			}
		}

		public override bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor)
		{
			ReLogic.Content.Asset<Texture2D> boulder = TextureAssets.Projectile[ProjectileID.Boulder];
			switch ((int)npc.ai[1])
			{
				case Idle:
					spritebatch.Draw(boulder.Value, npc.Bottom - screenPos - new Vector2(0, boulder.Height()*0.5f), null, lightColor, npc.rotation, boulder.Size() * 0.5f, 1f, SpriteEffects.None, 0);
					return false;
				case Roll:
					if (!npc.dontTakeDamage) return base.PreDraw(npc, spritebatch, screenPos, lightColor);
					spritebatch.Draw(boulder.Value, npc.Bottom - screenPos - new Vector2(0, boulder.Height() * 0.5f), null, lightColor, npc.rotation, boulder.Size()*0.5f, 1f, SpriteEffects.None, 0);
					return false;
			}
			return base.PreDraw(npc, spritebatch, screenPos, lightColor);
		}

		public override bool FindFrame(NPC npc, int frameHeight)
		{
			if (npc.ai[1] == Delay)
			{
				npc.frameCounter += MathF.Sign(npc.velocity.X) * npc.direction;
				int frameNum = (int)npc.frameCounter / 6;
				if (frameNum < 0)
				{
					npc.frameCounter += 8 * 6;
					frameNum = (int)npc.frameCounter / 6;
				}
				frameNum %= 8;
				npc.frame.Y = frameNum * frameHeight;
			}
			if (npc.ai[1] == ThrowHands)
			{
				int frameNum = -1;
				switch ((int)npc.ai[3])
				{
					case 1:
						if (npc.ai[0] < 24)
						{
							frameNum = (int)npc.ai[0] / 6;
							frameNum += 15;
						}
						else
						{
							if (npc.ai[0] < 36)
							{
								frameNum = (int)(npc.ai[0] - 24) / 6;
								frameNum += 19;
							}
							else
							{
								frameNum = 0;
							}
						}
						break;
					case 2:
						if (npc.ai[0] < 40)
						{
							frameNum = (int)npc.ai[0] / 14;
							frameNum += 10;
						}
						else
						{
							if (npc.ai[0] < 50)
							{
								frameNum = (int)(npc.ai[0] - 40) / 5;
								frameNum += 13;
							}
							else
								frameNum = 0;
						}
						break;
					case 3:
						//3 frame summon (9-11)
						//if (npc.ai[0] == 24 && npc.ai[2] > 2)
						//3 frame wind-down (11-9)
						//if (npc.ai[0] > 60)
						if (npc.ai[0] < 24)
						{
							frameNum = (int)npc.ai[0] / 8;
							frameNum += 9;
						}
						else
						{
							frameNum = Math.Max(3 - ((int)(npc.ai[0]-24) / 8), 0);
							frameNum += 9;
						}
						break;
				}
				npc.frame.Y = frameNum * frameHeight;
			}
			return false;
		}
	}
}
