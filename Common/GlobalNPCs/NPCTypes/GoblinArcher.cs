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

		const int Idle = 0;
		const int ApproachTarget = 1;
		const int Jump = 2;
		const int FireArrows = 3;

		const float MaxSpeed = 2f;
		const float Accel = 0.075f;
		const float JumpStrength = 5.5f;

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest(false);

			//This gets continually recalculated, so I need to continually reset it :(
			npc.stairFall = false;

			switch (npc.Phase())
			{
				case Idle:
					IdleAI(npc);
					break;
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
					npc.Phase(Idle);
					break;
			}
		}

		void IdleAI(NPC npc)
		{
			if (npc.TargetInAggroRange())
			{
				npc.Phase(ApproachTarget);
				return;
			}

			npc.direction = MathF.Sign(npc.ai[1]);

			float newVel = npc.velocity.X + (npc.direction * Accel);
			if (npc.direction == 0)
			{
				newVel = npc.velocity.X + (npc.spriteDirection * Accel);
				npc.direction = MathF.Sign(newVel);
			}
			const float IdleMaxSpeed = MaxSpeed * 0.67f;
			if (MathF.Abs(newVel) < IdleMaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = npc.direction * IdleMaxSpeed;

			if (npc.collideX)
			{
				Vector2 oldPos = npc.position;
				Vector2 oldVel = npc.velocity;
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if (npc.position.Equals(oldPos))
				{
					npc.position -= npc.oldVelocity * 2;
					npc.ai[1] = -npc.direction;
					npc.Phase(Idle);
					return;
				}
				else
				{
					if (MathF.Abs(npc.velocity.X) < MathF.Abs(oldVel.X))
					{
						npc.velocity.X = (npc.velocity.X + oldVel.X) * 0.5f;
					}
					npc.DoTimer(-3);
				}
			}

			Vector2 nextGround = npc.FindGroundInFront();
			if (npc.Grounded() && nextGround.Y > (npc.Bottom.Y + npc.height))
			{
				npc.velocity.X *= 0.5f;
				npc.ai[1] = -npc.direction;
				npc.Phase(Idle);
				return;
			}

			if (npc.Timer() > 150)
			{
				npc.ai[1] = -npc.direction;
				npc.Phase(Idle);
				return;
			}

			npc.velocity.Y += 0.036f; //Apply gravity
			npc.DoTimer();
		} //Hitbox doesn't matter, target not near enough to take contact damage
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

			//npc.stairFall = target.position.Y > npc.position.Y;

			float newVel = npc.velocity.X + (directionToMove * Accel);
			if (MathF.Abs(newVel) < MaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = npc.direction * MaxSpeed;

			if (npc.FindGroundInFront().Y > (npc.Bottom.Y + npc.height))
			{
				if (!npc.TargetInAggroRange())
				{
					npc.ai[1] = -npc.direction;
					npc.Phase(Idle);
				}
				else if (npc.LineOfSight(target.position))
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
			npc.DoTimer();
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
					npc.Phase(Idle);
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
			npc.DoTimer();
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

				if (npc.TargetInAggroRange())
				{
					npc.Phase(ApproachTarget);
				}
				else
				{
					npc.ai[1] = -npc.direction;
					npc.Phase(Idle);
				}
				return;
			}
			npc.velocity.X *= 0.9f;
			npc.DoTimer();
		}
	}
}
