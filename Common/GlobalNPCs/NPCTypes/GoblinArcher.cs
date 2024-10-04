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

		const int ApproachPlayer = 0;
		const int Jump = 1;
		const int FireArrows = 2;

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest();
			switch (npc.Phase())
			{
				case ApproachPlayer:
					ApproachPlayerAI(npc);
					break;
				case Jump:
					JumpAI(npc);
					break;
				case FireArrows:
					FireArrowsAI(npc);
					break;
				default:
					npc.Phase(ApproachPlayer);
					break;
			}
		}

		void ApproachPlayerAI(NPC npc) //No hitbox when walking
		{
			Player target = Main.player[npc.target];
			int directionToMove = target.position.X < npc.position.X ? -1 : 1;
			Vector2 distance = new Vector2(MathF.Abs(target.position.X - npc.position.X), MathF.Abs(target.position.Y - npc.position.Y));
			if ((distance.X < 240 && 80 < distance.X) || distance.Y > 480 || (distance.Y > 240 && distance.X < 80))
			{
				if (npc.LineOfSight(target.position))
				{
					npc.Phase(FireArrows);
					return;
				}
			}
			if (distance.X < 60 && distance.Y < 80)
			{
				npc.Phase(Jump);
				return;
			}
			if (MathF.Abs(npc.velocity.X) < 2f)
			{
				npc.velocity.X += directionToMove * 0.075f; //Still very fast accel, but not instantaneous
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
			if (npc.Grounded())
			{
				if (npc.Timer() == 0)
				{
					npc.velocity.Y -= 5.5f;
				}
				else
				{
					npc.Phase(ApproachPlayer);
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
				for (int i = 0; i < 3; i++)
				{
					Dust d = Dust.NewDustDirect(npc.Center, 2, 2, Terraria.ID.DustID.Torch);
					d.noGravity = true;
					d.scale = Main.rand.NextFloat(1.4f, 2f);
					d.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * (4 - d.scale);
				}
			}
			else if (time > 90)
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
				npc.Phase(ApproachPlayer);
				return;
			}
			npc.velocity.X *= 0.9f;
			npc.Timer(time + 1);
		}
	}
}
