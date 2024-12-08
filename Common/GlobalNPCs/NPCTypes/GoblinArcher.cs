﻿using System;
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
			if ((128 < distance.X && distance.X < 320) || distance.Y > 480 || (distance.Y > 240 && distance.X < 80))
			{
				if (npc.LineOfSight(target.position))
				{
					npc.Phase(FireArrows);
					return;
				}
			}

			const float MaxSpeed = 2f;
			const float Accel = 0.075f;
			float newVel = npc.velocity.X + directionToMove * Accel;
			if (MathF.Abs(newVel) < MaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = npc.direction * MaxSpeed;

			if (npc.FindGroundInFront().Y > (npc.Bottom.Y + (npc.height * 2)))
			{
				npc.velocity.X = 0;
				npc.Phase(FireArrows);
				return;
			}

			if (npc.collideX)
			{
				Vector2 oldPos = npc.position;
				Vector2 oldVel = npc.velocity;
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if (oldPos == npc.position && oldVel == npc.velocity && npc.Grounded())
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
					npc.velocity.Y -= 5.5f;
				}
				else
				{
					npc.Phase(ApproachTarget);
					return;
				}
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
			//Just add new projectile types in if you want to adjust this I guess I dunno
			int[] ArrowsToFire = new int[] { Terraria.ID.ProjectileID.WoodenArrowHostile, Terraria.ID.ProjectileID.FireArrow };
			int time = npc.Timer();
			if (40 < time && time < 45)
			{
				//Lossy compression because for some reason 'npc.ai[2]' turns the archer INVISIBLE when used ???
				//Couldn't find anything elsewhere that would cause this
				//Didn't want projectile fired DIRECTLY at player, so there's some opportunity to respond
				(int x, int y) = ((int)(target.Center.X) / 16, (int)(target.Center.Y) / 16);
				npc.ai[3] = (x << 16) | y;
				for (int i = 0; i < 4; i++)
				{
					Dust d = Dust.NewDustDirect(npc.Center + new Vector2(0, -4), 4, 4, Terraria.ID.DustID.Torch);
					d.noGravity = true;
					d.scale = Main.rand.NextFloat(1.6f);
					d.velocity = npc.DirectionTo(target.Center) * (3.5f - d.scale) * i;
					d.velocity = d.velocity.RotatedByRandom(MathHelper.ToRadians(15));
				}
			}
			else if (time > 60)
			{
				int xy = (int)npc.ai[3];
				(int x, int y) = ((xy & (int.MaxValue << 16)) >> 16, xy & (int.MaxValue >> 16));
				Vector2 vel = new Vector2(x * 16, y * 16);
				vel = npc.DirectionTo(vel) * 8;
				vel.Y *= 1.075f;
				vel.Y += -MathF.Abs(vel.X * 0.1f);
				Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, vel, Main.rand.Next(ArrowsToFire), npc.damage / 5, 1f, Main.myPlayer);
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
