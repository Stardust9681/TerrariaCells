using System;
using Terraria;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	public class GoblinArcher : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType.Equals(Terraria.ID.NPCID.GoblinArcher);
		}

		const int ApproachTarget = 0;
		const int Jump = 1;
		const int FireArrows = 2;

		const float MaxSpeed = 2f;
		const float Accel = 0.075f;
		const float JumpStrength = 5.5f;

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest();
			switch (npc.Phase())
			{
				case ApproachTarget:
					ApproachTargetAI(npc);
					break;
				case Jump:
					JumpAI(npc);
					break;
				case FireArrows:
					FireArrowsAI(npc);
					break;
				default:
					npc.Phase(ApproachTarget);
					break;
			}
		}

		void ApproachTargetAI(NPC npc) //No hitbox when walking
		{
			if (npc.Timer() == 0)
				CombatNPC.ToggleContactDamage(npc, false);
			Player target = Main.player[npc.target];
			int directionToMove = target.position.X < npc.position.X ? -1 : 1;
			Vector2 distance = new Vector2(MathF.Abs(target.position.X - npc.position.X), MathF.Abs(target.position.Y - npc.position.Y));
			float additiveDist = distance.X + distance.Y;
			//Within ~5 tiles or between 15-20 tiles away, try to fire arrows instead
			if(additiveDist < 80 || (240 < additiveDist && additiveDist < 320))
			{
				if (npc.LineOfSight(target.position))
				{
					npc.Phase(FireArrows);
					return;
				}
			}

			npc.stairFall = target.position.X > npc.position.X;

			float newVel = npc.velocity.X + directionToMove * Accel;
			if (MathF.Abs(newVel) < MaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = npc.direction * MaxSpeed;

			if (npc.FindGroundInFront().Y > (npc.Bottom.Y + (npc.height * 2)))
			{
				if (npc.LineOfSight(target.position))
				{
					npc.velocity.X = 0;
					npc.Phase(FireArrows);
				}
				else
				{
					npc.Phase(Jump);
				}
				return;
			}

			if (npc.collideX)
			{
				//Vector2 oldPos = npc.position;
				//Vector2 oldVel = npc.velocity;
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if (npc.Grounded())
				{
					npc.Phase(Jump);
					return;
				}
			}
			npc.velocity.Y += 0.036f; //Apply gravity
			npc.Timer(npc.Timer() + 1);
		}
		void JumpAI(NPC npc) //Hitbox when jumping
		{
			if (npc.Timer() == 0)
				CombatNPC.ToggleContactDamage(npc, true);
			if (npc.Grounded())
			{
				if (npc.Timer() == 0)
				{
					npc.velocity.Y -= JumpStrength;
				}
				else
				{
					npc.Phase(ApproachTarget);
					return;
				}
			}
			if (npc.collideX)
			{
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
			}
			if (MathF.Abs(npc.velocity.X) < 1.4f)
				npc.velocity.X += npc.direction * 0.014f;
			npc.velocity.Y += 0.036f;
			npc.Timer(npc.Timer() + 1);
		}
		void FireArrowsAI(NPC npc) //No hitbox when firing arrows
		{
			if (npc.Timer() == 0)
				CombatNPC.ToggleContactDamage(npc, false);
			Player target = Main.player[npc.target];
			
			int time = npc.Timer();
			if (50 < time && time < 55)
			{
				//Lossy compression into only ai[3] because for some reason ai[2] turns the archer INVISIBLE when used ???
				//Didn't want projectile fired DIRECTLY at player, so there's some opportunity to respond
				npc.ai[3] = Pack(target.Center.ToTileCoordinates16());
				for (int i = 0; i < 5; i++)
				{
					Dust d = Dust.NewDustDirect(npc.Center + new Vector2(0, -4), 4, 4, Terraria.ID.DustID.Torch);
					d.noGravity = true;
					d.scale = Main.rand.NextFloat(1.6f);
					d.velocity = npc.DirectionTo(target.Center) * (3.5f - d.scale) * i;
					d.velocity = d.velocity.RotatedByRandom(MathHelper.ToRadians(15));
				}
			}
			else if (time > 75)
			{
				//Just add new projectile types in if you want to adjust this I guess I dunno
				int[] arrowsToFire = new int[] { Terraria.ID.ProjectileID.WoodenArrowHostile, Terraria.ID.ProjectileID.FireArrow };

				int xy = (int)npc.ai[3];
				(ushort x, ushort y) = Common.Utilities.NPCHelpers.Unpack(xy);
				Vector2 vel = new Vector2(x * 16 + 8, y * 16 + 8);
				vel = npc.DirectionTo(vel) * 8.5f;
				vel.Y *= 1.075f;
				vel.Y += -MathF.Abs(vel.X * 0.08f);
				Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, vel, Main.rand.Next(arrowsToFire), npc.damage / 5, 1f, Main.myPlayer);
				proj.hostile = true;
				proj.friendly = false;
				npc.Phase(ApproachTarget);
				return;
			}
			npc.velocity.X *= 0.9f;
			npc.DoTimer();
		}
	}
}
