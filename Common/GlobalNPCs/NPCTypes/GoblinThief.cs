using System;
using Terraria;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	/// <remarks>
	/// Also used for <see cref="Terraria.ID.NPCID.Wolf"/>
	/// </remarks>
	public class GoblinThief : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType is Terraria.ID.NPCID.GoblinThief or Terraria.ID.NPCID.Wolf;
		}

		const int ApproachTarget = 0;
		const int Jump = 1;

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
				default:
					npc.Phase(0);
					break;
			}
		}

		void ApproachTargetAI(NPC npc) //No hitbox when walking
		{
			if (npc.Timer() == 0)
				CombatNPC.ToggleContactDamage(npc, false);
			Player target = Main.player[npc.target];
			npc.direction = npc.velocity.X < 0 ? -1 : 1;
			Vector2 distance = new Vector2(MathF.Abs(target.position.X - npc.position.X), MathF.Abs(target.position.Y - npc.position.Y));
			if (
				MathF.Abs(npc.velocity.X) > 2.8f
				&& npc.IsFacingTarget(target)
				&& ((96 < distance.X && distance.X < 128) || (128 < distance.X && distance.X < 160 && distance.Y > 48)))
			{
				npc.Phase(Jump);
				return;
			}

			int directionToMove = target.position.X < npc.position.X ? -1 : 1;
			if (npc.Timer() < 60 && !npc.IsFacingTarget(target) && distance.X < 80)
				directionToMove *= -1;

			const float MaxSpeed = 4f;
			const float Accel = 0.1f;
			float newVel = npc.velocity.X + directionToMove * Accel;
			if (MathF.Abs(newVel) < MaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = npc.direction * MaxSpeed;

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
					npc.velocity.Y -= 7.2f;
				}
				else
				{
					npc.Phase(ApproachTarget);
					return;
				}
			}
			if (MathF.Abs(npc.velocity.X) < 2.4f)
				npc.velocity.X += npc.direction * 0.024f;
			if (!npc.IsFacingTarget() && npc.velocity.Y > 0)
			{
				npc.velocity.Y *= 1.014f;
				npc.velocity.X *= 0.996f;
			}
			npc.velocity.Y += 0.024f;
			npc.Timer(npc.Timer() + 1);
		}
	}
}
