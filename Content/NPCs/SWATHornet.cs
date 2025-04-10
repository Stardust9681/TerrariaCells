using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Content.NPCs
{
    public class SWATHornet : ModNPC
    {
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 3;
		}
		public override void SetDefaults()
		{
			NPC.lifeMax = 40;
			NPC.damage = 30;
			NPC.knockBackResist = 0.8f;
			NPC.noGravity = true;
			NPC.width = 50;
			NPC.height = 36;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
		}
		private ref float Timer => ref NPC.ai[0];
		public override void AI()
		{
			const float Speed = 2.6f;

			//Will only target one player at a time
			if (!NPC.HasValidTarget)
				NPC.TargetClosest(false);

			if (!NPC.TargetInAggroRange(24 * 16))
			{
				if (!NPC.dontTakeDamage)
				{
					NPC.dontTakeDamage = true;
					NPC.netUpdate = true;
				}

				if (MathF.Abs(NPC.velocity.X) < 0.2f)
				{
					NPC.velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * Speed;
				}

				if (NPC.collideY)
					NPC.velocity.Y = -NPC.oldVelocity.Y * 0.8f;
				if (NPC.collideX)
					NPC.velocity.X = -NPC.oldVelocity.X * 0.8f;

				if (NPC.velocity.LengthSquared() < (Speed * Speed))
				{
					NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.velocity.SafeNormalize(Vector2.Zero) * Speed, 0.08f);
				}
				Timer = 0;
				return;
			}
			if (NPC.TryGetTarget(out Entity target))
			{
				//Initial follow to near position
				if (Timer < 60)
				{
					Vector2 followOffset = target.DirectionTo(NPC.Center) * 15 * 16;
					Vector2 followPos = target.Center + followOffset;

					NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(followPos.X - NPC.position.X) * Speed, 0.15f);
					NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, Math.Sign(followPos.Y - NPC.position.Y) * Speed, 0.15f);

					if (NPC.ai[2] == 0)
					{
						NPC.ai[2] = Main.rand.Next(-24, 24);
						NPC.netUpdate = true;
					}
				}
				//Get in place to dash
				else if (Timer < 90)
				{
					NPC.ai[1] = Math.Sign(NPC.position.X - target.Center.X);
					Vector2 followOffset = new Vector2(NPC.ai[1] * 15 * 16, NPC.ai[2]);
					Vector2 followPos = target.Center + followOffset;

					NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(followPos.X - NPC.position.X) * Speed, 0.33f);
					NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, Math.Sign(followPos.Y - NPC.position.Y) * Speed, 0.33f);
				}
				//Perform dash movement
				else if (Timer < 170)
				{
					Vector2 offset = new Vector2(NPC.ai[1] * 15 * 16, 0);
					Vector2 start = target.Center + offset;
					Vector2 end = target.Center - offset;

					int timer = (int)Timer - 90;
					if (timer < 20)
					{
						NPC.velocity.X = MathHelper.Lerp(4.5f * NPC.ai[1], 0, timer / 15f);
						NPC.rotation = MathHelper.ToRadians(NPC.velocity.X * 3);
						NPC.direction = NPC.spriteDirection = (int)-NPC.ai[1];
					}
					else if (timer < 60)
					{
						NPC.velocity.X = MathHelper.Lerp((end.X - start.X) / 30f, 0, MathF.Abs(timer - 15 - 20) / 40f);
						NPC.rotation = MathHelper.ToRadians(NPC.velocity.X);
						NPC.direction = NPC.spriteDirection = (int)-NPC.ai[1];

						Common.GlobalNPCs.CombatNPC.ToggleContactDamage(NPC, true);

						if (Main.rand.NextBool(3))
						{
							Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Bee);
							d.velocity = NPC.velocity * .5f;
							d.noGravity = true;
						}
					}
					else
					{
						NPC.velocity.X = MathHelper.Lerp(0, 4.5f * NPC.ai[1], (timer - 60) / 20f);
						NPC.rotation = MathHelper.ToRadians(NPC.velocity.X * 3);
						NPC.direction = NPC.spriteDirection = (int)NPC.ai[1];

						Common.GlobalNPCs.CombatNPC.ToggleContactDamage(NPC, false);
					}
				}
				//Basic follow after dash attack
				else if (Timer < 200)
				{
					Vector2 followOffset = target.DirectionTo((NPC.Center + new Vector2(0, -3f))) * 15 * 16;
					Vector2 followPos = target.Center + followOffset;

					NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, Math.Sign(followPos.X - NPC.position.X) * Speed, 0.15f);
					NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, Math.Sign(followPos.Y - NPC.position.Y) * Speed, 0.15f);
				}
				//Shoot bullet(s) and delay
				else if (Timer < 260)
				{
					if (NPC.dontTakeDamage)
					{
						NPC.dontTakeDamage = false;
						NPC.netUpdate = true;
					}
					if (Timer == 200)
					{
						NPC.ai[1] = Math.Sign(target.position.X - NPC.position.X);
						for (int i = 0; i < 2; i++)
						{
							float rotation = MathHelper.ToRadians(15 * ((i - 1) * 2 + 1));
							Vector2 velocity = NPC.DirectionTo(target.Center).RotatedBy(rotation) * 2.4f;
							Projectile proj = Projectile.NewProjectileDirect(
								NPC.GetSource_FromAI(),
								NPC.Center + (velocity * 2),
								velocity,
								ProjectileID.Bullet,
								Common.Utilities.TCellsUtils.ScaledHostileDamage(20),
								1f,
								Main.myPlayer);
							proj.hostile = true;
							proj.friendly = false;
							proj.netUpdate = true;
						}
					}
					int timer = (int)Timer - 200;
					NPC.velocity.X = MathHelper.Lerp(-NPC.ai[1], 0, timer / 60f);
					NPC.velocity.Y = MathHelper.Lerp(1f, 0, MathF.Abs(timer - 30) / 30f);
					NPC.rotation = NPC.ai[1] * MathHelper.ToRadians(timer * 0.8f);
				}
				else
				{
					Timer = 0;
				}
			}

			Timer++;
		}
		public override bool? CanFallThroughPlatforms()
		{
			return true;
		}
		public override void FindFrame(int frameHeight)
		{
			int frameRate = 8;
			if (Timer > 90 && Timer < 170)
				frameRate = 10;
			if (Timer > 200 && Timer < 260)
				frameRate = 4;

			NPC.frameCounter++;
			if (NPC.frameCounter > 60f/frameRate)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y += frameHeight;
				if (NPC.frame.Y > (Main.npcFrameCount[NPC.type]-1) * frameHeight)
					NPC.frame.Y = 0;
			}
		}
	}
}
